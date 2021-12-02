// <copyright file="ResultTest.cs" company="Float">
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

    public class ResultTest
    {
        [Fact]
        public void TestEmptyCtr()
        {
            var obj = new Result();
            Assert.IsType<Result>(obj);
            Assert.Null(obj.completion);
            Assert.Null(obj.success);
            Assert.Null(obj.response);
            Assert.Null(obj.duration);
            Assert.Null(obj.score);
            Assert.Null(obj.extensions);
            Assert.Equal("{}", obj.ToJSON(), true);
        }

        [Fact]
        public void TestJObjectCtr()
        {
            var cfg = new JObject
            {
                { "completion", true },
                { "success", true },
                { "response", "Yes" },
            };

            var obj = new Result(cfg);
            Assert.IsType<Result>(obj);
            Assert.True(obj.completion);
            Assert.True(obj.success);
            Assert.Equal("Yes", obj.response);
        }

        [Fact]
        public void TestStringOfJSONCtr()
        {
            var strOfJson = new StringOfJSON("{\"success\": true, \"completion\": true, \"response\": \"Yes\"}");
            var obj = new Result(strOfJson);
            Assert.IsType<Result>(obj);
            Assert.True(obj.success);
            Assert.True(obj.completion);
            Assert.Equal("Yes", obj.response);
        }
    }
}
