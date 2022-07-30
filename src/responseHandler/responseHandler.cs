/*
	DataBridges C# client Library
	https://www.databridges.io/



	Copyright 2022 Optomate Technologies Private Limited.

	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

	    http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dBridges.Utils;
using dBridges.Messages;
using dBridges.exceptions;
using System.Threading.Tasks;
using System.Threading;

namespace dBridges.responseHandler
{
    public class CResponseHandler
    {
        private string functionName;
        private string returnSubect;
        private string sid;
        private object dbcore;
        private bool isend;
        public readonly string id;
        public bool tracker;
        private string m_type;
        readonly SemaphoreSlim _rpc_cf_response_Lock;

        public CResponseHandler(string functionName, string returnSubect, string sid, object dbcoreobject, string mtypes)
        {
            this.functionName = functionName;
            this.returnSubect = returnSubect;
            this.sid = sid;
            this.dbcore = dbcoreobject;
            this.isend = false;
            this.id = returnSubect;
            this.tracker = false;
            this.m_type = mtypes;
            this._rpc_cf_response_Lock = new SemaphoreSlim(1, 1);


        }

        private async Task  send_data(string data, bool _isend,  string txt=null)
        {
            bool cstatus = false;
            if (!this.isend)
            {
                this.isend = _isend;

                if (this.m_type == "rpc")
                {
                                            
                    cstatus = await util.updatedBNewtworkCF(this.dbcore, MessageType.RPC_CALL_RESPONSE, null,
                        this.returnSubect, txt, this.sid, data, this.isend,
                        this.tracker);
                }
                else
                {
                    
                    cstatus = await util.updatedBNewtworkCF(this.dbcore, MessageType.CF_CALL_RESPONSE, null,
                        this.returnSubect, txt, this.sid , data, this.isend, this.tracker);
                }

                if (!cstatus)
                {
                    if (this.m_type == "rpc")
                    {
                        throw new dBError("E079"); 
                    }
                    else
                    {
                        throw new dBError("E068");
                    }
                }
            }
            else
            {
                if (this.m_type == "rpc")
                {
                    throw new dBError("E106");
                }
                else
                {
                    throw new dBError("E105");
                }
            }

        }


        public async Task next(string data)
        {
            try
            {
                await this._rpc_cf_response_Lock.WaitAsync().ConfigureAwait(false);
                await this.send_data(data, false, "");
            }
            finally
            {
                this._rpc_cf_response_Lock.Release();
            }

        }

        public async Task end(string data)
        {
            try
            {
                await this._rpc_cf_response_Lock.WaitAsync().ConfigureAwait(false);
                await this.send_data(data, true, "");
            }
            finally
            {
                this._rpc_cf_response_Lock.Release();
            }

        }

        public async Task exception(string expCode, string expShortMessage)
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            { {"c" ,  expCode},  {"m" , expShortMessage} };

            try {
                await this._rpc_cf_response_Lock.WaitAsync().ConfigureAwait(false);
                await this.send_data(util.DictionaryToString(data), true, "EXP");
            }
            finally
            {
                this._rpc_cf_response_Lock.Release();
            }
        }





    }
}
