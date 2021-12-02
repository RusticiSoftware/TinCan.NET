// <copyright file="VerbTest.cs" company="Float">
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
    using Xunit;
    using Newtonsoft.Json.Linq;
    using TinCan;
    using TinCan.Json;

    public class VerbTest
    {
        [Fact]
        public void TestEmptyCtr()
        {
            var obj = new Verb();
            Assert.IsType<Verb>(obj);
            Assert.Null(obj.id);
            Assert.Null(obj.display);
            Assert.Equal("{}", obj.ToJSON());
        }

        [Fact]
        public void TestJObjectCtr()
        {
            const string id = "http://adlnet.gov/expapi/verbs/experienced";

            var cfg = new JObject
            {
                { "id", id },
            };

            var obj = new Verb(cfg);
            Assert.IsType<Verb>(obj);
            Assert.Equal(obj.ToJSON(), $"{{\"id\":\"{id}\"}}");
        }

        [Fact]
        public void TestStringOfJSONCtr()
        {
            var json = $"{{\"id\":\"http://adlnet.gov/expapi/verbs/experienced\"}}";
            var strOfJson = new StringOfJSON(json);

            var obj = new Verb(strOfJson);
            Assert.IsType<Verb>(obj);
            Assert.Equal(obj.ToJSON(), json);
        }
    }
}
