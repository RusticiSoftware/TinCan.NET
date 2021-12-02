// <copyright file="StatementBase.cs" company="Float">
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
using System.Globalization;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public abstract class StatementBase : JsonModel
    {
        const string ISODateTimeFormat = "o";

        protected StatementBase()
        {
        }

        protected StatementBase(StringOfJSON json) : this(json?.toJObject())
        {
        }

        protected StatementBase(JObject jobj)
        {
            Contract.Requires(jobj != null);

            if (jobj["actor"] != null)
            {
                if (jobj["actor"]["objectType"] != null && (string)jobj["actor"]["objectType"] == Group.OBJECT_TYPE)
                {
                    actor = new Group(jobj.Value<JObject>("actor"));
                }
                else
                {
                    actor = new Agent(jobj.Value<JObject>("actor"));
                }
            }

            if (jobj["verb"] != null)
            {
                verb = new Verb(jobj.Value<JObject>("verb"));
            }

            if (jobj["object"] != null)
            {
                if (jobj["object"]["objectType"] != null)
                {
                    if ((string)jobj["object"]["objectType"] == Group.OBJECT_TYPE)
                    {
                        target = new Group(jobj.Value<JObject>("object"));
                    }
                    else if ((string)jobj["object"]["objectType"] == Agent.OBJECT_TYPE)
                    {
                        target = new Agent(jobj.Value<JObject>("object"));
                    }
                    else if ((string)jobj["object"]["objectType"] == Activity.OBJECT_TYPE)
                    {
                        target = new Activity(jobj.Value<JObject>("object"));
                    }
                    else if ((string)jobj["object"]["objectType"] == StatementRef.OBJECT_TYPE)
                    {
                        target = new StatementRef(jobj.Value<JObject>("object"));
                    }
                }
                else
                {
                    target = new Activity(jobj.Value<JObject>("object"));
                }
            }

            if (jobj["result"] != null)
            {
                result = new Result(jobj.Value<JObject>("result"));
            }

            if (jobj["context"] != null)
            {
                context = new Context(jobj.Value<JObject>("context"));
            }

            if (jobj["timestamp"] != null)
            {
                timestamp = jobj.Value<DateTime>("timestamp");
            }
        }

        public Agent actor { get; set; }

        public Verb verb { get; set; }

        public StatementTarget target { get; set; }

        public Result result { get; set; }

        public Context context { get; set; }

        public DateTime? timestamp { get; set; }

        public override JObject ToJObject(TCAPIVersion version)
        {
            var resultObject = new JObject();

            if (actor != null)
            {
                resultObject.Add("actor", actor.ToJObject(version));
            }

            if (verb != null)
            {
                resultObject.Add("verb", verb.ToJObject(version));
            }

            if (target != null)
            {
                resultObject.Add("object", target.ToJObject(version));
            }

            if (result != null)
            {
                resultObject.Add("result", result.ToJObject(version));
            }

            if (context != null)
            {
                resultObject.Add("context", context.ToJObject(version));
            }

            if (timestamp != null)
            {
                resultObject.Add("timestamp", timestamp.Value.ToString(ISODateTimeFormat, CultureInfo.InvariantCulture));
            }

            return resultObject;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[StatementBase: actor={actor}, verb={verb}, target={target}, result={result}, context={context}, timestamp={timestamp}]";
        }
    }
}
