// <copyright file="Activity.cs" company="Float">
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
    public class Activity : JsonModel, StatementTarget
    {
        public static readonly string OBJECT_TYPE = nameof(Activity);

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        public Activity()
        {
        }

        public Activity(Uri id)
        {
            Contract.Requires(id != null);
            this.id = id;
        }

        public Activity(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public Activity(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["id"] != null)
            {
                var thisId = jobj.Value<string>("id");
                var uri = new Uri(thisId);
                id = uri;
            }

            if (jobj["definition"] != null)
            {
                definition = new ActivityDefinition(jobj.Value<JObject>("definition"));
            }
        }

        public ActivityDefinition definition { get; init; } = new ActivityDefinition();

        public Uri id { get; init; }

        public string ObjectType => OBJECT_TYPE;

        /// <inheritdoc />
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

            if (definition != null)
            {
                result.Add("definition", definition.ToJObject(version));
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[Activity: id={id}, definition={definition}]";
        }
    }
}
