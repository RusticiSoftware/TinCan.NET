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
    public class Agent : StatementTarget
    {
        private String objectType = "Agent";

        public String name { get; set; }
        public String mbox { get; set; }
        public String mbox_sha1sum { get; set; }
        public String openid { get; set; }
        public AgentAccount account { get; set; }

        public Agent() { }

        public Agent(StringOfJSON json) : this(json.toJObject()) { }

        public Agent(JObject jobj)
        {
            if (jobj["mbox"] != null)
            {
                mbox = jobj.Value<String>("mbox");
            }
        }

        public override JObject toJObject(TCAPIVersion version)
        {
            JObject result = new JObject();
            result.Add("objectType", objectType);

            if (mbox != null)
            {
                result.Add("mbox", mbox);
            }

            return result;
        }
    }
}
