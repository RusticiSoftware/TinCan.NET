// <copyright file="Statement.cs" company="Float">
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
using System.Globalization;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class Statement : StatementBase
    {
        const string ISODateTimeFormat = "o";

        /// <summary>
        /// Initializes a new instance of the <see cref="Statement"/> class.
        /// </summary>
        public Statement()
        {
        }

        public Statement(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public Statement(JObject jobj) : base(jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["id"] != null)
            {
                id = new Guid(jobj.Value<string>("id"));
            }

            if (jobj["stored"] != null)
            {
                stored = jobj.Value<DateTime>("stored");
            }

            if (jobj["authority"] != null)
            {
                authority = new Agent(jobj.Value<JObject>("authority"));
            }

            if (jobj["version"] != null)
            {
                version = new TCAPIVersion(jobj.Value<string>("version"));
            }

            // handle SubStatement as target which isn't provided by StatementBase
            // because SubStatements are not allowed to nest
            if (jobj["object"] != null && (string)jobj["object"]["objectType"] == SubStatement.OBJECT_TYPE)
            {
                target = new SubStatement(jobj.Value<JObject>("object"));
            }
        }

        public Guid? id { get; set; }

        public DateTime? stored { get; set; }

        public Agent authority { get; set; }

        public TCAPIVersion version { get; set; }

        /// <inheritdoc />
        public override JObject ToJObject(TCAPIVersion version)
        {
            var resultObject = base.ToJObject(version);

            if (id != null)
            {
                resultObject.Add("id", id.ToString());
            }

            if (stored != null)
            {
                resultObject.Add("stored", stored.Value.ToString(ISODateTimeFormat, CultureInfo.InvariantCulture));
            }

            if (authority != null)
            {
                resultObject.Add("authority", authority.ToJObject(version));
            }

            if (version != null)
            {
                resultObject.Add("version", version.ToString());
            }

            return resultObject;
        }

        public void Stamp()
        {
            if (id == null)
            {
                id = Guid.NewGuid();
            }

            if (timestamp == null)
            {
                timestamp = DateTime.UtcNow;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[Statement: id={id}, stored={stored}, authority={authority}, version={version}, base={base.ToString()}]";
        }
    }
}
