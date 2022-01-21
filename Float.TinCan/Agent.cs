// <copyright file="Agent.cs" company="Float">
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

using System.Diagnostics.Contracts;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class Agent : JsonModel, StatementTarget
    {
        public static readonly string OBJECT_TYPE = "Agent";

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        public Agent()
        {
        }

        public Agent(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public Agent(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["name"] != null)
            {
                name = jobj.Value<string>("name");
            }

            if (jobj["mbox"] != null)
            {
                mbox = jobj.Value<string>("mbox");
            }

            if (jobj["mbox_sha1sum"] != null)
            {
                mbox_sha1sum = jobj.Value<string>("mbox_sha1sum");
            }

            if (jobj["openid"] != null)
            {
                openid = jobj.Value<string>("openid");
            }

            if (jobj["account"] != null)
            {
                account = new AgentAccount(jobj.Value<JObject>("account"));
            }
        }

        public virtual string ObjectType => OBJECT_TYPE;

        public string name { get; set; }

        public string mbox { get; set; }

        public string mbox_sha1sum { get; set; }

        public string openid { get; set; }

        public AgentAccount account { get; set; }

        /// <inheritdoc />
        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject
            {
                { "objectType", ObjectType },
            };

            if (name != null)
            {
                result.Add("name", name);
            }

            if (account != null)
            {
                result.Add("account", account.ToJObject(version));
            }
            else if (mbox != null)
            {
                result.Add("mbox", mbox);
            }
            else if (mbox_sha1sum != null)
            {
                result.Add("mbox_sha1sum", mbox_sha1sum);
            }
            else if (openid != null)
            {
                result.Add("openid", openid);
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[Agent: name={name}, mbox={mbox}, openid={openid}, account={account}]";
        }
    }
}
