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
    public class Statement : JSONBase
    {
        public Nullable<Guid> id { get; set; }
        public Agent actor { get; set; }
        public Verb verb { get; set; }
        public StatementTarget target { get; set; }

        public Statement() { }

        public Statement(StringOfJSON json) : this(json.toJObject()) { }

        public Statement(JObject jobj)
        {
            if (jobj["actor"] != null)
            {
                actor = jobj.Value<Agent>("actor");
            }
        }

        public override JObject toJObject(TCAPIVersion version)
        {
            JObject result = new JObject();

            if (id != null)
            {
                result.Add("id", id.ToString());
            }

            if (actor != null)
            {
                result.Add("actor", actor.toJObject(version));
            }

            if (verb != null)
            {
                result.Add("verb", verb.toJObject(version));
            }

            if (target != null)
            {
                result.Add("object", target.toJObject(version));
            }

            return result;
        }
    }
}
