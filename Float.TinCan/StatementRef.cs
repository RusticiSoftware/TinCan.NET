// <copyright file="StatementRef.cs" company="Float">
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
    public class StatementRef : JsonModel, StatementTarget
    {
        public static readonly string OBJECT_TYPE = nameof(StatementRef);

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementRef"/> class.
        /// </summary>
        public StatementRef()
        {
        }

        public StatementRef(Guid id)
        {
            this.id = id;
        }

        public StatementRef(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public StatementRef(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["id"] != null)
            {
                id = new Guid(jobj.Value<string>("id"));
            }
        }

        public string ObjectType => OBJECT_TYPE;

        public Guid? id { get; set; }

        /// <inheritdoc/>
        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject
            {
                { "objectType", ObjectType },
            };

            if (id != null)
            {
                result.Add("id", id.ToString());
            }

            return result;
        }
    }
}
