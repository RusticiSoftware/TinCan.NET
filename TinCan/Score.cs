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
    public class Score : JsonModel
    {
        public Nullable<Double> scaled { get; set; }
        public Nullable<Double> raw { get; set; }
        public Nullable<Double> min { get; set; }
        public Nullable<Double> max { get; set; }

        public Score() {}

        public Score(StringOfJSON json): this(json.toJObject()) {}

        public Score(JObject jobj)
        {
            if (jobj["scaled"] != null)
            {
                scaled = jobj.Value<Double>("scaled");
            }
            if (jobj["raw"] != null)
            {
                raw = jobj.Value<Double>("raw");
            }
            if (jobj["min"] != null)
            {
                min = jobj.Value<Double>("min");
            }
            if (jobj["max"] != null)
            {
                max = jobj.Value<Double>("max");
            }
        }

        public override JObject ToJObject(TCAPIVersion version) {
            JObject result = new JObject();

            if (scaled != null)
            {
                result.Add("scaled", scaled);
            }
            if (raw != null)
            {
                result.Add("raw", raw);
            }
            if (min != null)
            {
                result.Add("min", min);
            }
            if (max != null)
            {
                result.Add("max", max);
            }

            return result;
        }

        public static explicit operator Score(JObject jobj)
        {
            return new Score(jobj);
        }
    }
}
