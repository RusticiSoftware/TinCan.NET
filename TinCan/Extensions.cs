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
    public class Extensions : JSONBase
    {
        private Dictionary<Uri, JToken> map;

        public Extensions()
        {
            map = new Dictionary<Uri, JToken>();
        }

        public Extensions(JObject jobj) : this()
        {
            foreach (var item in jobj)
            {
                map.Add(new Uri(item.Key), item.Value); 
            }
        }

        public override JObject toJObject(TCAPIVersion version)
        {
            JObject result = new JObject();
            foreach (KeyValuePair<Uri, JToken> entry in map)
            {
                result.Add(entry.Key.ToString(), entry.Value);
            }

            return result;
        }

        public bool isEmpty()
        {
            return map.Count > 0 ? false : true;
        }

        public static explicit operator Extensions(JObject jobj)
        {
            return new Extensions(jobj);
        }
    }
}