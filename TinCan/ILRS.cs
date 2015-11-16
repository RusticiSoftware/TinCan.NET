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
using TinCan.Documents;
using TinCan.LRSResponses;

namespace TinCan
{
    public interface ILRS
    {
        AboutLRSResponse About();

        StatementLRSResponse SaveStatement(Statement statement);
        StatementLRSResponse VoidStatement(Guid id, Agent agent);
        StatementsResultLRSResponse SaveStatements(List<Statement> statements);
        StatementLRSResponse RetrieveStatement(Guid id);
        StatementLRSResponse RetrieveVoidedStatement(Guid id);
        StatementsResultLRSResponse QueryStatements(StatementsQuery query);
        StatementsResultLRSResponse MoreStatements(StatementsResult result);

        ProfileKeysLRSResponse RetrieveStateIds(Activity activity, Agent agent, Nullable<Guid> registration = null);
        StateLRSResponse RetrieveState(String id, Activity activity, Agent agent, Nullable<Guid> registration = null);
        LRSResponse SaveState(StateDocument state);
        LRSResponse DeleteState(StateDocument state);
        LRSResponse ClearState(Activity activity, Agent agent, Nullable<Guid> registration = null);

        ProfileKeysLRSResponse RetrieveActivityProfileIds(Activity activity);
        ActivityProfileLRSResponse RetrieveActivityProfile(String id, Activity activity);
        LRSResponse SaveActivityProfile(ActivityProfileDocument profile);
        LRSResponse DeleteActivityProfile(ActivityProfileDocument profile);

        ProfileKeysLRSResponse RetrieveAgentProfileIds(Agent agent);
        AgentProfileLRSResponse RetrieveAgentProfile(String id, Agent agent);
        LRSResponse SaveAgentProfile(AgentProfileDocument profile);
        LRSResponse DeleteAgentProfile(AgentProfileDocument profile);
    }
}
