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
    public class Activity : StatementTarget
    {
        private String objectType = "Activity";

        public Uri id { get; set; }
        //public ActivityDefinition definition { get; set; }

        public Activity() { }

        public Activity(StringOfJSON json) : this(json.toJObject()) { }

        public Activity(JObject jobj)
        {
            if (jobj["id"] != null)
            {
                id = new Uri(jobj.Value<String>("id"));
            }
        }

        public override JObject toJObject(TCAPIVersion version)
        {
            JObject result = new JObject();
            result.Add("objectType", objectType);

            if (id != null)
            {
                result.Add("id", id.ToString());
            }

            return result;
        }
    }
}
