// <copyright file="About.cs" company="Float">
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
    public class About : JsonModel
    {
        public About(string str) : this(new StringOfJSON(str))
        {
        }

        public About(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public About(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["version"] != null)
            {
                version = new List<TCAPIVersion>();

                foreach (string item in jobj.Value<JArray>("version"))
                {
                    version.Add(new TCAPIVersion(item));
                }
            }

            if (jobj["extensions"] != null)
            {
                extensions = new Extensions(jobj.Value<JObject>("extensions"));
            }
        }

        public List<TCAPIVersion> version { get; set; }

        public Extensions extensions { get; set; }

        /// <inheritdoc />
        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject();

            if (this.version != null)
            {
                var versions = new JArray();

                foreach (var v in this.version)
                {
                    versions.Add(v.ToString());
                }

                result.Add("version", versions);
            }

            if (extensions != null && !extensions.isEmpty())
            {
                result.Add("extensions", extensions.ToJObject(version));
            }

            return result;
        }
    }
}
