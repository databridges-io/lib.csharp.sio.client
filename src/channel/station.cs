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
using System.Threading.Tasks;
using dBridges.dispatchers;
using dBridges.Utils;
using dBridges.Messages;
using dBridges.exceptions;
using dBridges.events;
using dBridges.privateAccess;
using dBridges.Tokens;
using System.Text.RegularExpressions;
using RSG;
using dBridges.remoteprocedure;
using System.Collections.Concurrent;
namespace dBridges.channel
{
    public class station
    {
        private static readonly List<string> channel_type = new List<string> 
        { "pvt" ,   "prs", "sys"};

        private static readonly List<string> list_of_supported_functionname = new List<string>
        {"channelMemberList", "channelMemberInfo", "timeout" ,  "err" };

        private ConcurrentDictionary<string, object> c_channelsid_registry;
        private ConcurrentDictionary<string, string> c_channelname_sid;

    
        private object dbcore;
       
        private Action_dispatcher dispatch;

        public station(object dBCoreObject)
        {

            
            this.c_channelsid_registry = new ConcurrentDictionary<string, object>();
            this.c_channelname_sid = new ConcurrentDictionary<string, string>();

            this.dbcore = dBCoreObject;
            this.dispatch = new Action_dispatcher();

        }


        public void bind(string eventName, Delegate callback)
        {
            this.dispatch.bind(eventName, callback);
        }

        public void unbind(string eventName, Delegate callback)
        {
            this.dispatch.unbind(eventName, callback);
        }

        public void bind_all(Delegate callback)
        {
            this.dispatch.bind_all(callback);
        }

        public void unbind_all()
        {
            this.dispatch.unbind_all();
        }

        private string get_sid_From_channelname_sid(string channelName)
        {
            string sid = "";
          
            try
            {
                if (!this.c_channelname_sid.ContainsKey(channelName)) return "";
                this.c_channelname_sid.TryGetValue(channelName, out sid);
            }
            catch (Exception)
            {
                return "";
            }
            return sid;
        }

        private Dictionary<string, object> get_Dictionary_From_channelsid_registry(string sid)
        {
            object m_object;

            try
            {
                if (!this.c_channelsid_registry.ContainsKey(sid)) return null;
                this.c_channelsid_registry.TryGetValue(sid, out m_object);
            }
            catch (Exception)
            {
                return null;
            }
            return m_object  as Dictionary<string, object>;
        }


        public async Task handledispatcherEvents(string eventName , object eventInfo= null, string channelName= null, object metadata= null)
        {
            await this.dispatch.emit_channel(eventName, eventInfo,metadata);
            string sid = this.get_sid_From_channelname_sid(channelName);
            if (string.IsNullOrEmpty(sid)) return;

            
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return;

            if (m_object != null)
            {
                string type = m_object["type"] as string;
                if (type == "s")
                {
                    channel cobject = m_object["ino"] as channel;
                    await cobject.emit_channel(eventName, eventInfo, metadata);
                }
               
            }
        }


        public async Task handledispatcherEvents_publish(string eventName, object eventInfo = null, string channelName = null, object metadata = null)
        {
            await this.dispatch.emit_channel(eventName, eventInfo, metadata);
            
            string sid = this.get_sid_From_channelname_sid(channelName);
            if (string.IsNullOrEmpty(sid)) return;


            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return;


            if (m_object != null)
            {
                string type = m_object["type"] as string;
                if (type == "s")
                {
                    channel cobject = m_object["ino"] as channel;
                    await cobject.emit_publish(eventName, eventInfo, metadata);
                }
            }
        }



        public bool isPrivateChannel(string channelName)
        {
            bool flag = false;
            if (channelName.Contains(":"))
            {
                string[] sdata = channelName.ToLower().Split(':');
                if (station.channel_type.Contains(sdata[0])) flag = true;
            }
            return flag;
        }

        public async Task communicateR(int mtype, string channelName, string sid, string access_token)
        {
            bool cStatus = false;
            if (mtype == 0)
            {
             
                cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.SUBSCRIBE_TO_CHANNEL, channelName, sid, access_token, null, null);
            }
            else
            {
              
                cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.CONNECT_TO_CHANNEL, channelName, sid, access_token);
            }
           
                if (!cStatus){
                 
                if (mtype == 0){
                        throw (new dBError("E024"));
                    }else{
                        throw (new dBError("E090"));
                    }
                }
            }



        public void Remove_keys_from_Register(string name , string sid)
        {
            string v_value = "";
            object v_object;
            bool is_removed = false;
            try
            {
                is_removed = this.c_channelname_sid.TryRemove(name, out v_value);
                is_removed = this.c_channelsid_registry.TryRemove(sid, out v_object);
            }
            catch (Exception )
            {
               
            }
        }


        public async Task ReSubscribe(string sid)
        {
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return;

            string access_token = null;
            bool mprivate = this.isPrivateChannel(m_object["name"] as string);

            switch (m_object["status"] as string)
            {
                case channelState.SUBSCRIPTION_INITIATED:
                case channelState.SUBSCRIPTION_ACCEPTED:
                    try
                    {
                        if (!mprivate){
                          
                            await this.communicateR(0, m_object["name"] as string, sid, access_token);
                        }else{
                            CprivateResponse response = new CprivateResponse(0 ,  0, m_object["name"] as string, sid, this);
                            string m_actiontype = "";

                            if ((m_object["name"] as string).ToLower().StartsWith("sys:")){
                                m_actiontype = tokenTypes.SYSTEM_CHANNELSUBSCRIBE;
                            }else{
                                m_actiontype = tokenTypes.CHANNELSUBSCRIBE;
                            }

                            
                            await (this.dbcore as dBridges).accesstoken_dispatcher(m_object["name"] as string, m_actiontype ,  response );
						}

                    }
                    catch (dBError e)
                    {
                        List<string> eventse = new List<string> { systemEvents.OFFLINE };
                        await this.handleSubscribeEvents(eventse, e, m_object);
                    }
                    break;

                case channelState.CONNECTION_INITIATED:
                case channelState.CONNECTION_ACCEPTED:

                    try
                    {
                        if (!mprivate)
                        {
                            await this.communicateR(1, m_object["name"] as string, sid, access_token);
                        }
                        else
                        {
                            CprivateResponse response = new CprivateResponse(0, 1, m_object["name"] as string, sid, this);

                            await (this.dbcore as dBridges).accesstoken_dispatcher(m_object["name"] as string, tokenTypes.CHANNELCONNECT, response);
                        }


                    }
                    catch (Exception e)
                    {
                        List<string> eventse = new List<string> { systemEvents.OFFLINE };
                        await this.handleSubscribeEvents(eventse, e.Message, m_object);
                    }
                    break;
                case channelState.UNSUBSCRIBE_INITIATED:
                    (m_object["ino"] as channel).set_isOnline(false);
                    List<string> events = new List<string> { systemEvents.UNSUBSCRIBE_SUCCESS, systemEvents.REMOVE };

                    this.Remove_keys_from_Register(m_object["name"] as string, sid);


                    await this.handleSubscribeEvents(events, "", m_object);
                    break;
                case channelState.DISCONNECT_INITIATED:
                    (m_object["ino"] as channelnbd).set_isOnline(false);
                    List<string> eventsd = new List<string> { systemEvents.DISCONNECT_SUCCESS, systemEvents.REMOVE };

                    this.Remove_keys_from_Register(m_object["name"] as string, sid);

                    await this.handleSubscribeEvents(eventsd, "", m_object);
                    break;
                default:
                    break;
            }
        }




        public async Task  ReSubscribeAll()
        {
            foreach (KeyValuePair<string, string> entry in this.c_channelname_sid)
            {
                
                await this.ReSubscribe(entry.Value);

            }
        }


        public bool isEmptyOrSpaces(string str)
        {
            str = str.Trim();
            return string.IsNullOrEmpty(str);
        }


        private bool isNetworkConnected(string name, int valid_type = 0)
        {
            if(!((this.dbcore as dBridges).connectionstate.isconnected))
            {
                switch(valid_type)
                {
                    case 0:
                        throw (new dBError("E024"));
                        
                    case 1:
                        throw (new dBError("E090"));
                        
                    case 2:
                        throw (new dBError("E014"));
                        
                    case 3:
                        throw (new dBError("E019"));
                        
                    case 4:
                        throw (new dBError("E033"));                        
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
                    case 0:
                        throw (new dBError("E025"));
                        
                    case 1:
                        throw (new dBError("E095"));
                        
                    case 2:
                        throw (new dBError("E016"));
                        
                    case 3:
                        throw (new dBError("E021"));
                        
                    case 4:
                        throw (new dBError("E037"));
                            
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
                    case 0:
                        throw (new dBError("E027"));
                        
                    case 1:
                        throw (new dBError("E095"));
                        
                    case 2:
                        throw (new dBError("E017"));
                        
                    case 3:
                        throw (new dBError("E022"));
                        
                    case 4:
                        throw (new dBError("E036"));
                        
                }
            }
            return true;
        }


        private bool isvalidSyntex(string name)
        {
            Regex rgx = new Regex("^[a-zA-Z0-9.:_-]+$");
            return rgx.IsMatch(name);
        }
        private bool validateSyntax(string name , int valid_type = 0)
        {
            if (!isvalidSyntex(name))
            {
                switch (valid_type)
                {
                    case 0:
                        throw (new dBError("E028"));

                    case 1:
                        throw (new dBError("E095"));

                    case 2:
                        throw (new dBError("E015"));

                    case 3:
                        throw (new dBError("E023"));

                    case 4:
                        throw (new dBError("E039"));

                }
            }
                return true;
        }

        private bool validatePreDefinedName(string name, int valid_type = 0)
        {

            if (name.Contains(":"))
            {
                string[] sdata = name.ToLower().Split(':');
                if (!(station.channel_type.Contains(sdata[0])))
                {
                    switch (valid_type)
                    {
                        case 0:
                            throw (new dBError("E028"));
                            
                        case 1:
                            throw (new dBError("E095"));
                            
                        case 2:
                            throw (new dBError("E015"));
                            
                        case 3:
                            throw (new dBError("E023"));
                            
                        case 4:
                            throw (new dBError("E039"));
                    }

                } 
            }
            return true;
        }


        private bool isChannelNameExists(string name, int valid_type = 0)
        {
            if (this.c_channelname_sid.ContainsKey(name))
            {
                switch (valid_type)
                {
                    case 0:
                        throw (new dBError("E025"));

                    case 1:
                        throw (new dBError("E095"));

                    case 2:
                        throw (new dBError("E016"));

                    case 3:
                        throw (new dBError("E021"));

                    case 4:
                        throw (new dBError("E037"));

                }
            }
            return true;
        }


        private bool isChannelNameString(string name, int valid_type = 0)
        {
            if (name.GetType().Name != "String")
            {
                switch (valid_type)
                {
                    case 0:
                        throw (new dBError("E026"));

                    case 1:
                        throw (new dBError("E095"));

                    case 2:
                        throw (new dBError("E016"));

                    case 3:
                        throw (new dBError("E021"));

                    case 4:
                        throw (new dBError("E037"));

                }
            }
            return true;
        }


        private void validateName(string name, int valid_type=0)
        {
            try
            {
                
                this.isNetworkConnected(name, valid_type);
                this.isChannelNameString(name, valid_type);
                this.isEmptyORBlank(name, valid_type);
                this.validataNameLength(name, valid_type);
                this.validateSyntax(name, valid_type);
                this.validatePreDefinedName(name, valid_type);
            }
            catch(dBError e)
            {
                throw e;
            }
        }

        public async Task<object> communicate(int mtype, string channelName, bool mprivate , string actiontype)
        {
            bool cStatus = false;
            Dictionary<string, object> m_value;
            string access_token = null;
            string sid = util.GenerateUniqueId();
            
            if (!mprivate)
            {
                if (mtype == 0)
                {
                    cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.SUBSCRIBE_TO_CHANNEL, channelName, sid, access_token, null, null, 0, null);
                }
                else
                {
                    cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.CONNECT_TO_CHANNEL, channelName, sid, access_token);

                }
                if (!cStatus)
                {
                    if (mtype == 0)
                    {
                        throw (new dBError("E024"));
                    }
                    else
                    {
                        throw (new dBError("E090"));
                    }
                }

            }
            else
            {
                CprivateResponse response = new CprivateResponse(0, mtype, channelName, sid, this);

                await (this.dbcore as dBridges).accesstoken_dispatcher(channelName , actiontype,  response );
            }
           
            object m2_channel = null;
            if (mtype == 0) { 
            channel m_channel = new channel(channelName, sid, this.dbcore);
            m_value = new Dictionary<string, object>  {
                { "name", channelName },
                {"type",  "s" },
                {"status",channelState.SUBSCRIPTION_INITIATED },
                {"ino", m_channel } };
            m2_channel = m_channel;
        }else{
                channelnbd m_channel = new channelnbd(channelName, sid, this.dbcore);
                m_value = new Dictionary<string, object>  {
                { "name", channelName },
                {"type",  "c" },
                {"status",channelState.CONNECTION_INITIATED },
                {"ino", m_channel } };
                m2_channel = m_channel;
            }

            this.add_keys_from_Register(channelName, sid, m_value);
            return m2_channel;
        }




        public void add_keys_from_Register(string name, string sid , object v_object)
        {
            
            bool is_removed = false;
            try
            {
                is_removed = this.c_channelname_sid.TryAdd(name, sid);
                is_removed = this.c_channelsid_registry.TryAdd(sid, v_object);
            }
            catch (Exception)
            {
               
            }
        }



        public void update_keys_from_Register(string sid, object new_v_object)
        {

            object tout;
            try
            {
                if (this.c_channelsid_registry.TryGetValue(sid, out tout))
                    this.c_channelsid_registry.TryUpdate(sid, tout, new_v_object);
            }
            catch (Exception )
            {
               
            }
        }


        private async Task failure_dispatcher(int mtype , string sid , string reason, int newreason=0)
        {
            List<string> eventse;
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return;
             dBError dberror;
            if(mtype == 0){
                (m_object["ino"] as channel).set_isOnline(false);
                if (newreason == 0){
                    dberror = new dBError("E091");
                }else{
                    dberror = new dBError("E024");
                }
                dberror.updateCode("", reason);
                eventse = new List<string> { systemEvents.SUBSCRIBE_FAIL };
                this.Remove_keys_from_Register(m_object["name"] as string, sid);
                await this.handleSubscribeEvents(eventse , dberror ,  m_object);
            }
            else
            {
                if (newreason == 0){
                    dberror = new dBError("E092");
                }else{
                    dberror = new dBError("E090");
                }
                dberror.updateCode("", reason);
                eventse = new List<string> { systemEvents.CONNECT_FAIL };
                this.Remove_keys_from_Register(m_object["name"] as string, sid);
                await this.handleSubscribeEvents(eventse, dberror, m_object);
            }
	}

       public async Task send_to_dbr(int mtype , string channelName , string sid , CPrivateInfo access_data)
        {
            bool cStatus=false;
           if (access_data.statuscode != 0)
            {
               await this.failure_dispatcher(mtype, sid, access_data.error_message);
            }
            else
            {
                string acctoken = access_data.accesskey;
                if(mtype == 0)
                {
                    cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.SUBSCRIBE_TO_CHANNEL, channelName, sid, acctoken);
             
                }else{
                    cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.CONNECT_TO_CHANNEL, channelName, sid, acctoken);
                }
                if (!cStatus)
                {
                    await this.failure_dispatcher(mtype, sid, "library is not connected with the dbridges network",1);
                }
            }

        }


        public async Task<channel> subscribe(string channelName)
        {
            try
            {
                this.validateName(channelName);
            }
            catch (Exception error)
            {
                throw (error);
            }
      
            if (this.c_channelname_sid.ContainsKey(channelName)) { throw (new dBError("E093")); }

            bool mprivate = this.isPrivateChannel(channelName);


            object m_channel;
            string m_actiontype = "";

            if (channelName.ToLower().StartsWith("sys:"))
            {
                m_actiontype = tokenTypes.SYSTEM_CHANNELSUBSCRIBE;
            }
            else
            {
                m_actiontype = tokenTypes.CHANNELSUBSCRIBE;
            }

            try
            {
                m_channel = await this.communicate(0, channelName, mprivate, m_actiontype);
            }
            catch (Exception error)
            {
                throw (error);
            }
            return m_channel as channel;
        }



        public async Task<channelnbd> connect(string channelName)
        {
           
            if (channelName.ToLower() != "sys:*")
            {
                try
                {
                    this.validateName(channelName, 1);
    
                }catch (dBError error){
                    throw (error);
                }
            }


            if (channelName.ToLower().StartsWith("sys:")) throw (new dBError("E095"));

            if (this.c_channelname_sid.ContainsKey(channelName)) throw(new dBError("E094"));


            bool mprivate = this.isPrivateChannel(channelName);

            object m_channel = null;
      
            try
            {
                m_channel = await this.communicate(1, channelName, mprivate, tokenTypes.CHANNELCONNECT);
            }
            catch (dBError error){
                throw (error);
            }
            return m_channel as channelnbd;
        }






        public async Task unsubscribe(string channelName)
        {
     
            if (string.IsNullOrEmpty(channelName) || string.IsNullOrWhiteSpace(channelName)){throw (new dBError("E030"));}

            if (!this.c_channelname_sid.ContainsKey(channelName)) { throw (new dBError("E030")); }

            string sid = this.get_sid_From_channelname_sid(channelName);
            if(string.IsNullOrEmpty(sid)) { throw (new dBError("E030")); }

            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if(m_object == null) { throw (new dBError("E030")); }

            bool m_status = false;
            string mtype = m_object["type"] as string;

            if (mtype != "s") { throw (new dBError("E096")); }
            string mstatus = m_object["status"] as string;
            if (mstatus == channelState.UNSUBSCRIBE_INITIATED) { throw (new dBError("E097")); }


            if (mstatus ==channelState.SUBSCRIPTION_ACCEPTED ||
                mstatus ==channelState.SUBSCRIPTION_INITIATED ||
                mstatus ==channelState.SUBSCRIPTION_PENDING ||
                mstatus ==channelState.SUBSCRIPTION_ERROR ||
                mstatus ==channelState.UNSUBSCRIBE_ERROR)
            {
                m_status = await util.updatedBNewtworkSC(this.dbcore, MessageType.UNSUBSCRIBE_DISCONNECT_FROM_CHANNEL, channelName, sid, null);
            }
            if (!m_status) { throw (new dBError("E098")); }

            m_object["status"] =channelState.UNSUBSCRIBE_INITIATED;
            this.update_keys_from_Register(sid, m_object);
        }




        public async Task disconnect(string channelName)
        {
            if (!this.c_channelname_sid.ContainsKey(channelName)) throw (new dBError("E099"));

            string sid = this.get_sid_From_channelname_sid(channelName);
            if (string.IsNullOrEmpty(sid)) throw (new dBError("E099"));

            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) throw (new dBError("E099"));


            bool m_status = false;
            string mtype = m_object["type"] as string;

            if (mtype != "c") throw (new dBError("E100"));
            string mstatus = m_object["status"] as string;

            if (mstatus == channelState.DISCONNECT_INITIATED) throw (new dBError("E101"));

            if (mstatus == channelState.CONNECTION_ACCEPTED ||
                mstatus == channelState.CONNECTION_INITIATED ||
                mstatus == channelState.CONNECTION_PENDING ||
                mstatus == channelState.CONNECTION_ERROR ||
                mstatus == channelState.DISCONNECT_ERROR)
            {
                m_status = await util.updatedBNewtworkSC(this.dbcore, MessageType.UNSUBSCRIBE_DISCONNECT_FROM_CHANNEL, channelName, sid ,  null );
		}

            if (!m_status) throw (new dBError("E102"));
            m_object["status"] = channelState.DISCONNECT_INITIATED;
        }






        public async Task handleSubscribeEvents(List<string> eventName, object eventData, Dictionary<string, object> m_object)
        {

            for (int i = 0; i < eventName.Count; i++)
            {
                string mtype = m_object["type"] as string;
                string channelName = "";
                if (mtype == "s") channelName = (m_object["ino"] as channel).getChannelName();
                
                if (mtype == "c") channelName = (m_object["ino"] as channelnbd).getChannelName();

                metaData md = new metaData(channelName, eventName[i]);

                await this.dispatch.emit_channel(eventName[i], eventData, md);

                if (mtype == "s") await (m_object["ino"] as channel).emit_channel(eventName[i], eventData, md);
                if (mtype == "c") await (m_object["ino"] as channelnbd).emit_channel(eventName[i], eventData, md);

                }

            }
        



        public async Task updateSubscribeStatus(string sid, string status, object reason)
        {
            if (!this.c_channelsid_registry.ContainsKey(sid)) return;
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return;


            string mtype = m_object["type"] as string;

            switch (mtype)
            {
                case "s":
                    switch (status)
                    {
                        case channelState.SUBSCRIPTION_ACCEPTED:
                            m_object["status"] = status;
                            this.update_keys_from_Register(sid, m_object);
                            (m_object["ino"] as channel).set_isOnline(true);
                            List<string> events = new List<string> { systemEvents.SUBSCRIBE_SUCCESS, systemEvents.ONLINE };
                            await this.handleSubscribeEvents(events, "", m_object);
                            break;
                        default:
                            m_object["status"] = status;
                            (m_object["ino"] as channel).set_isOnline(false);
                            List<string> eventssd = new List<string> { systemEvents.SUBSCRIBE_FAIL };

                            this.Remove_keys_from_Register(m_object["name"] as string, sid);

                            await this.handleSubscribeEvents(eventssd, reason, m_object);
                            break;
                    }
                    break;
                case "c":
                    switch (status)
                    {
                        case channelState.CONNECTION_ACCEPTED:
                            m_object["status"] = status;

                            this.update_keys_from_Register(sid, m_object);
                            (m_object["ino"] as channelnbd).set_isOnline(true);
                            List<string> events = new List<string> { systemEvents.CONNECT_SUCCESS, systemEvents.ONLINE };
                            await this.handleSubscribeEvents(events, "", m_object);
                            break;
                        default:
                            m_object["status"] = status;
                            (m_object["ino"] as channelnbd).set_isOnline(false);
                            List<string> eventssd = new List<string> { systemEvents.CONNECT_FAIL };

                            this.Remove_keys_from_Register(m_object["name"] as string, sid);

                            await this.handleSubscribeEvents(eventssd, reason, m_object);
                            break;
                    }
                    break;
                default:
                    break;
            }

        }

        public async Task updateSubscribeStatusRepeat(string sid, string status, object reason)
        {
            if (!this.c_channelsid_registry.ContainsKey(sid)) return;
            Dictionary<string, object> m_object = this.c_channelsid_registry[sid] as Dictionary<string, object>;

            string mtype = m_object["type"] as string;

            switch (mtype)
            {
                case "s":
                    switch (status)
                    {
                        case channelState.SUBSCRIPTION_ACCEPTED:
                            m_object["status"] = status;
                            this.update_keys_from_Register(sid, m_object);
                            (m_object["ino"] as channel).set_isOnline(true);
                            List<string> events = new List<string> { systemEvents.RESUBSCRIBE_SUCCESS, systemEvents.ONLINE };
                            await this.handleSubscribeEvents(events, "", m_object);
                            break;
                        default:
                            m_object["status"] = status;
                            this.update_keys_from_Register(sid, m_object);
                            (m_object["ino"] as channel).set_isOnline(false);
                            List<string> eventssd = new List<string> { systemEvents.RESUBSCRIBE_FAIL, systemEvents.OFFLINE };
                            await this.handleSubscribeEvents(eventssd, reason, m_object);
                            break;

                    }
                    break;
                case "c":
                    switch (status)
                    {
                        case channelState.CONNECTION_ACCEPTED:
                            m_object["status"] = status;
                            this.update_keys_from_Register(sid, m_object);
                            (m_object["ino"] as channelnbd).set_isOnline(true);
                            List<string> events = new List<string> { systemEvents.RECONNECT_SUCCESS, systemEvents.ONLINE };
                            await this.handleSubscribeEvents(events, "", m_object);
                            break;
                        default:
                            m_object["status"] = status;
                            this.update_keys_from_Register(sid, m_object);
                            (m_object["ino"] as channelnbd).set_isOnline(false);
                            List<string> eventssd = new List<string> { systemEvents.RECONNECT_FAIL, systemEvents.OFFLINE };
                            await this.handleSubscribeEvents(eventssd, reason, m_object);
                            break;

                    }
                    break;
                default:
                    break;
            }

        }

        public async Task updateChannelsStatusAddChange(int life_cycle, string sid, string status, object reason)
        {
            if (life_cycle == 0) 
            {
                await this.updateSubscribeStatus(sid, status, reason);
            }
            else
            {
                await this.updateSubscribeStatusRepeat(sid, status, reason);
            }
        }


        public async Task updateChannelsStatusRemove(string sid, string status, object reason)
        {
            if (!this.c_channelsid_registry.ContainsKey(sid)) return;
            Dictionary<string, object> m_object = this.c_channelsid_registry[sid] as Dictionary<string, object>;

            string mtype = m_object["type"] as string;

            switch (mtype)
            {
                case "s":
                    switch (status)
                    {
                        case channelState.UNSUBSCRIBE_ACCEPTED:
                            m_object["status"] = status;
                            this.update_keys_from_Register(sid, m_object);
                            (m_object["ino"] as channel).set_isOnline(false);
                            List<string> events = new List<string> { systemEvents.UNSUBSCRIBE_SUCCESS, systemEvents.REMOVE };

                            string name = m_object["name"] as string;
                            this.Remove_keys_from_Register(name, sid);

                           

                            await this.handleSubscribeEvents(events, "", m_object);

                            break;
                        default:
                            m_object["status"] =channelState.SUBSCRIPTION_ACCEPTED;
                            this.update_keys_from_Register(sid, m_object);
                            (m_object["ino"] as channel).set_isOnline(true);
                            List<string> eventssd = new List<string> { systemEvents.UNSUBSCRIBE_FAIL, systemEvents.ONLINE };
                            await this.handleSubscribeEvents(eventssd, reason, m_object);
                            break;

                    }
                    break;
                case "c":
                    switch (status)
                    {
                        case channelState.DISCONNECT_ACCEPTED:
                            m_object["status"] = status;
                            this.update_keys_from_Register(sid, m_object);
                            (m_object["ino"] as channelnbd).set_isOnline(false);
                            List<string> events = new List<string> { systemEvents.DISCONNECT_SUCCESS, systemEvents.REMOVE };
                            this.Remove_keys_from_Register(m_object["name"] as string, sid);

                            await this.handleSubscribeEvents(events, "", m_object);

                            break;
                        default:
                            m_object["status"] = channelState.DISCONNECT_ACCEPTED;
                            
                            this.update_keys_from_Register(sid, m_object);
                            (m_object["ino"] as channelnbd).set_isOnline(true);
                            List<string> eventssd = new List<string> { systemEvents.DISCONNECT_FAIL, systemEvents.ONLINE };
                            await this.handleSubscribeEvents(eventssd, reason, m_object);
                            break;

                    }
                    break;
                default:
                    break;
            }

        }

        public bool _isonline(string sid)
        {
            if (!this.c_channelsid_registry.ContainsKey(sid)) return false;

            
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return false;

            string mstatus = m_object["status"] as string;
            if (mstatus ==channelState.CONNECTION_ACCEPTED ||
                mstatus ==channelState.SUBSCRIPTION_ACCEPTED) return true;

            return false;
        }

        public bool isOnline(string channelName)
        {
            if (!this.c_channelname_sid.ContainsKey(channelName)) return false;
            if (!(this.dbcore as dBridges).isSocketConnected()) return false;

            string sid = this.get_sid_From_channelname_sid(channelName);
            if (string.IsNullOrEmpty(sid)) return false;

            return this._isonline(sid);
        }

        public List<Dictionary<string, object>> list()
        {
            List<Dictionary<string, object>> m_data = new List<Dictionary<string, object>>();

            foreach (KeyValuePair<string, object> entry in this.c_channelsid_registry)
            {
                Dictionary<string, object> m_object = entry.Value as Dictionary<string, object>;
                Dictionary<string, object> mtemp = new Dictionary<string, object> { {"name",  m_object["name"] as string } ,
                                                                                    {"type", (m_object["type"] as string == "s")? "subscribe": "connect"  },
                                                                                    {"isonlne", this._isonline(entry.Key) }};
                m_data.Add(mtemp);
            }
            return m_data;
        }

        public async Task send_OfflineEvents()
        {

            foreach (KeyValuePair<string, object> entry in this.c_channelsid_registry)
            {
                Dictionary<string, object> m_object = entry.Value as Dictionary<string, object>;
                metaData md = new metaData(m_object["name"] as string,   systemEvents.OFFLINE);
                await this.handledispatcherEvents(systemEvents.OFFLINE, "", m_object["name"] as string,md);
            }

        }

        public string get_subscribeStatus(string sid)
        {
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return "";

            string mstatus = m_object["status"] as string;
            return mstatus;
        }


        public string get_channelType(string sid)
        {
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return "";

            string ntype = m_object["type"] as string;
            return ntype;
        }


        public string get_channelName(string sid)
        {
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return "";

            string name = m_object["name"] as string;
            return name;
        }


        public string getConnectStatus(string sid)
        {
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return "";

            string mstatus = m_object["status"] as string;
            return mstatus;
        }


        public object getChannel(string sid)
        {
            if (!this.c_channelsid_registry.ContainsKey(sid)) return null;
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return null;


            object mobject = m_object["ino"];
            return mobject;
        }

        public string getChannelName(string sid)
        {
            if (!this.c_channelsid_registry.ContainsKey(sid)) return null;
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return "";

            string name = m_object["name"] as string;
            return name;
        }

        public bool isSubscribedChannel(string sid)
        {
            bool mflag = false;
            if (!this.c_channelsid_registry.ContainsKey(sid)) return mflag;
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return mflag;

            string ntype = m_object["type"] as string;
            if (ntype == "s")
            {
                mflag = true;
            }
            return mflag;
        }


        public void clean_channel(string sid)
        {
            Dictionary<string, object> m_object = this.get_Dictionary_From_channelsid_registry(sid);
            if (m_object == null) return;

            string ntype = m_object["type"] as string;
            if (ntype == "s"){
                channel cn = m_object["ino"] as channel;
                cn.unbind();
                cn.unbind_all();

            }else{
                channelnbd cn = m_object["ino"] as channelnbd;
                cn.unbind();
            }
        }


        public async Task cleanUp_All()
        {
            foreach (KeyValuePair<string, string> entry in this.c_channelname_sid)
            {
                metaData md = new metaData(entry.Key, systemEvents.REMOVE);

                await this.handledispatcherEvents(systemEvents.REMOVE, "", entry.Key, md);
                clean_channel(entry.Value);
            }
            this.c_channelname_sid.Clear();
            this.c_channelsid_registry.Clear();
            //this.dispatch.unbind();
            //this.dispatch.unbind_all();
        }



        }

    }
