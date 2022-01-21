// <copyright file="StatementsResult.cs" company="Float">
// Copyright 2014 Rustici Software, 2018 Float, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class StatementsResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatementsResult"/> class.
        /// </summary>
        public StatementsResult()
        {
        }

        public StatementsResult(string str) : this(new StringOfJSON(str))
        {
        }

        public StatementsResult(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public StatementsResult(List<Statement> statements)
        {
            Contract.Requires(statements != null);
            this.statements = statements;
        }

        public StatementsResult(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["statements"] != null)
            {
                statements = jobj.Value<JArray>("statements").Select(item => new Statement((JObject)item)).ToList();
            }

            if (jobj["more"] != null)
            {
                more = jobj.Value<string>("more");
            }
        }

        public List<Statement> statements { get; set; }

        public string more { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[StatementsResult: statements={string.Join("|", statements)}, more={more}]";
        }
    }
}
