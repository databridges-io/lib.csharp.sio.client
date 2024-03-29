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
using System.Text.RegularExpressions;
using dBridges.Utils;
using dBridges.Messages;
using dBridges.events;
using dBridges.privateAccess;
using System.Collections.Concurrent;


namespace dBridges.remoteprocedure
{
    public class CRpc
    {
        private static readonly List<string> server_type = new List<string>
        { "pvt" ,   "prs", "sys"};

        private ConcurrentDictionary<string, object> c_serversid_registry;


        private ConcurrentDictionary<string, Dictionary<string,string>> c_servername_sid;


        private object dbcore;
        
        private Action_dispatcher dispatch;


        private ConcurrentDictionary<string, object> c_callersid_object;
        private Random generator;


        public CRpc(object dBCoreObject)
        {
            this.c_serversid_registry = new ConcurrentDictionary<string, object>();
            this.c_servername_sid = new ConcurrentDictionary<string, Dictionary<string,string>>();

            this.dbcore = dBCoreObject;

            this.dispatch = new Action_dispatcher();
            this.c_callersid_object = new ConcurrentDictionary<string, object>();
            this.generator = new Random();
        }

        public bool isEmptyOrSpaces(string str)
        {
            str = str.Trim();
            return string.IsNullOrEmpty(str);
        }


        private bool isNetworkConnected(string name, int valid_type = 0)
        {
            if (!((this.dbcore as dBridges).connectionstate.isconnected))
            {
                switch (valid_type)
                {

                    case 1:
                        throw (new dBError("E048"));
                    default:
                        throw (new dBError("E048"));
                }
            }
            return true;
        }


        private bool isEmptyORBlank(string name, int valid_type = 0)
        {
            if (isEmptyOrSpaces(name))
            {
                switch (valid_type)
                {
                    case 1:
                        throw (new dBError("E048"));
                    default:
                        throw (new dBError("E048"));
                }
            }
            return true;
        }


        private bool validataNameLength(string name, int valid_type = 0)
        {
            if (name.Length > 64)
            {
                switch (valid_type)
                {
                    case 1:
                        throw (new dBError("E051"));
                    default:
                        throw (new dBError("E051"));
                }
            }
            return true;
        }


        private bool isvalidSyntex(string name)
        {
            Regex rgx = new Regex("^[a-zA-Z0-9.:_-]+$");
            return rgx.IsMatch(name);
        }
        private bool validateSyntax(string name, int valid_type = 0)
        {
            if (!isvalidSyntex(name))
            {
                switch (valid_type)
                {
                    case 1:
                        throw (new dBError("E052"));
                    default:
                        throw (new dBError("E052"));
                }
            }
            return true;
        }

        private bool validatePreDefinedName(string name, int valid_type = 0)
        {

            if (name.Contains(":"))
            {
                string[] sdata = name.ToLower().Split(':');
                if (!(CRpc.server_type.Contains(sdata[0])))
                {
                    switch (valid_type)
                    {
                        case 1:
                            throw (new dBError("E052"));
                        default:
                            throw (new dBError("E052"));
                    }

                }
            }
            return true;
        }


        private void validateServerName(string name, int valid_type = 0)
        {
            try
            {
                this.isEmptyORBlank(name, valid_type);
                this.validataNameLength(name, valid_type);
                this.validateSyntax(name, valid_type);
                this.validatePreDefinedName(name, valid_type);
            }
            catch (dBError e)
            {
                throw e;
            }
        }

        public bool issidExists(string sid)
        {
            return this.c_serversid_registry.ContainsKey(sid);
        }


        public string get_rpcStatus(string sid)
        {
            if (!this.c_serversid_registry.ContainsKey(sid)) return "";

            CRpcContainer container = this.get_rpcContainer(sid);
            if (container == null) return "";
            return container.status;
        }


        public void bind(string eventName, Delegate callback)
        {
            this.dispatch.bind(eventName, callback);
        }


        public void unbind(string eventName, Delegate callback = null)
        {
            this.dispatch.unbind(eventName, callback);
        }


        public void bind_all(Delegate callback)
        {
            this.dispatch.bind_all(callback);
        }



        public void unbind_all(Delegate callback = null)
        {
            this.dispatch.unbind_all(callback);
        }

        public bool isPrivateServer(string serverName)
        {
            bool flag = false;
            if (serverName.Contains(":"))
            {
                string[] sdata = serverName.ToLower().Split(":".ToCharArray());
                if (CRpc.server_type.Contains(sdata[0]))
                {
                    flag = true;
                }

            }
            return flag;
        }



        private async Task communicateR(int mtype, string serverName, string sid, string access_token)
        {
            bool cStatus = false;

            if (mtype == 0)
            {
                cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.REGISTER_RPC_SERVER, serverName, sid, access_token);
            }
            else
            {
                cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.CONNECT_TO_RPC_SERVER, serverName, sid, access_token);
            }
            if (!cStatus)
            {
                if (mtype == 0)
                {
                    throw new dBError("E057");
                }
                else
                {
                    throw new dBError("E053");
                }
            }
        }


        private CRpcContainer get_rpcContainer(string sid)
        {
            object tout;
            CRpcContainer container = null;
            if (!this.c_serversid_registry.ContainsKey(sid)) return container;

            try
            {
                if (this.c_serversid_registry.TryGetValue(sid, out tout))
                {
                    container = tout as CRpcContainer;
                    return container;
                }
            }
            catch (Exception )
            {
                
            }
            return null;
        }



        private async Task ReSubscribe(string sid)
        {
         
            CRpcContainer m_object = this.get_rpcContainer(sid);
            if (m_object == null) return;

            string access_token = "";

            bool mprivate = this.isPrivateServer(m_object.name);

            switch (m_object.status)
            {
                case rpcState.RPC_CONNECTION_ACCEPTED:
                case rpcState.RPC_CONNECTION_INITIATED:
                    try
                    {
                        if (!mprivate)
                        {
                            await this.communicateR(1, m_object.name, sid, access_token);
                        }
                        else
                        {
                            CprivateResponse response = new CprivateResponse(1, 1, m_object.name as string, sid, this);
                            await (this.dbcore as dBridges).accesstoken_dispatcher(m_object.name, Tokens.tokenTypes.RPCCONNECT, response);
                        }
                    }
                    catch (dBError error)
                    {
                        List<string> eventse = new List<string> { systemEvents.RPC_CONNECT_FAIL, systemEvents.SERVER_OFFLINE };

                        await this.handleRegisterEvents(eventse, error, m_object);
                    }
                    break;
            }

        }


        public async Task ReSubscribeAll()
        {
            foreach (KeyValuePair<string, Dictionary<string,string>> entry in this.c_servername_sid)
            {
                foreach(KeyValuePair<string,string> kentry in entry.Value)
                    await this.ReSubscribe(kentry.Key);

            }
        }


        public async Task handleRegisterEvents(List<string> eventName, object eventData, CRpcContainer m_object)
        {

            for (int i = 0; i < eventName.Count; i++)
            {
                string mtype = m_object.type;
                string Name = "";
                if (mtype == "c") { Name = (m_object.ino as CrpCaller).getServerName(); }
                CrpcMetaData md = new CrpcMetaData(Name, eventName[i]);
                if (m_object.type == "c")
                {
                    await this.dispatch.emit_channel(eventName[i], eventData, md);
                    await (m_object.ino as CrpCaller).emit_channel(eventName[i], eventData, md);
                }
            }
        }



        public async Task handledispatcherEvents(string eventName, object eventInfo = null, string serverName = "", CrpcMetaData metadata = null)
        {

            await this.dispatch.emit(eventName, eventInfo, metadata);
            if (!this.c_servername_sid.ContainsKey(serverName)) return;

            Dictionary<string,string> sids = this.get_sidFrom_cservername_sid(serverName);
            //if (string.IsNullOrEmpty(sid)) return;
            if (sids.Count == 0) return;

            foreach (KeyValuePair<string, string> kentry in sids)
            {
                CRpcContainer m_object = this.get_rpcContainer(kentry.Key);
                if (m_object == null) return;

                if (m_object.type == "c")
                    await (m_object.ino as CrpCaller).emit(eventName, eventInfo, serverName, metadata);
            }
        }

        private string GetUniqueSid(string sid)
        {
            String nsid = this.generator.Next().ToString();
            if (this.c_serversid_registry.ContainsKey(nsid))
            {
                nsid = this.generator.Next().ToString();
            }
            return nsid;
        }
        

    private bool update_rpcContainer(string sid, string status)
    {
        object tout;
        CRpcContainer container = null;
        if (!this.c_serversid_registry.ContainsKey(sid)) return false;

        try
        {
            if (this.c_serversid_registry.TryGetValue(sid, out tout))
            {
                container = tout as CRpcContainer;
                container.status = status;
                this.c_serversid_registry.TryUpdate(sid, tout, container);
                return true;
            }
        }
        catch (Exception )
        {
           
        }
        return false;
    }




    public async Task updateRegistrationStatus(string sid, string status, object reason)
        {

        if (!this.c_serversid_registry.ContainsKey(sid)) return;
        CRpcContainer m_object = this.get_rpcContainer(sid);
        if (m_object == null) return;


        switch (m_object.type)
            {
                case "c":
                    switch (status)
                    {
                        case rpcState.RPC_CONNECTION_ACCEPTED:
                            this.update_rpcContainer(sid, status);
                            (m_object.ino as CrpCaller).set_isOnline(true);
                            await this.handleRegisterEvents(new List<string>() { systemEvents.RPC_CONNECT_SUCCESS, systemEvents.SERVER_ONLINE }, "", m_object);
                            break;
                        default:
                            this.update_rpcContainer(sid, status);
                            (m_object.ino as CrpCaller).set_isOnline(false);

                            await this.handleRegisterEvents(new List<string>() { systemEvents.RPC_CONNECT_FAIL }, reason, m_object);

                            Dictionary<string,string> v_string;
                            bool is_removed = false;
                            object v_object;
                            try
                            {
                                is_removed = this.c_servername_sid.TryRemove(m_object.name, out v_string);
                                is_removed = this.c_serversid_registry.TryRemove(sid, out v_object);


                            }
                            catch (Exception)
                            {
                                return;
                            }

                            break;
                    }
                    break;

            }

        }


        public async Task updateRegistrationStatusRepeat(string sid, string status, object reason)
        {


            if (!this.c_serversid_registry.ContainsKey(sid)) return;
            CRpcContainer m_object = this.get_rpcContainer(sid);
            if (m_object == null) return;


            switch (m_object.type)
            {
                case "c":
                    switch (status)
                    {
                        case rpcState.RPC_CONNECTION_ACCEPTED:
                            this.update_rpcContainer(sid, status);
                            (m_object.ino as CrpCaller).set_isOnline(true);

                            await this.handleRegisterEvents(new List<string>() {  systemEvents.SERVER_ONLINE }, "", m_object);
                            break;
                        default:
                            this.update_rpcContainer(sid, status);
                            (m_object.ino as CrpCaller).set_isOnline(false);


                            await this.handleRegisterEvents(new List<string>() { systemEvents.SERVER_OFFLINE }, reason, m_object);
                           
                            Dictionary<string,string> v_string;
                            bool is_removed = false;
                            object v_object;
                            try
                            {
                                is_removed = this.c_servername_sid.TryRemove(m_object.name, out v_string);
                                is_removed = this.c_serversid_registry.TryRemove(sid, out v_object);


                            }
                            catch (Exception)
                            {
                                return;
                            }

                            break;
                    }
                    break;

            }

        }


        public async Task updateRegistrationStatusAddChange(int life_cycle, string sid, string status, object reason)
        {
            if (life_cycle == 0)
            {
                await this.updateRegistrationStatus(sid, status, reason);
            }
            else
            {
                await this.updateRegistrationStatusRepeat(sid, status, reason);
            }
        }




        private async Task failure_dispatcher(int mtype, string sid, string reason, int newreason = 0)
        {
            List<string> eventse;
            if (!this.c_serversid_registry.ContainsKey(sid)) return;
            CRpcContainer m_object = this.get_rpcContainer(sid);
            if (m_object == null) return;


            (m_object.ino as CrpCaller).set_isOnline(false);
            dBError dberror = (newreason == 0) ? new dBError("E104") : new dBError("E053");
           
            
            dberror.updateCode("", reason);
            eventse = new List<string> { systemEvents.RPC_CONNECT_FAIL };
            await this.handleRegisterEvents(eventse, dberror, m_object);

            Dictionary<string,string> v_string;
            bool is_removed = false;
            object v_object;
            try
            {
                is_removed = this.c_servername_sid.TryRemove(m_object.name, out v_string);
                is_removed = this.c_serversid_registry.TryRemove(sid, out v_object);


            }
            catch (Exception)
            {
                return;
            }

        }

        public async Task send_to_dbr(int mtype, string channelName, string sid, CPrivateInfo access_data)
        {
            bool cStatus = false;
            if (access_data.statuscode != 0)
            {
                await this.failure_dispatcher(mtype, sid, access_data.error_message);
            }
            else
            {
                string acctoken = access_data.accesskey;
                cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.CONNECT_TO_RPC_SERVER, channelName, sid, acctoken);
                if (!cStatus)
                {
                    await this.failure_dispatcher(mtype, sid, "library is not connected with the dbridges network",1);
                }
            }

        }



        public async Task<CrpCaller> communicate(string serverName, bool mprivate, string action)
        {
            bool cStatus = false;

            string access_token = "";
            string sid = util.GenerateUniqueId();

            if (!mprivate)
            {
                cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.CONNECT_TO_RPC_SERVER, serverName, sid, access_token);
                if (!cStatus) throw new dBError("E053");
            }
            else
            {
                CprivateResponse response = new CprivateResponse(1, 1, serverName, sid, this);
                await (this.dbcore as dBridges).accesstoken_dispatcher(serverName, Tokens.tokenTypes.RPCCONNECT, response);
            }

            CrpCaller rpccaller = new CrpCaller(serverName, this.dbcore, this, "rpc");
            CRpcContainer m_value = new CRpcContainer(serverName, "c", rpcState.RPC_CONNECTION_INITIATED, rpccaller);
            try
            {
                if(!this.c_servername_sid.ContainsKey(serverName))
                {
                    Dictionary<string, string> sids = new Dictionary<string, string>();
                    sids.Add(sid, "");
                    this.c_servername_sid.TryAdd(serverName, sids);
                  
                }
                else
                {
                    try
                    {
                        Dictionary<string, string> sids;
                        if (this.c_servername_sid.TryGetValue(serverName, out sids))
                            sids.Add(sid, "");
                    }
                    catch (Exception){

                    }

                }
                this.c_serversid_registry.TryAdd(sid, m_value);
            }
            catch (Exception)
            {
                throw new dBError("E053");
            }

            return rpccaller;
        }



        public async Task<CrpCaller> connect(string serverName)
        {
            try
            {
                this.validateServerName(serverName, 1);
            }
            catch (dBError error)
            {
                throw error;
            }

            bool mprivate = this.isPrivateServer(serverName);

            CrpCaller m_caller = null;

            try
            {
                m_caller = await this.communicate(serverName, mprivate, Tokens.tokenTypes.RPCCONNECT);
            }
            catch (dBError error)
            {
                throw (error);
            }
            return m_caller;
        }



        private bool update_rpcContainer_count(string sid)
        {
            object tout;
            CRpcContainer container = null;
            if (!this.c_serversid_registry.ContainsKey(sid)) return false;

            try
            {
                if (this.c_serversid_registry.TryGetValue(sid, out tout))
                {
                    container = tout as CRpcContainer;
                    container.count += 1;
                    this.c_serversid_registry.TryUpdate(sid, tout, container);
                    return true;
                }
            }
            catch (Exception )
            {
               
            }
            return false;
        }



        public CrpCaller ChannelCall(string serverName)
        {
            string sid;
            if (this.c_servername_sid.ContainsKey(serverName))
            {
                Dictionary<string,string> sids = this.get_sidFrom_cservername_sid(serverName);
                List<string> list_sids = new List<string>(  sids.Keys);
                sid = list_sids[0];
                CRpcContainer mobject = this.get_rpcContainer(sid);
                this.update_rpcContainer_count(sid);
                return mobject.ino as CrpCaller;
            }
            else
            {
                sid = util.GenerateUniqueId();
                CrpCaller rpccaller = new CrpCaller(serverName, this.dbcore, this, "ch");
                CRpcContainer m_value = new CRpcContainer(serverName, "x", rpcState.RPC_CONNECTION_INITIATED, rpccaller);
                try
                {
                    

                    if (!this.c_servername_sid.ContainsKey(serverName))
                    {
                        Dictionary<string, string> sids = new Dictionary<string, string>();
                        sids.Add(sid, "");
                        this.c_servername_sid.TryAdd(serverName, sids);
                    }
                    else
                    {
                        try
                        {
                            Dictionary<string, string> sids;
                            if (this.c_servername_sid.TryGetValue(serverName, out sids))
                                sids.Add(sid, "");
                        }
                        catch (Exception)
                        {

                        }

                    }

                    this.c_serversid_registry.TryAdd(sid, m_value);
                }
                catch (Exception)
                {
                    throw new dBError("E109");
                }
                return rpccaller;
            }
        }


        private bool update_rpcContainer_count_decrement(string sid)
        {
            object tout;
            CRpcContainer container = null;
            if (!this.c_serversid_registry.ContainsKey(sid)) return false;

            try
            {
                if (this.c_serversid_registry.TryGetValue(sid, out tout))
                {
                    container = tout as CRpcContainer;
                    container.count -= 1;
                    this.c_serversid_registry.TryUpdate(sid, tout, container);
                    return true;
                }
            }
            catch (Exception )
            {
               
            }
            return false;
        }


       /* public void ClearChannel(string channelName)
        {
            if (!this.c_servername_sid.ContainsKey(channelName)) return;
            Dictionary<string,string> sids = this.get_sidFrom_cservername_sid(channelName);
            List<string> list_sids = new List<string>(sids.Keys);
            CRpcContainer m_object = this.get_rpcContainer(list_sids[0] );
            if (m_object.count == 1)
            {
                try
                {
                    this.c_servername_sid.TryRemove(channelName, out _);
                    this.c_serversid_registry.TryRemove(list_sids[0], out _);
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                this.update_rpcContainer_count_decrement(list_sids[0]);
            }
        }

        */
        public void store_object(string sid, CrpCaller rpccaller)
        {
            try
            {
                this.c_callersid_object.TryAdd(sid, rpccaller);
            }
            catch (Exception)
            {
                return;
            }
            return;
        }


        public void delete_object(string sid)
        {
            try
            {
                this.c_callersid_object.TryRemove(sid, out _);
            }
            catch (Exception)
            {
                return;
            }

        }

        public CrpCaller get_object(string sid)
        {
            bool isexists = false;
            object m_object;
            try
            {
                isexists = this.c_callersid_object.TryGetValue(sid, out m_object);
                if (isexists)
                {
                    CrpCaller crpc = m_object as CrpCaller;
                    return crpc;
                }
            }
            catch (Exception) { }


            return null;
        }


        public object get_rpcServerObject(string sid)
        {
            if (this.c_serversid_registry.ContainsKey(sid))
            {
                CRpcContainer mobject = this.get_rpcContainer(sid);
                return mobject.ino;
            }
            else
            {
                return null;
            }
        }

        private Dictionary<string, string> get_sidFrom_cservername_sid(string servername)
        {
            Dictionary<string,string> tout;

            if (!this.c_servername_sid.ContainsKey(servername)) return null;

            try
            {
                if (this.c_servername_sid.TryGetValue(servername, out tout))
                {

                    return tout;
                }
            }
            catch (Exception )
            {
                
            }
            return null;
        }

        public async Task send_OfflineEvents()
        {

            foreach (KeyValuePair<string, Dictionary<string, string>> entry in this.c_servername_sid)
            {
                foreach (KeyValuePair<string, string> kentry in entry.Value)
                {
                    CRpcContainer rpccontainer = get_rpcContainer(kentry.Key);
                    if (rpccontainer.type == "c")
                    {
                        (rpccontainer.ino as CrpCaller).set_isOnline(false);
                        await this.handleRegisterEvents(new List<string>() { systemEvents.SERVER_OFFLINE }, null, rpccontainer);
                    }
                   
                }
            }

        }

        private bool clean_registry(string sid)
        {
            CRpcContainer rpccontainer = get_rpcContainer(sid);
            bool excludesflag = false;
            if(rpccontainer.type == "r")
            {
                excludesflag = false;
            }
            else
            {
                (rpccontainer.ino as CrpCaller).unbind();
                excludesflag = true;
            }

            return excludesflag;
        }
        public async Task clean_all()
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> entry in this.c_servername_sid)
            {
                foreach (KeyValuePair<string, string> kentry in entry.Value)
                {
                    bool excludesflag = clean_registry(kentry.Key);
                    if (excludesflag)
                    {
                        object container;
                        this.c_serversid_registry.TryRemove(kentry.Key, out container);
                    }
                }
                Dictionary<string, string> tout;
                this.c_servername_sid.TryRemove(entry.Key, out tout);
            }
        }

    }
}
