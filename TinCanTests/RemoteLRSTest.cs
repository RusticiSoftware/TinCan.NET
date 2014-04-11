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
    using NUnit.Framework;
    using Newtonsoft.Json.Linq;
    using TinCan;
    using TinCan.json;

    [TestFixture]
    class RemoteLRSTest
    {
        [Test]
        public void TestEmptyCtr()
        {
            var obj = new RemoteLRS();
            Assert.IsInstanceOf<RemoteLRS>(obj);
            Assert.IsNull(obj.endpoint);
            Assert.IsNull(obj.auth);
            Assert.IsNull(obj.version);
        }

        [Test]
        public void TestAbout()
        {
            var lrs = new RemoteLRS();
            lrs.version = TCAPIVersion.latest();
            lrs.endpoint = new Uri("http://cloud.scorm.com/tc/3HYPTQLAI9/sandbox/");

            TinCan.LRSResponse.About lrsRes = lrs.About();
            Assert.IsTrue(lrsRes.success);
            Console.WriteLine(lrsRes.content.toJSON(true));
        }

        [Test]
        public void TestAboutFailure()
        {
            var lrs = new RemoteLRS();
            lrs.version = TCAPIVersion.latest();
            lrs.endpoint = new Uri("http://cloud.scorm.com/tc/HYPTQLAI9/sandbox/");

            TinCan.LRSResponse.About lrsRes = lrs.About();
            Assert.IsFalse(lrsRes.success);
            Console.WriteLine(lrsRes.httpException);
        }
    }
}