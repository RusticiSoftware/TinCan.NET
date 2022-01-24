// <copyright file="Verb.cs" company="Float">
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
    public class Verb : JsonModel
    {
        public static readonly Verb Completed = new (new ("http://adlnet.gov/expapi/verbs/completed"), "en-US", "completed");
        public static readonly Verb Terminated = new (new ("http://adlnet.gov/expapi/verbs/terminated"), "en-US", "terminated");
        public static readonly Verb Launched = new (new ("http://adlnet.gov/expapi/verbs/launched"), "en-US", "launched");
        public static readonly Verb Suspended = new (new ("http://adlnet.gov/expapi/verbs/suspended"), "en-US", "suspended");
        public static readonly Verb Favorited = new (new ("http://activitystrea.ms/schema/1.0/favorite"), "en-US", "favorited");
        public static readonly Verb Unfavorited = new (new ("http://activitystrea.ms/schema/1.0/unfavorite"), "en-US", "unfavorited");
        public static readonly Verb Initialized = new (new ("http://adlnet.gov/expapi/verbs/initialized"), "en-US", "initialized");
        internal static readonly Verb Voided = new (new ("http://adlnet.gov/expapi/verbs/voided"), "en-US", "voided");

        /// <summary>
        /// Initializes a new instance of the <see cref="Verb"/> class.
        /// </summary>
        public Verb()
        {
        }

        public Verb(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public Verb(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["id"] != null)
            {
                id = new Uri(jobj.Value<string>("id"));
            }

            if (jobj["display"] != null)
            {
                display = new LanguageMap(jobj.Value<JObject>("display"));
            }
        }

        public Verb(Uri uri)
        {
            Contract.Requires(uri != null);
            id = uri;
        }

        public Verb(Uri uri, string defaultLanguage, string defaultTerm)
        {
            Contract.Requires(uri != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(defaultLanguage));
            Contract.Requires(!string.IsNullOrWhiteSpace(defaultTerm));

            id = uri;
            display = new LanguageMap
            {
                { defaultLanguage, defaultTerm },
            };
        }

        public Uri id { get; init; }

        public LanguageMap display { get; init; } = new LanguageMap();

        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject();

            if (id != null)
            {
                result.Add("id", id.ToString());
            }

            if (display != null && !display.isEmpty())
            {
                result.Add("display", display.ToJObject(version));
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[Verb: id={id}, display={display}]";
        }
    }
}
