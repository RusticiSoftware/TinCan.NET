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
    using System.Xml;
    using NUnit.Framework;
    using Newtonsoft.Json.Linq;
    using TinCan;
    using TinCan.Documents;
    using TinCan.Json;
    using TinCan.LRSResponses;

    [TestFixture]
    class RemoteLRSResourceTest
    {
        RemoteLRS lrs;

        [SetUp]
        public void Init()
        {
            Console.WriteLine("Running " + TestContext.CurrentContext.Test.FullName);

            //
            // these are credentials used by the other OSS libs when building via Travis-CI
            // so are okay to include in the repository, if you wish to have access to the
            // results of the test suite then supply your own endpoint, username, and password
            //
            lrs = new RemoteLRS(
                "https://cloud.scorm.com/tc/U2S4SI5FY0/sandbox/",
                "Nja986GYE1_XrWMmFUE",
                "Bd9lDr1kjaWWY6RID_4"
            );
        }

        [Test]
        public void TestAbout()
        {
            AboutLRSResponse lrsRes = lrs.About();
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestAboutFailure()
        {
            lrs.endpoint = new Uri("http://cloud.scorm.com/tc/3TQLAI9/sandbox/");

            AboutLRSResponse lrsRes = lrs.About();
            Assert.IsFalse(lrsRes.success);
            Console.WriteLine("TestAboutFailure - errMsg: " + lrsRes.errMsg);
        }

        [Test]
        public void TestSaveStatement()
        {
            var statement = new Statement();
            statement.actor = Support.agent;
            statement.verb = Support.verb;
            statement.target = Support.activity;

            StatementLRSResponse lrsRes = lrs.SaveStatement(statement);
            Assert.IsTrue(lrsRes.success);
            Assert.AreEqual(statement, lrsRes.content);
            Assert.IsNotNull(lrsRes.content.id);
        }

        [Test]
        public void TestSaveStatementWithID()
        {
            var statement = new Statement();
            statement.Stamp();
            statement.actor = Support.agent;
            statement.verb = Support.verb;
            statement.target = Support.activity;

            StatementLRSResponse lrsRes = lrs.SaveStatement(statement);
            Assert.IsTrue(lrsRes.success);
            Assert.AreEqual(statement, lrsRes.content);
        }

        [Test]
        public void TestSaveStatementStatementRef()
        {
            var statement = new Statement();
            statement.Stamp();
            statement.actor = Support.agent;
            statement.verb = Support.verb;
            statement.target = Support.statementRef;

            StatementLRSResponse lrsRes = lrs.SaveStatement(statement);
            Assert.IsTrue(lrsRes.success);
            Assert.AreEqual(statement, lrsRes.content);
        }

        [Test]
        public void TestSaveStatementSubStatement()
        {
            var statement = new Statement();
            statement.Stamp();
            statement.actor = Support.agent;
            statement.verb = Support.verb;
            statement.target = Support.subStatement;

            Console.WriteLine(statement.ToJSON(true));

            StatementLRSResponse lrsRes = lrs.SaveStatement(statement);
            Assert.IsTrue(lrsRes.success);
            Assert.AreEqual(statement, lrsRes.content);
        }

        [Test]
        public void TestVoidStatement()
        {
            Guid toVoid = Guid.NewGuid();
            StatementLRSResponse lrsRes = lrs.VoidStatement(toVoid, Support.agent);

            Assert.IsTrue(lrsRes.success, "LRS response successful");
            Assert.AreEqual(new Uri("http://adlnet.gov/expapi/verbs/voided"), lrsRes.content.verb.id, "voiding statement uses voided verb");
            Assert.AreEqual(toVoid, ((StatementRef) lrsRes.content.target).id, "voiding statement target correct id");
        }

        [Test]
        public void TestSaveStatements()
        {
            var statement1 = new Statement();
            statement1.actor = Support.agent;
            statement1.verb = Support.verb;
            statement1.target = Support.parent;

            var statement2 = new Statement();
            statement2.actor = Support.agent;
            statement2.verb = Support.verb;
            statement2.target = Support.activity;
            statement2.context = Support.context;

            var statements = new List<Statement>();
            statements.Add(statement1);
            statements.Add(statement2);

            StatementsResultLRSResponse lrsRes = lrs.SaveStatements(statements);
            Assert.IsTrue(lrsRes.success);
            // TODO: check statements match and ids not null
        }

        [Test]
        public void TestRetrieveStatement()
        {
            var statement = new TinCan.Statement();
            statement.Stamp();
            statement.actor = Support.agent;
            statement.verb = Support.verb;
            statement.target = Support.activity;
            statement.context = Support.context;
            statement.result = Support.result;

            StatementLRSResponse saveRes = lrs.SaveStatement(statement);
            if (saveRes.success)
            {
                StatementLRSResponse retRes = lrs.RetrieveStatement(saveRes.content.id.Value);
                Assert.IsTrue(retRes.success);
                Console.WriteLine("TestRetrieveStatement - statement: " + retRes.content.ToJSON(true));
            }
            else
            {
                // TODO: skipped?
            }
        }

        [Test]
        public void TestQueryStatements()
        {
            var query = new TinCan.StatementsQuery();
            query.agent = Support.agent;
            query.verbId = Support.verb.id;
            query.activityId = Support.parent.id;
            query.relatedActivities = true;
            query.relatedAgents = true;
            query.format = StatementsQueryResultFormat.IDS;
            query.limit = 10;

            StatementsResultLRSResponse lrsRes = lrs.QueryStatements(query);
            Assert.IsTrue(lrsRes.success);
            Console.WriteLine("TestQueryStatements - statement count: " + lrsRes.content.statements.Count);
        }

        [Test]
        public void TestMoreStatements()
        {
            var query = new TinCan.StatementsQuery();
            query.format = StatementsQueryResultFormat.IDS;
            query.limit = 2;

            StatementsResultLRSResponse queryRes = lrs.QueryStatements(query);
            if (queryRes.success && queryRes.content.more != null)
            {
                StatementsResultLRSResponse moreRes = lrs.MoreStatements(queryRes.content);
                Assert.IsTrue(moreRes.success);
                Console.WriteLine("TestMoreStatements - statement count: " + moreRes.content.statements.Count);
            }
            else
            {
                // TODO: skipped?
            }
        }

        [Test]
        public void TestRetrieveStateIds()
        {
            ProfileKeysLRSResponse lrsRes = lrs.RetrieveStateIds(Support.activity, Support.agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveState()
        {
            StateLRSResponse lrsRes = lrs.RetrieveState("test", Support.activity, Support.agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestSaveState()
        {
            var doc = new StateDocument();
            doc.activity = Support.activity;
            doc.agent = Support.agent;
            doc.id = "test";
            doc.content = System.Text.Encoding.UTF8.GetBytes("Test value");

            LRSResponse lrsRes = lrs.SaveState(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestDeleteState()
        {
            var doc = new StateDocument();
            doc.activity = Support.activity;
            doc.agent = Support.agent;
            doc.id = "test";

            LRSResponse lrsRes = lrs.DeleteState(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestClearState()
        {
            LRSResponse lrsRes = lrs.ClearState(Support.activity, Support.agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveActivityProfileIds()
        {
            ProfileKeysLRSResponse lrsRes = lrs.RetrieveActivityProfileIds(Support.activity);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveActivityProfile()
        {
            ActivityProfileLRSResponse lrsRes = lrs.RetrieveActivityProfile("test", Support.activity);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestSaveActivityProfile()
        {
            var doc = new ActivityProfileDocument();
            doc.activity = Support.activity;
            doc.id = "test";
            doc.content = System.Text.Encoding.UTF8.GetBytes("Test value");

            LRSResponse lrsRes = lrs.SaveActivityProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestDeleteActivityProfile()
        {
            var doc = new ActivityProfileDocument();
            doc.activity = Support.activity;
            doc.id = "test";

            LRSResponse lrsRes = lrs.DeleteActivityProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveAgentProfileIds()
        {
            ProfileKeysLRSResponse lrsRes = lrs.RetrieveAgentProfileIds(Support.agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveAgentProfile()
        {
            AgentProfileLRSResponse lrsRes = lrs.RetrieveAgentProfile("test", Support.agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestSaveAgentProfile()
        {
            var doc = new AgentProfileDocument();
            doc.agent = Support.agent;
            doc.id = "test";
            doc.content = System.Text.Encoding.UTF8.GetBytes("Test value");

            LRSResponse lrsRes = lrs.SaveAgentProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestDeleteAgentProfile()
        {
            var doc = new AgentProfileDocument();
            doc.agent = Support.agent;
            doc.id = "test";

            LRSResponse lrsRes = lrs.DeleteAgentProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }
    }
}