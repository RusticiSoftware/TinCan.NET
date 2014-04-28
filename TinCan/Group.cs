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
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TinCan.json;

namespace TinCan
{
    public class Group : Agent
    {
        public static readonly new String OBJECT_TYPE = "Group";
        public override String ObjectType { get { return OBJECT_TYPE; } }

        public List<Agent> member { get; set; }

        public Group() : base() { }
        public Group(StringOfJSON json) : this(json.toJObject()) { }

        public Group(JObject jobj) : base(jobj)
        {
            if (jobj["member"] != null)
            {
                member = new List<Agent>();
                foreach (JObject jagent in jobj["member"])
                {
                    member.Add(new Agent(jagent));
                }
            }
        }

        public override JObject ToJObject(TCAPIVersion version)
        {
            JObject result = base.ToJObject(version);
            if (member != null && member.Count > 0)
            {
                var jmember = new JArray();
                result.Add("member", jmember);

                foreach (Agent agent in member)
                {
                    jmember.Add(agent.ToJObject(version));
                }
            }

            return result;
        }

        public static explicit operator Group(JObject jobj)
        {
            return new Group(jobj);
        }
    }
}
