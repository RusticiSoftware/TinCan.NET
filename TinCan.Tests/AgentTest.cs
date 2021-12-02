// <copyright file="AgentTest.cs" company="Float">
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
    using System;

    public class AgentTest
    {
        [Fact]
        public void TestEmptyCtr()
        {
            var obj = new Agent();
            Assert.IsType<Agent>(obj);
            Assert.Null(obj.mbox);
            Assert.Equal("{\"objectType\":\"Agent\"}", obj.ToJSON(), true);
        }

        [Fact]
        public void TestJObjectCtr()
        {
            const string mbox = "mailto:tincancsharp@tincanapi.com";

            var cfg = new JObject
            {
                { "mbox", mbox },
            };

            var obj = new Agent(cfg);
            Assert.IsType<Agent>(obj);
            Assert.Equal(obj.mbox, mbox);
        }

        [Fact]
        public void TestStringOfJSONCtr()
        {
            const string mbox = "mailto:tincancsharp@tincanapi.com";
            var strOfJson = new StringOfJSON($"{{\"mbox\":\"{mbox}\"}}");

            var obj = new Agent(strOfJson);
            Assert.IsType<Agent>(obj);
            Assert.Equal(obj.mbox, mbox);
        }

        [Fact]
        public void TestAgentAccountHomepage()
        {
            var serverText = "https://gowithfloat.com";
            var server = new Uri(serverText);
            var agent = new Agent
            {
                account = new AgentAccount
                {
                    name = "Float bot",
                    homePage = server,
                }
            };
            Assert.IsType<Agent>(agent);
            Assert.Equal(agent.account.homePage.AbsoluteUri, $"{serverText}/");

            var jAgent = agent.ToJObject();
            Assert.Equal(jAgent["account"]["homePage"], serverText);
        }
    }
}
