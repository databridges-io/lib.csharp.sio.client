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

using dBridges.channel;
using dBridges.remoteprocedure;
using System.Threading.Tasks;

namespace dBridges.privateAccess
{
 public   class CprivateResponse
    {
        private string name;
        private object  rcCore;
        private int object_type;
        private string sid;
        private int class_type;

        public CprivateResponse(int object_type, int class_type,   string name, string sid, object rcCore)
        {
            this.name = name;
            this.rcCore = rcCore;
            this.object_type = object_type;
            this.class_type = class_type;
            this.sid = sid;
        }

       

        public   async Task end(CPrivateInfo data)
        {
            if (this.object_type == 0)
                  await (this.rcCore as station).send_to_dbr(this.class_type, this.name, this.sid, data);
            else
                await (this.rcCore as CRpc).send_to_dbr(this.class_type, this.name, this.sid, data);
        }


        public  async Task exception(int errorcode, string errormessage)
        {
            if (errorcode == 0) errorcode = 9;

            CPrivateInfo pinfo = new CPrivateInfo(errorcode, errormessage, "");

            if (this.object_type == 0 )
                 await (this.rcCore as station).send_to_dbr(this.class_type, this.name, this.sid, pinfo);
            else
                 await (this.rcCore as CRpc).send_to_dbr(this.class_type, this.name, this.sid, pinfo);
        }


    }
}
