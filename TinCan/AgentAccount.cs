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
using TinCan.Json;

namespace TinCan
{
    public class AgentAccount : JsonModel
    {
        // TODO: check to make sure is absolute?
        public Uri homePage { get; set; }
        public String name { get; set; }

        public AgentAccount() { }

        public AgentAccount(StringOfJSON json) : this(json.toJObject()) { }

        public AgentAccount(JObject jobj)
        {
            if (jobj["homePage"] != null)
            {
                homePage = new Uri(jobj.Value<String>("homePage"));
            }
            if (jobj["name"] != null)
            {
                name = jobj.Value<String>("name");
            }
        }

        public AgentAccount(Uri homePage, String name)
        {
            this.homePage = homePage;
            this.name = name;
        }

        public override JObject ToJObject(TCAPIVersion version)
        {
            JObject result = new JObject();
            if (homePage != null)
            {
                result.Add("homePage", homePage.ToString());
            }
            if (name != null)
            {
                result.Add("name", name);
            }

            return result;
        }

        public static explicit operator AgentAccount(JObject jobj)
        {
            return new AgentAccount(jobj);
        }
    }
}
