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
using System;
using System.Collections.Generic;

namespace TinCan
{
    interface LRS
    {
        TinCan.LRSResponse.About About();

        TinCan.LRSResponse.Statement SaveStatement(Statement statement);
        TinCan.LRSResponse.StatementsResult SaveStatements(List<Statement> statements);
        TinCan.LRSResponse.Statement RetrieveStatement(Guid id);
        TinCan.LRSResponse.Statement RetrieveVoidedStatement(Guid id);
        TinCan.LRSResponse.StatementsResult QueryStatements();
        TinCan.LRSResponse.StatementsResult MoreStatements(StatementsResult result);

        TinCan.LRSResponse.ProfileKeys RetrieveStateIds(Activity activity, Agent agent, Nullable<Guid> registration = null);
        TinCan.LRSResponse.State RetrieveState(String id, Activity activity, Agent agent, Nullable<Guid> registration = null);
        TinCan.LRSResponse.Base SaveState(Document.State profile);
        TinCan.LRSResponse.Base DeleteState(Document.State profile);
        TinCan.LRSResponse.Base ClearState(Activity activity, Agent agent, Nullable<Guid> registration = null);

        TinCan.LRSResponse.ProfileKeys RetrieveActivityProfileIds(Activity activity);
        TinCan.LRSResponse.ActivityProfile RetrieveActivityProfile(String id, Activity activity);
        TinCan.LRSResponse.Base SaveActivityProfile(Document.ActivityProfile profile);
        TinCan.LRSResponse.Base DeleteActivityProfile(Document.ActivityProfile profile);

        TinCan.LRSResponse.ProfileKeys RetrieveAgentProfileIds(Agent agent);
        TinCan.LRSResponse.AgentProfile RetrieveAgentProfile(String id, Agent agent);
        TinCan.LRSResponse.Base SaveAgentProfile(Document.AgentProfile profile);
        TinCan.LRSResponse.Base DeleteAgentProfile(Document.AgentProfile profile);
    }
}
