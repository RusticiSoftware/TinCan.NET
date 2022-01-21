// <copyright file="Support.cs" company="Float">
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

namespace TinCan.Tests
{
    using System;
    using System.Collections.Generic;
    using TinCan;

    static class Support
    {
        public static Agent agent;
        public static Verb verb;
        public static Activity activity;
        public static Activity parent;
        public static Context context;
        public static Result result;
        public static Score score;
        public static StatementRef statementRef;
        public static SubStatement subStatement;

        static Support()
        {
            agent = new Agent
            {
                mbox = "mailto:tincancsharp@tincanapi.com"
            };

            verb = new Verb(new Uri("http://adlnet.gov/expapi/verbs/experienced"));
            verb.display.Add("en-US", "experienced");

            activity = new Activity
            {
                id = new Uri("http://tincanapi.com/TinCanCSharp/Test/Unit/0"),
            };

            activity.definition.type = new Uri("http://id.tincanapi.com/activitytype/unit-test");
            activity.definition.name = new LanguageMap
            {
                { "en-US", "Tin Can C# Tests: Unit 0" },
            };
            activity.definition.description = new LanguageMap
            {
                { "en-US", "Unit test 0 in the test suite for the Tin Can C# library." },
            };

            parent = new Activity
            {
                id = new Uri("http://tincanapi.com/TinCanCSharp/Test"),
                definition = new ActivityDefinition
                {
                    type = new Uri("http://id.tincanapi.com/activitytype/unit-test-suite"),
                    name = new LanguageMap(),
                },
            };

            parent.definition.name.Add("en-US", "Tin Can C# Tests");

            parent.definition.description = new LanguageMap
            {
                { "en-US", "Unit test suite for the Tin Can C# library." },
            };

            statementRef = new StatementRef(Guid.NewGuid());

            context = new Context
            {
                registration = Guid.NewGuid(),
                statement = statementRef,
                contextActivities = new ContextActivities
                {
                    parent = new List<Activity>(),
                },
            };

            context.contextActivities.parent.Add(parent);

            score = new Score
            {
                raw = 97,
                scaled = 0.97,
                max = 100,
                min = 0,
            };

            result = new Result
            {
                score = score,
                success = true,
                completion = true,
                duration = new TimeSpan(1, 2, 16, 43),
            };

            subStatement = new SubStatement
            {
                actor = agent,
                verb = verb,
                target = parent,
            };
        }
    }
}
