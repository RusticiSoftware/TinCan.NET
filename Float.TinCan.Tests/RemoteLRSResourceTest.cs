// <copyright file="RemoteLRSResourceTest.cs" company="Float">
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
    using System.Threading.Tasks;
    using Xunit;
    using TinCan;
    using TinCan.Documents;

    public class RemoteLRSResourceTest
    {
        readonly RemoteLRS lrs;

        public RemoteLRSResourceTest()
        {
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

        [Fact]
        public async Task TestAbout()
        {
            var lrsRes = await lrs.About();
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestAboutFailure()
        {
            lrs.endpoint = new Uri("http://cloud.scorm.com/tc/3TQLAI9/sandbox/");

            var lrsRes = await lrs.About();
            Assert.False(lrsRes.success);
        }

        [Fact]
        public async Task TestSaveStatement()
        {
            var statement = new Statement
            {
                actor = Support.agent,
                verb = Support.verb,
                target = Support.activity,
            };

            var lrsRes = await lrs.SaveStatement(statement);
            Assert.True(lrsRes.success);
            Assert.Equal(statement, lrsRes.content);
            Assert.NotNull(lrsRes.content.id);
        }

        [Fact]
        public async Task TestSaveStatementWithID()
        {
            var statement = new Statement();
            statement.Stamp();
            statement.actor = Support.agent;
            statement.verb = Support.verb;
            statement.target = Support.activity;

            var lrsRes = await lrs.SaveStatement(statement);
            Assert.True(lrsRes.success);
            Assert.Equal(statement, lrsRes.content);
        }

        [Fact]
        public async Task TestSaveStatementStatementRef()
        {
            var statement = new Statement();
            statement.Stamp();
            statement.actor = Support.agent;
            statement.verb = Support.verb;
            statement.target = Support.statementRef;

            var lrsRes = await lrs.SaveStatement(statement);
            Assert.True(lrsRes.success);
            Assert.Equal(statement, lrsRes.content);
        }

        [Fact]
        public async Task TestSaveStatementSubStatement()
        {
            var statement = new Statement();
            statement.Stamp();
            statement.actor = Support.agent;
            statement.verb = Support.verb;
            statement.target = Support.subStatement;

            var lrsRes = await lrs.SaveStatement(statement);
            Assert.True(lrsRes.success);
            Assert.Equal(statement, lrsRes.content);
        }

        [Fact]
        public async Task TestVoidStatement()
        {
            var toVoid = Guid.NewGuid();
            var lrsRes = await lrs.VoidStatement(toVoid, Support.agent);

            Assert.True(lrsRes.success, "LRS response successful");
            Assert.Equal(new Uri("http://adlnet.gov/expapi/verbs/voided"), lrsRes.content.verb.id);
            Assert.Equal(toVoid, ((StatementRef)lrsRes.content.target).id);
        }

        [Fact]
        public async Task TestSaveStatements()
        {
            var statement1 = new Statement
            {
                actor = Support.agent,
                verb = Support.verb,
                target = Support.parent,
            };

            var statement2 = new Statement
            {
                actor = Support.agent,
                verb = Support.verb,
                target = Support.activity,
                context = Support.context,
            };

            var statements = new List<Statement>
            {
                statement1,
                statement2,
            };

            var lrsRes = await lrs.SaveStatements(statements);
            Assert.True(lrsRes.success);
            // TODO: check statements match and ids not null
        }

        [Fact]
        public async Task TestRetrieveStatement()
        {
            var statement = new Statement
            {
                actor = Support.agent,
                verb = Support.verb,
                target = Support.activity,
                context = Support.context,
                result = Support.result,
            };

            statement.Stamp();

            var saveRes = await lrs.SaveStatement(statement);

            if (saveRes.success)
            {
                var retRes = await lrs.RetrieveStatement(saveRes.content.id.Value);
                Assert.True(retRes.success);
            }
        }

        [Fact]
        public async Task TestQueryStatements()
        {
            var query = new StatementsQuery
            {
                agent = Support.agent,
                verbId = Support.verb.id,
                activityId = Support.parent.id,
                relatedActivities = true,
                relatedAgents = true,
                format = StatementsQueryResultFormat.IDS,
                limit = 10,
            };

            var lrsRes = await lrs.QueryStatements(query);
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestMoreStatements()
        {
            var query = new StatementsQuery
            {
                format = StatementsQueryResultFormat.IDS,
                limit = 2,
            };

            var queryRes = await lrs.QueryStatements(query);
            
            if (queryRes.success && queryRes.content.more != null)
            {
                var moreRes = await lrs.MoreStatements(queryRes.content);
                Assert.True(moreRes.success);
            }
        }

        [Fact]
        public async Task TestRetrieveStateIds()
        {
            var lrsRes = await lrs.RetrieveStateIds(Support.activity, Support.agent);
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestRetrieveState()
        {
            var lrsRes = await lrs.RetrieveState("test", Support.activity, Support.agent);
            Assert.True(lrsRes.success);
            Assert.IsType<StateDocument>(lrsRes.content);
        }

        [Fact]
        public async Task TestSaveState()
        {
            var doc = new StateDocument
            {
                activity = Support.activity,
                agent = Support.agent,
                id = "test",
                content = System.Text.Encoding.UTF8.GetBytes("Test value"),
                contentType = "application/json;charset=utf8"
            };

            var lrsRes = await lrs.SaveState(doc);
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestDeleteState()
        {
            var doc = new StateDocument
            {
                activity = Support.activity,
                agent = Support.agent,
                id = "test",
            };

            var lrsRes = await lrs.DeleteState(doc);
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestClearState()
        {
            var lrsRes = await lrs.ClearState(Support.activity, Support.agent);
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestRetrieveActivityProfileIds()
        {
            var lrsRes = await lrs.RetrieveActivityProfileIds(Support.activity);
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestRetrieveActivityProfile()
        {
            var lrsRes = await lrs.RetrieveActivityProfile("test", Support.activity);
            Assert.True(lrsRes.success);
            Assert.IsType<ActivityProfileDocument>(lrsRes.content);
        }

        [Fact]
        public async Task TestSaveActivityProfile()
        {
            var doc = new ActivityProfileDocument
            {
                activity = Support.activity,
                id = "test",
                content = System.Text.Encoding.UTF8.GetBytes("Test value"),
            };

            var lrsRes = await lrs.SaveActivityProfile(doc);
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestDeleteActivityProfile()
        {
            var doc = new ActivityProfileDocument
            {
                activity = Support.activity,
                id = "test",
            };

            var lrsRes = await lrs.DeleteActivityProfile(doc);
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestRetrieveAgentProfileIds()
        {
            var lrsRes = await lrs.RetrieveAgentProfileIds(Support.agent);
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestRetrieveAgentProfile()
        {
            var lrsRes = await lrs.RetrieveAgentProfile("test", Support.agent);
            Assert.True(lrsRes.success);
            Assert.IsType<AgentProfileDocument>(lrsRes.content);
        }

        [Fact]
        public async Task TestSaveAgentProfile()
        {
            var doc = new AgentProfileDocument
            {
                agent = Support.agent,
                id = "test",
                content = System.Text.Encoding.UTF8.GetBytes("Test value"),
            };

            var lrsRes = await lrs.SaveAgentProfile(doc);
            Assert.True(lrsRes.success);
        }

        [Fact]
        public async Task TestDeleteAgentProfile()
        {
            var doc = new AgentProfileDocument
            {
                agent = Support.agent,
                id = "test",
            };

            var lrsRes = await lrs.DeleteAgentProfile(doc);
            Assert.True(lrsRes.success);
        }
    }
}
