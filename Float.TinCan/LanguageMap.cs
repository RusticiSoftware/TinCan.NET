// <copyright file="LanguageMap.cs" company="Float">
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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class LanguageMap : JsonModel, IEnumerable<KeyValuePair<string, string>>
    {
        readonly Dictionary<string, string> map;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageMap"/> class.
        /// </summary>
        public LanguageMap()
        {
            map = new Dictionary<string, string>();
        }

        public LanguageMap(Dictionary<string, string> map)
        {
            Contract.Requires(map != null);
            this.map = map;
        }

        public LanguageMap(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public LanguageMap(JObject jobj) : this()
        {
            Contract.Requires(jobj != null);

            foreach (var entry in jobj)
            {
                map.Add(entry.Key, (string)entry.Value);
            }
        }

        /// <inheritdoc />
        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject();

            foreach (var entry in map)
            {
                result.Add(entry.Key, entry.Value);
            }

            return result;
        }

        public bool isEmpty()
        {
            return map.Count <= 0;
        }

        public void Add(string lang, string value)
        {
            map.Add(lang, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[LanguageMap: {string.Join(",", map)}]";
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return map.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return map.GetEnumerator();
        }
    }
}
