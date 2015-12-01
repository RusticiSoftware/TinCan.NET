/*
    Copyright 2014 Rustici Software

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/
namespace TinCanTests
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

        static Support () {
            agent = new Agent();
            agent.mbox = "mailto:tincancsharp@tincanapi.com";

            verb = new Verb("http://adlnet.gov/expapi/verbs/experienced");
            verb.display = new LanguageMap();
            verb.display.Add("en-US", "experienced");

            activity = new Activity();
            activity.id = new Uri("http://tincanapi.com/TinCanCSharp/Test/Unit/0");
            activity.definition = new ActivityDefinition();
            activity.definition.type = new Uri("http://id.tincanapi.com/activitytype/unit-test");
            activity.definition.name = new LanguageMap();
            activity.definition.name.Add("en-US", "Tin Can C# Tests: Unit 0");
            activity.definition.description = new LanguageMap();
            activity.definition.description.Add("en-US", "Unit test 0 in the test suite for the Tin Can C# library.");

            activity.definition.interactionType = InteractionType.Choice;
            activity.definition.choices = new List<InteractionComponent>();

            for (int i = 1; i <= 3; i++)
            {
                InteractionComponent interactionComponent = new InteractionComponent();
                
                interactionComponent.id = "choice-" + i.ToString();
                interactionComponent.description = new LanguageMap();
                interactionComponent.description.Add("en-US", "Choice " + i.ToString());

                activity.definition.choices.Add(interactionComponent);
            }

            activity.definition.correctResponsesPattern = new List<string>();

            for (int i = 1; i <= 2; i++)
            {
                activity.definition.correctResponsesPattern.Add("choice-" + i.ToString());                
            }            

            parent = new Activity();
            parent.id = new Uri("http://tincanapi.com/TinCanCSharp/Test");
            parent.definition = new ActivityDefinition();
            parent.definition.type = new Uri("http://id.tincanapi.com/activitytype/unit-test-suite");
            //parent.definition.moreInfo = new Uri("http://rusticisoftware.github.io/TinCanCSharp/");
            parent.definition.name = new LanguageMap();
            parent.definition.name.Add("en-US", "Tin Can C# Tests");
            parent.definition.description = new LanguageMap();
            parent.definition.description.Add("en-US", "Unit test suite for the Tin Can C# library.");

            statementRef = new StatementRef(Guid.NewGuid());

            context = new Context();
            context.registration = Guid.NewGuid();
            context.statement = statementRef;
            context.contextActivities = new ContextActivities();
            context.contextActivities.parent = new List<Activity>();
            context.contextActivities.parent.Add(parent);          

            score = new Score();
            score.raw = 97;
            score.scaled = 0.97;
            score.max = 100;
            score.min = 0;

            result = new Result();
            result.score = score;
            result.success = true;
            result.completion = true;
            result.duration = new TimeSpan(1, 2, 16, 43);
            result.response = "choice-2";

            subStatement = new SubStatement();
            subStatement.actor = agent;
            subStatement.verb = verb;
            subStatement.target = parent;
        }
    }
}