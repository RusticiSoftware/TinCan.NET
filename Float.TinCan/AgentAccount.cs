// <copyright file="AgentAccount.cs" company="Float">
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

using System;
using System.Diagnostics.Contracts;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class AgentAccount : JsonModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAccount"/> class.
        /// </summary>
        public AgentAccount()
        {
        }

        public AgentAccount(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public AgentAccount(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["homePage"] != null)
            {
                homePage = new Uri(jobj.Value<string>("homePage"));
            }

            if (jobj["name"] != null)
            {
                name = jobj.Value<string>("name");
            }
        }

        public AgentAccount(Uri homePage, string name)
        {
            Contract.Requires(homePage != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            this.homePage = homePage;
            this.name = name;
        }

        // TODO: check to make sure is absolute?
        public Uri homePage { get; set; }

        public string name { get; set; }

        /// <inheritdoc />
        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject();

            if (homePage != null)
            {
                result.Add("homePage", homePage.ToString().TrimEnd('/'));
            }

            if (name != null)
            {
                result.Add("name", name);
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var trimmedHomepage = homePage.ToString().TrimEnd('/');
            return $"[AgentAccount: homePage={trimmedHomepage}, name={name}]";
        }
    }
}
