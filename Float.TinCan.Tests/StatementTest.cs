// <copyright file="StatementTest.cs" company="Float">
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

    public class StatementTest
    {
        [Fact]
        public void TestEmptyCtr()
        {
            var obj = new Statement();
            Assert.IsType<Statement>(obj);
            Assert.Null(obj.id);
            Assert.Null(obj.actor);
            Assert.Null(obj.verb);
            Assert.Null(obj.target);
            Assert.Null(obj.result);
            Assert.Null(obj.context);
            Assert.Null(obj.version);
            Assert.Null(obj.timestamp);
            Assert.Null(obj.stored);
            Assert.Equal("{\"version\":\"1.0.1\"}", obj.ToJSON(), true);
        }

        [Fact]
        public void TestJObjectCtrSubStatement()
        {
            var cfg = new JObject
            {
                { "actor", Support.agent.ToJObject() },
                { "verb", Support.verb.ToJObject() },
                { "object", Support.subStatement.ToJObject() },
            };

            var obj = new Statement(cfg);
            Assert.IsType<Statement>(obj);
            Assert.IsType<SubStatement>(obj.target);
        }
    }
}
