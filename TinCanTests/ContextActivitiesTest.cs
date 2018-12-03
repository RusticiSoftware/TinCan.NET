/*
    Copyright 2018 Rustici Software

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
    using System.Collections.Generic;
    using NUnit.Framework;
    using TinCan;
    using TinCan.Json;

    [TestFixture]
    class ContextActivitiesTest
    {
        private Activity sampleActivity1 = new Activity
        {
            id = "http://0.bar/"
        };

        private Activity sampleActivity2 = new Activity
        {
            id = "http://1.bar/"
        };

        [Test]
        public void ConstructorWithObject()
        {
            var json = "{" +
                "\"parent\": " + sampleActivity1.ToJSON() + "," +
                "\"grouping\": " + sampleActivity1.ToJSON() + "," +
                "\"category\": " + sampleActivity1.ToJSON() + "," +
                "\"other\": " + sampleActivity1.ToJSON() +
            "}";

            ContextActivities contextActivities = new ContextActivities(new StringOfJSON(json));

            ValidateActivityList(contextActivities.parent, 1);
            ValidateActivityList(contextActivities.grouping, 1);
            ValidateActivityList(contextActivities.category, 1);
            ValidateActivityList(contextActivities.other, 1);
        }

        [Test]
        public void ConstructorWithArray()
        {
            var json = "{" +
                "\"parent\": [" + sampleActivity1.ToJSON() + ", " + sampleActivity2.ToJSON() + "]," +
                "\"grouping\": [" + sampleActivity1.ToJSON() + ", " + sampleActivity2.ToJSON() + "]," +
                "\"category\": [" + sampleActivity1.ToJSON() + ", " + sampleActivity2.ToJSON() + "]," +
                "\"other\": [" + sampleActivity1.ToJSON() + ", " + sampleActivity2.ToJSON() + "]" +
            "}";

            ContextActivities contextActivities = new ContextActivities(new StringOfJSON(json));

            ValidateActivityList(contextActivities.parent, 2);
            ValidateActivityList(contextActivities.grouping, 2);
            ValidateActivityList(contextActivities.category, 2);
            ValidateActivityList(contextActivities.other, 2);
        }

        private void ValidateActivityList(List<Activity> list, int expectedLength)
        {
            Assert.IsTrue(list.Count == expectedLength);

            for (int i = 0; i < list.Count; i++)
            {
                Assert.IsTrue(list[i].id == "http://" + i + ".bar/");
            }
        }
    }
}
