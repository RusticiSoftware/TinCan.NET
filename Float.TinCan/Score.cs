// <copyright file="Score.cs" company="Float">
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

using System.Diagnostics.Contracts;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class Score : JsonModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Score"/> class.
        /// </summary>
        public Score()
        {
        }

        public Score(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public Score(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["scaled"] != null)
            {
                scaled = jobj.Value<double>("scaled");
            }

            if (jobj["raw"] != null)
            {
                raw = jobj.Value<double>("raw");
            }

            if (jobj["min"] != null)
            {
                min = jobj.Value<double>("min");
            }

            if (jobj["max"] != null)
            {
                max = jobj.Value<double>("max");
            }
        }

        public double? scaled { get; set; }

        public double? raw { get; set; }

        public double? min { get; set; }

        public double? max { get; set; }

        /// <inheritdoc />
        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject();

            if (scaled != null)
            {
                result.Add("scaled", scaled);
            }

            if (raw != null)
            {
                result.Add("raw", raw);
            }

            if (min != null)
            {
                result.Add("min", min);
            }

            if (max != null)
            {
                result.Add("max", max);
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[Score: scaled={scaled}, raw={raw}, min={min}, max={max}]";
        }
    }
}
