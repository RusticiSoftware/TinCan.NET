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
using TinCan.Json;

namespace TinCan
{
    public class StatementsResult
    {
        public List<Statement> statements { get; set; }
        public String more { get; set; }

        public StatementsResult() {}
        public StatementsResult(String str) : this(new StringOfJSON(str)) {}
        public StatementsResult(StringOfJSON json) : this(json.toJObject()) {}
        public StatementsResult(List<Statement> statements)
        {
            this.statements = statements;
        }

        public StatementsResult(JObject jobj)
        {
            if (jobj["statements"] != null)
            {
                statements = new List<Statement>();
                foreach (var item in jobj.Value<JArray>("statements"))
                {
                    statements.Add(new Statement((JObject)item));
                }
            }
            if (jobj["more"] != null)
            {
                more = jobj.Value<String>("more");
            }
        }
    }
}