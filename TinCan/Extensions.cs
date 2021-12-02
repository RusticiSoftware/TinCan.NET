// <copyright file="Extensions.cs" company="Float">
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class Extensions : JsonModel
    {
        readonly Dictionary<Uri, JToken> map;

        public Extensions()
        {
            map = new Dictionary<Uri, JToken>();
        }

        public Extensions(JObject jobj) : this()
        {
            Contract.Requires(jobj != null);

            foreach (var item in jobj)
            {
                map.Add(new Uri(item.Key), item.Value);
            }
        }

        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject();

            foreach (var entry in map)
            {
                result.Add(entry.Key.ToString(), entry.Value);
            }

            return result;
        }

        public bool isEmpty()
        {
            return map.Count <= 0;
        }
    }
}
