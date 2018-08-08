using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinCan;

namespace TinCanTests
{
    [TestFixture]
    class ContextTest
    {
        [TestCase(false)]
        [TestCase(true)]
        public void TestGroupInstructor(bool isGroup)
        {
            // Build our test JObject
            Context exampleContext = BuildTextContext();
            if (isGroup)
            {
                exampleContext.instructor = new Group();
            }
            else
            {
                exampleContext.instructor = new Agent();
            }
            JObject contextObj = exampleContext.ToJObject();

            // Ensure that Context.instructor is the correct Type
            Context testContext = new Context(contextObj);
            Assert.IsTrue(testContext.instructor is Agent);
            Assert.AreEqual(isGroup, testContext.instructor is Group);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestGroupTeam(bool isGroup)
        {
            // Build our test JObject
            Context exampleContext = BuildTextContext();
            if (isGroup)
            {
                exampleContext.team = new Group();
            }
            else
            {
                exampleContext.team = new Agent();
            }
            JObject contextObj = exampleContext.ToJObject();
            
            // Ensure that Context.instructor is the correct Type
            Context testContext = new Context(contextObj);
            Assert.IsTrue(testContext.team is Agent);
            Assert.AreEqual(isGroup, testContext.team is Group);
        }

        private Context BuildTextContext()
        {
            Guid registration = new Guid("42c0855b-8f64-47f3-b0e2-3f337930045a");
            ContextActivities contextActivities = new ContextActivities();
            string revision = "";
            string platform = "";
            string language = "";
            StatementRef statement = new StatementRef();
            TinCan.Extensions extensions = new TinCan.Extensions();

            Context context = new Context();
            context.registration = registration;
            context.contextActivities = contextActivities;
            context.revision = revision;
            context.platform = platform;
            context.language = language;
            context.statement = statement;
            context.extensions = extensions;

            return context;
        }
    }
}
