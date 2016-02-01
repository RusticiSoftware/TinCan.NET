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
    public class Activity : JsonModel, StatementTarget
    {
        public static readonly String OBJECT_TYPE = "Activity";
        public String ObjectType { get { return OBJECT_TYPE; } }

        private String _id;
        public String id
        {
            get { return _id; }
            set
            {
                Uri uri = new Uri(value);
                _id = value;
            }
        }

        public ActivityDefinition definition { get; set; }

        public Activity() { }

        public Activity(StringOfJSON json) : this(json.toJObject()) { }

        public Activity(JObject jobj)
        {
            if (jobj["id"] != null)
            {
                String idFromJSON = jobj.Value<String>("id");
                Uri uri = new Uri(idFromJSON);
                id = idFromJSON;
            }
            if (jobj["definition"] != null)
            {
                definition = (ActivityDefinition)jobj.Value<JObject>("definition");
            }
        }

        public override JObject ToJObject(TCAPIVersion version)
        {
            JObject result = new JObject();
            result.Add("objectType", ObjectType);

            if (id != null)
            {
                result.Add("id", id);
            }
            if (definition != null)
            {
                result.Add("definition", definition.ToJObject(version));
            }

            return result;
        }

        public static explicit operator Activity(JObject jobj)
        {
            return new Activity(jobj);
        }
    }
}
