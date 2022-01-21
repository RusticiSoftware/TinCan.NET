// <copyright file="ActivityDefinition.cs" company="Float">
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
    public class ActivityDefinition : JsonModel
    {
        public static readonly Uri ModuleActivityType = new ("http://adlnet.gov/expapi/activities/module");
        public static readonly Uri CourseActivityType = new ("http://adlnet.gov/expapi/activities/course");
        public static readonly Uri ResourceActivityType = new ("http://id.tincanapi.com/activitytype/resource");

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityDefinition"/> class.
        /// </summary>
        public ActivityDefinition()
        {
        }

        public ActivityDefinition(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public ActivityDefinition(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["type"] != null)
            {
                type = new Uri(jobj.Value<string>("type"));
            }

            if (jobj["moreInfo"] != null)
            {
                moreInfo = new Uri(jobj.Value<string>("moreInfo"));
            }

            if (jobj["name"] != null)
            {
                name = new LanguageMap(jobj.Value<JObject>("name"));
            }

            if (jobj["description"] != null)
            {
                description = new LanguageMap(jobj.Value<JObject>("description"));
            }

            if (jobj["extensions"] != null)
            {
                extensions = new Extensions(jobj.Value<JObject>("extensions"));
            }
        }

        public Uri type { get; set; }

        public Uri moreInfo { get; set; }

        public LanguageMap name { get; set; }

        public LanguageMap description { get; set; }

        public Extensions extensions { get; set; }

        /// <inheritdoc />
        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject();

            if (type != null)
            {
                result.Add("type", type.ToString());
            }

            if (moreInfo != null)
            {
                result.Add("moreInfo", moreInfo.ToString());
            }

            if (name != null && !name.isEmpty())
            {
                result.Add("name", name.ToJObject(version));
            }

            if (description != null && !description.isEmpty())
            {
                result.Add("description", description.ToJObject(version));
            }

            if (extensions != null && !extensions.isEmpty())
            {
                result.Add("extensions", extensions.ToJObject(version));
            }

            return result;
        }
    }
}
