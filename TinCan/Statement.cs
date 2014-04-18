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
    public class Statement : StatementBase
    {
        private const String ISODateTimeFormat = "o";

        public Nullable<Guid> id { get; set; }
        public Nullable<DateTime> stored { get; set; }
        public Agent authority { get; set; }
        public TCAPIVersion version { get; set; }
        //public List<Attachment> attachments { get; set; }

        public Statement() : base() { }
        public Statement(StringOfJSON json) : this(json.toJObject()) { }

        public Statement(JObject jobj) : base(jobj) {
            if (jobj["id"] != null)
            {
                id = new Guid(jobj.Value<String>("id"));
            }
            if (jobj["stored"] != null)
            {
                stored = jobj.Value<DateTime>("stored");
            }
            if (jobj["authority"] != null)
            {
                authority = (Agent)jobj.Value<JObject>("authority");
            }
            if (jobj["version"] != null)
            {
                version = (TCAPIVersion)jobj.Value<String>("version");
            }
        }

        public override JObject toJObject(TCAPIVersion version)
        {
            JObject result = base.toJObject(version);

            if (id != null)
            {
                result.Add("id", id.ToString());
            }
            if (stored != null)
            {
                result.Add("stored", stored.Value.ToString(ISODateTimeFormat));
            }
            if (authority != null)
            {
                result.Add("authority", authority.toJObject(version));
            }
            if (version != null)
            {
                result.Add("version", version.ToString());
            }

            return result;
        }

        public void Stamp()
        {
            if (id == null)
            {
                id = Guid.NewGuid();
            }
            if (timestamp == null)
            {
                timestamp = DateTime.UtcNow;
            }
        }
    }
}
