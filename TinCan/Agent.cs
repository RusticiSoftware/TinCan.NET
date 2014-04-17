/*
    Copyright 2014 Rustici Software

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
using Newtonsoft.Json.Linq;
using TinCan.json;

namespace TinCan
{
    public class Agent : JSONBase, StatementTarget
    {
        public static readonly String OBJECT_TYPE = "Agent";
        public virtual String ObjectType { get { return OBJECT_TYPE; } }

        public String name { get; set; }
        public String mbox { get; set; }
        public String mbox_sha1sum { get; set; }
        public String openid { get; set; }
        public AgentAccount account { get; set; }

        public Agent() { }

        public Agent(StringOfJSON json) : this(json.toJObject()) { }

        public Agent(JObject jobj)
        {
            if (jobj["name"] != null)
            {
                name = jobj.Value<String>("name");
            }

            if (jobj["mbox"] != null)
            {
                mbox = jobj.Value<String>("mbox");
            }
            if (jobj["mbox_sha1sum"] != null)
            {
                mbox_sha1sum = jobj.Value<String>("mbox_sha1sum");
            }
            if (jobj["openid"] != null)
            {
                openid = jobj.Value<String>("openid");
            }
            if (jobj["account"] != null)
            {
                account = (AgentAccount)jobj.Value<JObject>("account");
            }
        }

        public override JObject toJObject(TCAPIVersion version)
        {
            JObject result = new JObject();
            result.Add("objectType", ObjectType);

            if (name != null)
            {
                result.Add("name", name);
            }

            if (account != null)
            {
                result.Add("account", account.toJObject(version));
            }
            else if (mbox != null)
            {
                result.Add("mbox", mbox);
            }
            else if (mbox_sha1sum != null)
            {
                result.Add("mbox_sha1sum", mbox_sha1sum);
            }
            else if (openid != null)
            {
                result.Add("openid", openid);
            }

            return result;
        }

        public static explicit operator Agent(JObject jobj)
        {
            return new Agent(jobj);
        }
    }
}
