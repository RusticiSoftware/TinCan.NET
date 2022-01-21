// <copyright file="RemoteLRS.cs" company="Float">
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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TinCan.Documents;
using TinCan.LRSResponses;

namespace TinCan
{
    public class RemoteLRS : ILRS
    {
        readonly SemaphoreSlim makeRequestSemaphore = new SemaphoreSlim(1, 1);

        readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteLRS"/> class.
        /// </summary>
        public RemoteLRS()
        {
        }

        public RemoteLRS(Uri endpoint, TCAPIVersion version, string username, string password)
        {
            Contract.Requires(endpoint != null);
            Contract.Requires(version != null);
            this.endpoint = endpoint;
            this.version = version;
            SetAuth(username, password);
        }

        public RemoteLRS(string endpoint, TCAPIVersion version, string username, string password) : this(new Uri(endpoint), version, username, password)
        {
        }

        public RemoteLRS(string endpoint, string username, string password) : this(endpoint, TCAPIVersion.latest(), username, password)
        {
        }

        enum RequestType
        {
            post,
            put,
        }

        public Uri endpoint { get; set; }

        public TCAPIVersion version { get; set; }

        public string auth { get; set; }

        public void SetAuth(string username, string password)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(username));
            Contract.Requires(!string.IsNullOrWhiteSpace(password));

            auth = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}";
        }

        /// <inheritdoc/>
        public async Task<AboutLRSResponse> About()
        {
            var request = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = "about",
            };

            var response = await MakeRequest(request).ConfigureAwait(false);

            return response.Status == HttpStatusCode.OK
                ? SuccessResult<AboutLRSResponse, About>(new About(StringFromBytes(response.Content)))
                : FailureResult<AboutLRSResponse>(response);
        }

        /// <inheritdoc/>
        public async Task<StatementLRSResponse> SaveStatement(Statement statement)
        {
            Contract.Requires(statement != null);

            var request = new LRSHttpRequest
            {
                QueryParams = new Dictionary<string, string>(),
                Resource = "statements",
                ContentType = "application/json",
                Content = Encoding.UTF8.GetBytes(statement.ToJSON(version)),
            };

            if (statement.id == null)
            {
                request.Method = HttpMethod.Post;
            }
            else
            {
                request.Method = HttpMethod.Put;
                request.QueryParams.Add("statementId", statement.id.ToString());
            }

            var response = await MakeRequest(request).ConfigureAwait(false);

            switch (statement.id)
            {
                case null when response.Status == HttpStatusCode.OK:
                    statement.id = new Guid((string)JArray.Parse(StringFromBytes(response.Content))[0]);
                    return SuccessResult<StatementLRSResponse, Statement>(statement);
                case null:
                    return FailureResult<StatementLRSResponse>(response);
                default:
                    return response.Status != HttpStatusCode.NoContent
                        ? FailureResult<StatementLRSResponse>(response)
                        : SuccessResult<StatementLRSResponse, Statement>(statement);
            }
        }

        public async Task<StatementLRSResponse> VoidStatement(Guid id, Agent agent)
        {
            Contract.Requires(agent != null);

            var voidStatement = new Statement
            {
                actor = agent,
                verb = Verb.Voided,
                target = new StatementRef { id = id },
            };

            return await SaveStatement(voidStatement).ConfigureAwait(false);
        }

        public async Task<StatementsResultLRSResponse> SaveStatements(List<Statement> statements)
        {
            Contract.Requires(statements != null);

            var request = new LRSHttpRequest
            {
                Resource = "statements",
                Method = HttpMethod.Post,
                ContentType = "application/json",
            };

            var jarray = new JArray();
            statements.ForEach(st => jarray.Add(st.ToJObject(version)));
            request.Content = Encoding.UTF8.GetBytes(jarray.ToString());

            var response = await MakeRequest(request).ConfigureAwait(false);
            StatementsResultLRSResponse result;

            if (response.Status == HttpStatusCode.OK)
            {
                var ids = JArray.Parse(StringFromBytes(response.Content));

                for (var i = 0; i < ids.Count; i++)
                {
                    statements[i].id = new Guid((string)ids[i]);
                }

                result = SuccessResult<StatementsResultLRSResponse, StatementsResult>(new StatementsResult(statements));
            }
            else
            {
                result = FailureResult<StatementsResultLRSResponse>(response);
            }

            return result;
        }

        public async Task<StatementLRSResponse> RetrieveStatement(Guid id)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "statementId", id.ToString() },
            };

            return await GetStatement(queryParams).ConfigureAwait(false);
        }

        public async Task<StatementLRSResponse> RetrieveVoidedStatement(Guid id)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "voidedStatementId", id.ToString() },
            };

            return await GetStatement(queryParams).ConfigureAwait(false);
        }

        public async Task<StatementsResultLRSResponse> QueryStatements(StatementsQuery query)
        {
            Contract.Requires(query != null);

            var request = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = "statements",
                QueryParams = query.ToParameterMap(version),
            };

            var response = await MakeRequest(request).ConfigureAwait(false);

            return response.Status == HttpStatusCode.OK
                ? SuccessResult<StatementsResultLRSResponse, StatementsResult>(new StatementsResult(new Json.StringOfJSON(StringFromBytes(response.Content))))
                : FailureResult<StatementsResultLRSResponse>(response);
        }

        public async Task<StatementsResultLRSResponse> MoreStatements(StatementsResult statementsResult)
        {
            Contract.Requires(statementsResult != null);

            var request = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = endpoint.Host,
            };

            if (!request.Resource.EndsWith("/", StringComparison.Ordinal) && !statementsResult.more.StartsWith("/", StringComparison.Ordinal))
            {
                  request.Resource += "/";
            }
            else if (request.Resource.EndsWith("/", StringComparison.Ordinal) && statementsResult.more.StartsWith("/", StringComparison.Ordinal))
            {
                statementsResult.more.Remove(0);
            }

            request.Resource += statementsResult.more;
            request.Resource = $"{endpoint.Scheme}://{request.Resource}";

            var response = await MakeRequest(request).ConfigureAwait(false);

            return response.Status == HttpStatusCode.OK
                ? SuccessResult<StatementsResultLRSResponse, StatementsResult>(new StatementsResult(new Json.StringOfJSON(StringFromBytes(response.Content))))
                : FailureResult<StatementsResultLRSResponse>(response);
        }

        public async Task<ProfileKeysLRSResponse> RetrieveStateIds(Activity activity, Agent agent, Guid? registration = null)
        {
            Contract.Requires(activity != null);
            Contract.Requires(agent != null);

            var queryParams = new Dictionary<string, string>
            {
                { "activityId", $"{activity.id}" },
                { "agent", agent.ToJSON(version) },
            };

            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
            }

            return await GetProfileKeys("activities/state", queryParams).ConfigureAwait(false);
        }

        public async Task<StateLRSResponse> RetrieveState(string id, Activity activity, Agent agent, Guid? registration = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(id));
            Contract.Requires(activity != null);
            Contract.Requires(agent != null);

            var queryParams = new Dictionary<string, string>
            {
                { "stateId", id },
                { "activityId", $"{activity.id}" },
                { "agent", agent.ToJSON(version) },
            };

            var state = new StateDocument
            {
                id = id,
                activity = activity,
                agent = agent,
            };

            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
                state.registration = registration;
            }

            var response = await GetDocument("activities/state", queryParams, state).ConfigureAwait(false);

            return response.Status == HttpStatusCode.OK || response.Status == HttpStatusCode.NotFound
                ? SuccessResult<StateLRSResponse, StateDocument>(state)
                : FailureResult<StateLRSResponse>(response);
        }

        public async Task<LRSResponse> SaveState(StateDocument state)
        {
            Contract.Requires(state != null);

            var queryParams = new Dictionary<string, string>
            {
                { "stateId", state.id },
                { "activityId", $"{state.activity.id}" },
                { "agent", state.agent.ToJSON(version) },
            };

            if (state.registration != null)
            {
                queryParams.Add("registration", state.registration.ToString());
            }

            return await SaveDocument("activities/state", queryParams, state).ConfigureAwait(false);
        }

        public async Task<LRSResponse> DeleteState(StateDocument state)
        {
            Contract.Requires(state != null);

            var queryParams = new Dictionary<string, string>
            {
                { "stateId", state.id },
                { "activityId", $"{state.activity.id}" },
                { "agent", state.agent.ToJSON(version) },
            };

            if (state.registration != null)
            {
                queryParams.Add("registration", state.registration.ToString());
            }

            return await DeleteDocument("activities/state", queryParams).ConfigureAwait(false);
        }

        public async Task<LRSResponse> ClearState(Activity activity, Agent agent, Guid? registration = null)
        {
            Contract.Requires(activity != null);
            Contract.Requires(agent != null);

            var queryParams = new Dictionary<string, string>
            {
                { "activityId", $"{activity.id}" },
                { "agent", agent.ToJSON(version) },
            };

            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
            }

            return await DeleteDocument("activities/state", queryParams).ConfigureAwait(false);
        }

        public async Task<ProfileKeysLRSResponse> RetrieveActivityProfileIds(Activity activity)
        {
            Contract.Requires(activity != null);

            var queryParams = new Dictionary<string, string>
            {
                { "activityId", $"{activity.id}" },
            };

            return await GetProfileKeys("activities/profile", queryParams).ConfigureAwait(false);
        }

        public async Task<ActivityProfileLRSResponse> RetrieveActivityProfile(string id, Activity activity)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(id));
            Contract.Requires(activity != null);

            var queryParams = new Dictionary<string, string>
            {
                { "profileId", id },
                { "activityId", $"{activity.id}" },
            };

            var profile = new ActivityProfileDocument
            {
                id = id,
                activity = activity,
            };

            var response = await GetDocument("activities/profile", queryParams, profile).ConfigureAwait(false);

            return response.Status == HttpStatusCode.OK || response.Status == HttpStatusCode.NotFound
                ? SuccessResult<ActivityProfileLRSResponse, ActivityProfileDocument>(profile)
                : FailureResult<ActivityProfileLRSResponse>(response);
        }

        public async Task<LRSResponse> SaveActivityProfile(ActivityProfileDocument profile)
        {
            Contract.Requires(profile != null);

            var queryParams = new Dictionary<string, string>
            {
                { "profileId", profile.id },
                { "activityId", $"{profile.activity.id}" },
            };

            return await SaveDocument("activities/profile", queryParams, profile).ConfigureAwait(false);
        }

        public async Task<LRSResponse> DeleteActivityProfile(ActivityProfileDocument profile)
        {
            Contract.Requires(profile != null);

            var queryParams = new Dictionary<string, string>
            {
                { "profileId", profile.id },
                { "activityId", $"{profile.activity.id}" },
            };

            return await DeleteDocument("activities/profile", queryParams).ConfigureAwait(false);
        }

        public async Task<ProfileKeysLRSResponse> RetrieveAgentProfileIds(Agent agent)
        {
            Contract.Requires(agent != null);

            var queryParams = new Dictionary<string, string>
            {
                { "agent", agent.ToJSON(version) },
            };

            return await GetProfileKeys("agents/profile", queryParams).ConfigureAwait(false);
        }

        public async Task<AgentProfileLRSResponse> RetrieveAgentProfile(string id, Agent agent)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(id));
            Contract.Requires(agent != null);

            var queryParams = new Dictionary<string, string>
            {
                { "profileId", id },
                { "agent", agent.ToJSON(version) },
            };

            var profile = new AgentProfileDocument
            {
                id = id,
                agent = agent,
            };

            var response = await GetDocument("agents/profile", queryParams, profile).ConfigureAwait(false);
            AgentProfileLRSResponse result;

            if (response.Status == HttpStatusCode.OK || response.Status == HttpStatusCode.NotFound)
            {
                var document = new AgentProfileDocument
                {
                    content = response.Content,
                    contentType = response.ContentType,
                    etag = response.Etag,
                };

                result = SuccessResult<AgentProfileLRSResponse, AgentProfileDocument>(document);
            }
            else
            {
                result = FailureResult<AgentProfileLRSResponse>(response);
            }

            return result;
        }

        public async Task<LRSResponse> SaveAgentProfile(AgentProfileDocument profile)
        {
            Contract.Requires(profile != null);

            return await SaveAgentProfile(profile, RequestType.put).ConfigureAwait(false);
        }

        public async Task<LRSResponse> ForceSaveAgentProfile(AgentProfileDocument profile)
        {
            Contract.Requires(profile != null);

            return await SaveAgentProfile(profile, RequestType.post).ConfigureAwait(false);
        }

        public async Task<LRSResponse> DeleteAgentProfile(AgentProfileDocument profile)
        {
            Contract.Requires(profile != null);

            var queryParams = new Dictionary<string, string>
            {
                { "profileId", profile.id },
                { "agent", profile.agent.ToJSON(version) },
            };

            return await DeleteDocument("agents/profile", queryParams).ConfigureAwait(false);
        }

        static TResponse SuccessResult<TResponse, TContent>(TContent content) where TResponse : ILRSContentResponse<TContent>, new()
        {
            return new TResponse
            {
                success = true,
                content = content,
            };
        }

        static TResponse FailureResult<TResponse>(LRSHttpResponse response) where TResponse : ILRSResponse, new()
        {
            var result = new TResponse
            {
                success = false,
                httpException = response.Exception,
            };

            if (response.Status is HttpStatusCode status)
            {
                result.SetErrMsgFromBytes(response.Content, (int)status);
            }
            else
            {
                result.SetErrMsgFromBytes(response.Content);
            }

            return result;
        }

        static string StringFromBytes(byte[] bytes)
        {
            Contract.Requires(bytes != null);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        async Task<LRSHttpResponse> MakeRequest(LRSHttpRequest req)
        {
            Contract.Requires(req != null);

            string url;
            if (req.Resource.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                url = req.Resource;
            }
            else
            {
                url = endpoint.ToString();

                if (!url.EndsWith("/", StringComparison.Ordinal) && !req.Resource.StartsWith("/", StringComparison.Ordinal))
                {
                    url += "/";
                }

                url += req.Resource;
            }

            if (req.QueryParams != null)
            {
                var qs = string.Empty;

                foreach (var entry in req.QueryParams)
                {
                    if (!string.IsNullOrEmpty(qs))
                    {
                        qs += "&";
                    }

                    qs += WebUtility.UrlEncode(entry.Key) + "=" + WebUtility.UrlEncode(entry.Value);
                }

                if (!string.IsNullOrEmpty(qs))
                {
                    url += "?" + qs;
                }
            }

            var webReq = new HttpRequestMessage(req.Method, new Uri(url));

            // We only have one client. We cannot modify it while its in use.
            await makeRequestSemaphore.WaitAsync().ConfigureAwait(false);
            webReq.Headers.Add("X-Experience-API-Version", version.ToString());
            webReq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(req.ContentType ?? "application/content-stream"));

            if (auth != null)
            {
                webReq.Headers.Add("Authorization", auth);
            }

            if (req.Headers != null)
            {
                foreach (var entry in req.Headers)
                {
                    if (webReq.Headers.Contains(entry.Key))
                    {
                        makeRequestSemaphore.Release();
                        throw new InvalidOperationException($"Tried to add duplicate entry {entry.Key} to request headers with value {entry.Value}; previous value {client.DefaultRequestHeaders.GetValues(entry.Key)}");
                    }

                    webReq.Headers.Add(entry.Key, entry.Value);
                }
            }

            if (req.Content != null)
            {
                webReq.Content = new ByteArrayContent(req.Content);
                webReq.Content.Headers.Add("Content-Length", req.Content.Length.ToString(CultureInfo.InvariantCulture));
                webReq.Content.Headers.Add("Content-Type", req.ContentType ?? "text/plain");
            }

            LRSHttpResponse resp;

            try
            {
                var response = await client.SendAsync(webReq).ConfigureAwait(false);
                resp = new LRSHttpResponse(response);
            }
            catch (WebException ex)
            {
                resp = new LRSHttpResponse(ex);
            }
            finally
            {
                makeRequestSemaphore.Release();
            }

            return resp;
        }

        async Task<LRSHttpResponse> GetDocument(string resource, Dictionary<string, string> queryParams, Document document)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(resource));
            Contract.Requires(document != null);

            var request = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = resource,
                QueryParams = queryParams,
            };

            var response = await MakeRequest(request).ConfigureAwait(false);

            if (response.Status == HttpStatusCode.OK)
            {
                document.content = response.Content;
                document.contentType = response.ContentType;

                if (response.LastModified is DateTime last)
                {
                    document.timestamp = last;
                }

                document.etag = response.Etag;
            }

            return response;
        }

        async Task<ProfileKeysLRSResponse> GetProfileKeys(string resource, Dictionary<string, string> queryParams)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(resource));

            var request = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = resource,
                QueryParams = queryParams,
            };

            var response = await MakeRequest(request).ConfigureAwait(false);
            ProfileKeysLRSResponse result;

            if (response.Status == HttpStatusCode.OK)
            {
                result = new ProfileKeysLRSResponse
                {
                    success = true,
                };

                var keys = JArray.Parse(StringFromBytes(response.Content));

                if (keys.Count > 0)
                {
                    result.content = keys.Select(key => (string)key).ToList();
                }
            }
            else
            {
                result = FailureResult<ProfileKeysLRSResponse>(response);
            }

            return result;
        }

        async Task<LRSResponse> SaveDocument(string resource, Dictionary<string, string> queryParams, Document document, RequestType requestType = RequestType.put)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(resource));
            Contract.Requires(document != null);

            if (document.contentType == null)
            {
                document.contentType = "application/json";
            }

            var contentType = MediaTypeHeaderValue.Parse(document.contentType);

            var request = new LRSHttpRequest
            {
                Method = requestType == RequestType.post ? HttpMethod.Post : HttpMethod.Put,
                Resource = resource,
                QueryParams = queryParams,
                ContentType = contentType.MediaType,
                Content = document.content,
            };

            if (document.etag != null)
            {
                request.Headers = new Dictionary<string, string>
                {
                    { "If-Match", document.etag },
                };
            }

            var response = await MakeRequest(request).ConfigureAwait(false);
            return response.Status == HttpStatusCode.NoContent
                ? new LRSResponse(true)
                : FailureResult<LRSResponse>(response);
        }

        async Task<LRSResponse> DeleteDocument(string resource, Dictionary<string, string> queryParams)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(resource));

            var request = new LRSHttpRequest
            {
                Method = HttpMethod.Delete,
                Resource = resource,
                QueryParams = queryParams,
            };

            var response = await MakeRequest(request).ConfigureAwait(false);

            return response.Status == HttpStatusCode.NoContent
                ? new LRSResponse(true)
                : FailureResult<LRSResponse>(response);
        }

        async Task<StatementLRSResponse> GetStatement(Dictionary<string, string> queryParams)
        {
            var request = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = "statements",
                QueryParams = queryParams,
            };

            var response = await MakeRequest(request).ConfigureAwait(false);

            return response.Status == HttpStatusCode.OK
                ? SuccessResult<StatementLRSResponse, Statement>(new Statement(new Json.StringOfJSON(StringFromBytes(response.Content))))
                : FailureResult<StatementLRSResponse>(response);
        }

        async Task<LRSResponse> SaveAgentProfile(AgentProfileDocument profile, RequestType requestType)
        {
            Contract.Requires(profile != null);

            var queryParams = new Dictionary<string, string>
            {
                { "profileId", profile.id },
                { "agent", profile.agent.ToJSON(version) },
            };

            return await SaveDocument("agents/profile", queryParams, profile, requestType).ConfigureAwait(false);
        }
    }
}
