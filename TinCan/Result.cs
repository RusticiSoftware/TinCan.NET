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
using System.Xml;
using Newtonsoft.Json.Linq;
using TinCan.json;

namespace TinCan
{
    public class Result : JSONBase
    {
        public Nullable<Boolean> completion { get; set; }
        public Nullable<Boolean> success { get; set; }
        public String response { get; set; }
        public TimeSpan duration { get; set; }
        public Score score { get; set; }
        public Extensions extensions { get; set; }

        public Result() {}

        public Result(StringOfJSON json): this(json.toJObject()) {}

        public Result(JObject jobj)
        {
            if (jobj["completion"] != null)
            {
                completion = jobj.Value<Boolean>("completion");
            }
            if (jobj["success"] != null)
            {
                success = jobj.Value<Boolean>("success");
            }
            if (jobj["response"] != null)
            {
                response = jobj.Value<String>("response");
            }
            if (jobj["duration"] != null)
            {
                duration = XmlConvert.ToTimeSpan(jobj.Value<String>("duration"));
            }
            if (jobj["score"] != null)
            {
                score = (Score)jobj.Value<JObject>("score");
            }
            if (jobj["extensions"] != null)
            {
                extensions = (Extensions)jobj.Value<JObject>("extensions");
            }
        }

        public override JObject toJObject(TCAPIVersion version) {
            JObject result = new JObject();

            if (completion != null)
            {
                result.Add("completion", completion);
            }
            if (success != null)
            {
                result.Add("success", success);
            }
            if (response != null)
            {
                result.Add("response", response);
            }
            if (duration != null)
            {
                result.Add("duration", XmlConvert.ToString(duration));
            }
            if (score != null)
            {
                result.Add("score", score.toJObject(version));
            }
            if (extensions != null)
            {
                result.Add("extensions", extensions.toJObject(version));
            }

            return result;
        }

        public static explicit operator Result(JObject jobj)
        {
            return new Result(jobj);
        }
    }
}
