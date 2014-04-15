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
    class RemoteLRSResourceTest
    {
        RemoteLRS lrs;
        Agent agent;
        Activity activity;

        [SetUp]
        public void Init()
        {
            Console.WriteLine("Running " + TestContext.CurrentContext.Test.FullName);
            lrs = new RemoteLRS();
            lrs.version = TCAPIVersion.latest();
            lrs.endpoint = new Uri("http://cloud.scorm.com/tc/3HYPTQLAI9/sandbox/");
            lrs.auth = "Basic X3RKaUVSRU50TjJQbG5hRy1iMDpWczczMWM3cVhNX3dSVXNaRnUw";

            agent = new Agent();
            agent.mbox = "mailto:tincancsharp@tincanapi.com";

            activity = new Activity();
            activity.id = new Uri("http://tincanapi.com/TinCanCSharp/Test/Activity");
        }

        [Test]
        public void TestAbout()
        {
            TinCan.LRSResponse.About lrsRes = lrs.About();
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestAboutFailure()
        {
            lrs.endpoint = new Uri("http://cloud.scorm.com/tc/3TQLAI9/sandbox/");

            TinCan.LRSResponse.About lrsRes = lrs.About();
            Assert.IsFalse(lrsRes.success);
        }

        [Test]
        public void TestSaveStatement()
        {
            var statement = new TinCan.Statement();
            statement.actor = agent;
            statement.verb = new TinCan.Verb("http://adlnet.gov/expapi/verbs/experienced");
            statement.target = activity;

            TinCan.LRSResponse.Statement lrsRes = lrs.SaveStatement(statement);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveStatement()
        {
            TinCan.LRSResponse.Statement lrsRes = lrs.RetrieveStatement(new Guid("20ae0c9e-4658-4e0a-9320-381b7c49bb09"));
            Assert.IsTrue(lrsRes.success);
            Console.WriteLine("TestRetrieveStatement - statement: " + lrsRes.content.toJSON(lrs.version));
        }

        [Test]
        public void TestRetrieveStateIds()
        {
            TinCan.LRSResponse.ProfileKeys lrsRes = lrs.RetrieveStateIds(activity, agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveState()
        {
            TinCan.LRSResponse.State lrsRes = lrs.RetrieveState("test", activity, agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestSaveState()
        {
            var doc = new TinCan.Document.State();
            doc.activity = activity;
            doc.agent = agent;
            doc.id = "test";
            doc.content = System.Text.Encoding.UTF8.GetBytes("Test value");

            TinCan.LRSResponse.Base lrsRes = lrs.SaveState(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestDeleteState()
        {
            var doc = new TinCan.Document.State();
            doc.activity = activity;
            doc.agent = agent;
            doc.id = "test";

            TinCan.LRSResponse.Base lrsRes = lrs.DeleteState(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestClearState()
        {
            TinCan.LRSResponse.Base lrsRes = lrs.ClearState(activity, agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveActivityProfileIds()
        {
            TinCan.LRSResponse.ProfileKeys lrsRes = lrs.RetrieveActivityProfileIds(activity);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveActivityProfile()
        {
            TinCan.LRSResponse.ActivityProfile lrsRes = lrs.RetrieveActivityProfile("test", activity);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestSaveActivityProfile()
        {
            var doc = new TinCan.Document.ActivityProfile();
            doc.activity = activity;
            doc.id = "test";
            doc.content = System.Text.Encoding.UTF8.GetBytes("Test value");

            TinCan.LRSResponse.Base lrsRes = lrs.SaveActivityProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestDeleteActivityProfile()
        {
            var doc = new TinCan.Document.ActivityProfile();
            doc.activity = activity;
            doc.id = "test";

            TinCan.LRSResponse.Base lrsRes = lrs.DeleteActivityProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveAgentProfileIds()
        {
            TinCan.LRSResponse.ProfileKeys lrsRes = lrs.RetrieveAgentProfileIds(agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveAgentProfile()
        {
            TinCan.LRSResponse.AgentProfile lrsRes = lrs.RetrieveAgentProfile("test", agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestSaveAgentProfile()
        {
            var doc = new TinCan.Document.AgentProfile();
            doc.agent = agent;
            doc.id = "test";
            doc.content = System.Text.Encoding.UTF8.GetBytes("Test value");

            TinCan.LRSResponse.Base lrsRes = lrs.SaveAgentProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestDeleteAgentProfile()
        {
            var doc = new TinCan.Document.AgentProfile();
            doc.agent = agent;
            doc.id = "test";

            TinCan.LRSResponse.Base lrsRes = lrs.DeleteAgentProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }
    }
}