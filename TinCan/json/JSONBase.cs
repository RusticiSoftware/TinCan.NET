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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TinCan.json
{
    public abstract class JSONBase : JSON
    {
        public abstract JObject toJObject(TCAPIVersion version);

        public JObject toJObject()
        {
            return toJObject(TCAPIVersion.latest());
        }

        public string toJSON(TCAPIVersion version, bool pretty = false)
        {
            Formatting formatting = Formatting.None;
            if (pretty)
            {
                formatting = Formatting.Indented;
            }

            return JsonConvert.SerializeObject(toJObject(version), formatting);
        }

        public string toJSON(bool pretty = false)
        {
            return toJSON(TCAPIVersion.latest(), pretty);
        }
    }
}
