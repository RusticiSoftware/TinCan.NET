// <copyright file="ContextActivities.cs" company="Float">
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
    public class ContextActivities : JsonModel
    {
        public ContextActivities()
        {
        }

        public ContextActivities(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public ContextActivities(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["parent"] != null)
            {
                parent = new List<Activity>();
                foreach (JObject jactivity in jobj["parent"])
                {
                    parent.Add(new Activity(jactivity));
                }
            }

            if (jobj["grouping"] != null)
            {
                grouping = new List<Activity>();
                foreach (JObject jactivity in jobj["grouping"])
                {
                    grouping.Add(new Activity(jactivity));
                }
            }

            if (jobj["category"] != null)
            {
                category = new List<Activity>();

                foreach (var token in jobj["category"])
                {
                    if (token is JObject activity)
                    {
                        category.Add(new Activity(activity));
                    }
                }
            }

            if (jobj["other"] != null)
            {
                other = new List<Activity>();

                foreach (var token in jobj["other"])
                {
                    if (token is JObject activity)
                    {
                        other.Add(new Activity(activity));
                    }
                }
            }
        }

        public List<Activity> parent { get; set; }

        public List<Activity> grouping { get; set; }

        public List<Activity> category { get; set; }

        public List<Activity> other { get; set; }

        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject();

            if (parent != null && parent.Count > 0)
            {
                var jparent = new JArray();
                result.Add("parent", jparent);

                foreach (var activity in parent)
                {
                    jparent.Add(activity.ToJObject(version));
                }
            }

            if (grouping != null && grouping.Count > 0)
            {
                var jgrouping = new JArray();
                result.Add("grouping", jgrouping);

                foreach (var activity in grouping)
                {
                    jgrouping.Add(activity.ToJObject(version));
                }
            }

            if (category != null && category.Count > 0)
            {
                var jcategory = new JArray();
                result.Add("category", jcategory);

                foreach (var activity in category)
                {
                    jcategory.Add(activity.ToJObject(version));
                }
            }

            if (other != null && other.Count > 0)
            {
                var jother = new JArray();
                result.Add("other", jother);

                foreach (var activity in other)
                {
                    jother.Add(activity.ToJObject(version));
                }
            }

            return result;
        }
    }
}
