// <copyright file="Context.cs" company="Float">
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
    public class Context : JsonModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        public Context()
        {
        }

        public Context(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public Context(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["registration"] != null)
            {
                registration = new Guid(jobj.Value<string>("registration"));
            }

            if (jobj["instructor"] != null)
            {
                // TODO: can be Group?
                instructor = new Agent(jobj.Value<JObject>("instructor"));
            }

            if (jobj["team"] != null)
            {
                // TODO: can be Group?
                team = new Agent(jobj.Value<JObject>("team"));
            }

            if (jobj["contextActivities"] != null)
            {
                contextActivities = new ContextActivities(jobj.Value<JObject>("contextActivities"));
            }

            if (jobj["revision"] != null)
            {
                revision = jobj.Value<string>("revision");
            }

            if (jobj["platform"] != null)
            {
                platform = jobj.Value<string>("platform");
            }

            if (jobj["language"] != null)
            {
                language = jobj.Value<string>("language");
            }

            if (jobj["statement"] != null)
            {
                statement = new StatementRef(jobj.Value<JObject>("statement"));
            }

            if (jobj["extensions"] != null)
            {
                extensions = new Extensions(jobj.Value<JObject>("extensions"));
            }
        }

        public Guid? registration { get; set; }

        public Agent instructor { get; set; }

        public Agent team { get; set; }

        public ContextActivities contextActivities { get; set; }

        public string revision { get; set; }

        public string platform { get; set; }

        public string language { get; set; }

        public StatementRef statement { get; set; }

        public Extensions extensions { get; set; }

        /// <inheritdoc />
        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject();

            if (registration != null)
            {
                result.Add("registration", registration.ToString());
            }

            if (instructor != null)
            {
                result.Add("instructor", instructor.ToJObject(version));
            }

            if (team != null)
            {
                result.Add("team", team.ToJObject(version));
            }

            if (contextActivities != null)
            {
                result.Add("contextActivities", contextActivities.ToJObject(version));
            }

            if (revision != null)
            {
                result.Add("revision", revision);
            }

            if (platform != null)
            {
                result.Add("platform", platform);
            }

            if (language != null)
            {
                result.Add("language", language);
            }

            if (statement != null)
            {
                result.Add("statement", statement.ToJObject(version));
            }

            if (extensions != null)
            {
                result.Add("extensions", extensions.ToJObject(version));
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[Context: registration={registration}";
        }
    }
}
