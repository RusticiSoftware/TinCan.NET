// <copyright file="ILRS.cs" company="Float">
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinCan.Documents;
using TinCan.LRSResponses;

namespace TinCan
{
    public interface ILRS
    {
        Task<AboutLRSResponse> About();

        Task<StatementLRSResponse> SaveStatement(Statement statement);

        Task<StatementLRSResponse> VoidStatement(Guid id, Agent agent);

        Task<StatementsResultLRSResponse> SaveStatements(List<Statement> statements);

        Task<StatementLRSResponse> RetrieveStatement(Guid id);

        Task<StatementLRSResponse> RetrieveVoidedStatement(Guid id);

        Task<StatementsResultLRSResponse> QueryStatements(StatementsQuery query);

        Task<StatementsResultLRSResponse> MoreStatements(StatementsResult statementsResult);

        Task<ProfileKeysLRSResponse> RetrieveStateIds(Activity activity, Agent agent, Guid? registration = null);

        Task<StateLRSResponse> RetrieveState(string id, Activity activity, Agent agent, Guid? registration = null);

        Task<LRSResponse> SaveState(StateDocument state);

        Task<LRSResponse> DeleteState(StateDocument state);

        Task<LRSResponse> ClearState(Activity activity, Agent agent, Guid? registration = null);

        Task<ProfileKeysLRSResponse> RetrieveActivityProfileIds(Activity activity);

        Task<ActivityProfileLRSResponse> RetrieveActivityProfile(string id, Activity activity);

        Task<LRSResponse> SaveActivityProfile(ActivityProfileDocument profile);

        Task<LRSResponse> DeleteActivityProfile(ActivityProfileDocument profile);

        Task<ProfileKeysLRSResponse> RetrieveAgentProfileIds(Agent agent);

        Task<AgentProfileLRSResponse> RetrieveAgentProfile(string id, Agent agent);

        Task<LRSResponse> SaveAgentProfile(AgentProfileDocument profile);

        Task<LRSResponse> ForceSaveAgentProfile(AgentProfileDocument profile);

        Task<LRSResponse> DeleteAgentProfile(AgentProfileDocument profile);
    }
}
