﻿/*
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
using System.Threading.Tasks;

using dBridges.dispatchers;
using dBridges.exceptions;
using dBridges.Messages;
using dBridges.Utils;
using RSG;
using dBridges.remoteprocedure;
using dBridges.events;

namespace dBridges.channel
{
    public class channelnbd
    {
        private string channelName;
        private string sid;
        private object dbcore;
        private bool _isOnline;
     
        private Action_dispatcher dispatch;

        private static readonly List<string> list_of_supported_functionname = new List<string>
        {"channelMemberList", "channelMemberInfo", "timeout" ,  "err" };



        private static readonly List<string> connectSupportedEvents =new List<string> 
                                {systemEvents.CONNECT_SUCCESS,
                                systemEvents.CONNECT_FAIL,
                                systemEvents.RECONNECT_SUCCESS,
                                systemEvents.RECONNECT_FAIL,
                                systemEvents.DISCONNECT_SUCCESS,
                                systemEvents.DISCONNECT_FAIL,
                                systemEvents.ONLINE,
                                systemEvents.OFFLINE,
                                systemEvents.REMOVE,
                                systemEvents.PARTICIPANT_JOINED,
                                systemEvents.PARTICIPANT_LEFT };



        public channelnbd(string channelName, string sid, object dBCoreObject) : base()
        {
            this.channelName = channelName;
            this.sid = sid;
            this.dbcore = dBCoreObject;
            this._isOnline = false;
            
            this.dispatch = new Action_dispatcher();
        }


        public string getChannelName()
        {
            return this.channelName;
        }

        public bool isOnline()
        {
            return this._isOnline;
        }

        public void set_isOnline(bool value)
        {
            this._isOnline = value;
        }


        public void bind(string eventName, Delegate callback)
        {
            if (connectSupportedEvents.Contains(eventName))
            {
                try
                {
                    this.dispatch.bind(eventName, callback);
			     }catch (dBError err){
                    throw err;
                }
            }else{
                throw (new dBError("E103"));
   
             }
        }

       public void  unbind(string eventName, Delegate callback)
        {
            if (connectSupportedEvents.Contains(eventName))
            {
                if (this.dispatch.isExists(eventName)) this.dispatch.unbind(eventName, callback);
		    }
        }

        public void unbind()
        {
            this.dispatch.unbind();
        }


        public async Task emit_channel(string eventname, object payload = null, object metadata = null)
        {
            await this.dispatch.emit_channel(eventname, payload,  metadata);
	    }



        public async Task publish(string eventName, string eventData, string exclude_session_id = null, string source_id = null, string seqnum = null)
        {
            if (!this._isOnline) { throw (new dBError("E014")); }

            if (string.IsNullOrEmpty(eventName)) { throw (new dBError("E058")); }
            if (string.IsNullOrWhiteSpace(eventName)) { throw (new dBError("E059")); }
            if (this.channelName.ToLower().StartsWith("sys:*")) { throw (new dBError("E015")); }


            bool m_status = await util.updatedBNewtworkSC(this.dbcore, MessageType.PUBLISH_TO_CHANNEL, this.channelName, (string.IsNullOrEmpty(exclude_session_id) || string.IsNullOrWhiteSpace(exclude_session_id)) ? null : exclude_session_id, eventData, eventName, source_id, 0, seqnum);


            if (!m_status) throw (new dBError("E014"));
            return;
        }



        public async Task sendmsg(string eventName, string eventData, string to_session_id, string source_id = null, string seqnum = null)
        {


            if (channelName.ToLower() == "sys:*") throw (new dBError("E020"));
            if (!this._isOnline) { throw (new dBError("E019")); }

            if (channelName.ToLower().StartsWith("prs:"))
            {
                if (string.IsNullOrEmpty(source_id) || string.IsNullOrWhiteSpace(source_id)) throw (new dBError("E020"));
            }

            bool m_status = await util.updatedBNewtworkSC(this.dbcore, MessageType.SERVER_CHANNEL_SENDMSG, this.channelName, (to_session_id == null) ? null : to_session_id, eventData, eventName, source_id, 0, seqnum);
            if (!m_status) throw (new dBError("E019"));
            return;
        }


        public async Task<IPromise<object>> call(string functionName, string inparameter, UInt64 ttlms, Action<object> progress_callback)
        {
            var promise = new Promise<object>();

            if (!this._isOnline) { promise.Reject(new dBError("E019")); }
            if (!channelnbd.list_of_supported_functionname.Contains(functionName))
            {
                promise.Reject(new dBError("E038"));
            }


            if (this.channelName.ToLower().StartsWith("prs:") || this.channelName.ToLower().StartsWith("sys:"))
            {
                CrpCaller caller = (this.dbcore as dBridges).rpc.ChannelCall(this.channelName);

                IPromise<object> p = await caller.call(functionName, inparameter, ttlms, progress_callback);

                     p.Then((result) =>
                     {
                        // (this.dbcore as dBridges).rpc.ClearChannel(this.channelName);
                         promise.Resolve(result);
                        
                     })
                    .Catch((exec) =>
                    {
                       // (this.dbcore as dBridges).rpc.ClearChannel(this.channelName);
                        promise.Reject(exec);
                        
                    });

            }
            else
            {
                promise.Reject(new dBError("E039"));
            }
            return promise;
        }

    }
}
