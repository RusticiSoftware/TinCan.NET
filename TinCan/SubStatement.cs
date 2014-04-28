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
    public class SubStatement : StatementBase, StatementTarget
    {
        public static readonly String OBJECT_TYPE = "SubStatement";
        public String ObjectType { get { return OBJECT_TYPE; } }

        public SubStatement() {}

        public SubStatement(StringOfJSON json): this(json.toJObject()) {}

        public SubStatement(JObject jobj) : base(jobj) { }

        public override JObject ToJObject(TCAPIVersion version) {
            var result = base.ToJObject(version);

            result.Add("objectType", ObjectType);

            return result;
        }

        public static explicit operator SubStatement(JObject jobj)
        {
            return new SubStatement(jobj);
        }
    }
}
