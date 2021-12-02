﻿// <copyright file="Result.cs" company="Float">
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
using System.Xml;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class Result : JsonModel
    {
        public Result()
        {
        }

        public Result(StringOfJSON json) : this(json?.toJObject())
        {
        }

        public Result(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["completion"] != null)
            {
                completion = jobj.Value<bool>("completion");
            }

            if (jobj["success"] != null)
            {
                success = jobj.Value<bool>("success");
            }

            if (jobj["response"] != null)
            {
                response = jobj.Value<string>("response");
            }

            if (jobj["duration"] != null)
            {
                duration = XmlConvert.ToTimeSpan(jobj.Value<string>("duration"));
            }

            if (jobj["score"] != null)
            {
                score = new Score(jobj.Value<JObject>("score"));
            }

            if (jobj["extensions"] != null)
            {
                extensions = new Extensions(jobj.Value<JObject>("extensions"));
            }
        }

        public bool? completion { get; set; }

        public bool? success { get; set; }

        public string response { get; set; }

        public TimeSpan? duration { get; set; }

        public Score score { get; set; }

        public Extensions extensions { get; set; }

        public override JObject ToJObject(TCAPIVersion version)
        {
            var result = new JObject();

            if (completion != null)
            {
                result.Add("completion", completion);
            }

            if (success != null)
            {
                result.Add("success", success);
            }

            if (response != null)
            {
                result.Add("response", response);
            }

            if (duration != null)
            {
                result.Add("duration", XmlConvert.ToString((TimeSpan)duration));
            }

            if (score != null)
            {
                result.Add("score", score.ToJObject(version));
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
            return $"[Result: completion={completion}, success={success}, response={response}, duration={duration}, score={score}, extensions={extensions}]";
        }
    }
}
