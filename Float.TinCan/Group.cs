// <copyright file="Group.cs" company="Float">
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
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class Group : Agent
    {
        public static readonly new string OBJECT_TYPE = "Group";

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        public Group()
        {
        }

        public Group(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public Group(JObject jobj) : base(jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["member"] != null)
            {
                member = new List<Agent>();

                foreach (var token in jobj["member"])
                {
                    if (token is JObject jagent)
                    {
                        member.Add(new Agent(jagent));
                    }
                }
            }
        }

        public override string ObjectType => OBJECT_TYPE;

        public List<Agent> member { get; set; }

        /// <inheritdoc />
        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = base.ToJObject(version);

            if (member != null && member.Count > 0)
            {
                var jmember = new JArray();
                result.Add("member", jmember);

                foreach (var agent in member)
                {
                    jmember.Add(agent.ToJObject(version));
                }
            }

            return result;
        }
    }
}
