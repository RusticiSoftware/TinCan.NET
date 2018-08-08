/*
    Copyright 2018 Rustici Software

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
using TinCan.Json;

namespace TinCan
{
    public class InteractionComponent : JsonModel
    {
        public String id;
        public LanguageMap description { get; set; }

        public InteractionComponent()
        {

        }

        public InteractionComponent(JObject jobj)
        {
            if (jobj["id"] != null)
            {
                id = jobj.Value<String>("id");
            }
            if (jobj["description"] != null)
            {
                description = (LanguageMap)jobj.Value<JObject>("description");
            }
        }

        public override JObject ToJObject(TCAPIVersion version)
        {
            JObject result = new JObject();

            if (id != null)
            {
                result.Add("id", id);
            }
            if (description != null && !description.isEmpty())
            {
                result.Add("description", description.ToJObject(version));
            }

            return result;
        }

        public static explicit operator InteractionComponent(JObject jobj)
        {
            return new InteractionComponent(jobj);
        }

    }

}
