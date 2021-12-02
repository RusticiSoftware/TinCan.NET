﻿// <copyright file="JsonModel.cs" company="Float">
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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TinCan.Json
{
    public abstract class JsonModel : IJsonModel
    {
        public abstract JObject ToJObject(TCAPIVersion version);

        public JObject ToJObject()
        {
            return ToJObject(TCAPIVersion.latest());
        }

        public string ToJSON(TCAPIVersion version, bool pretty = false)
        {
            return JsonConvert.SerializeObject(ToJObject(version), pretty ? Formatting.Indented : Formatting.None);
        }

        public string ToJSON(bool pretty = false)
        {
            return ToJSON(TCAPIVersion.latest(), pretty);
        }
    }
}
