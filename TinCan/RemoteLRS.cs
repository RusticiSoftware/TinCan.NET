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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TinCan.Documents;
using TinCan.LRSResponses;

namespace TinCan
{
    public class RemoteLRS : ILRS
    {
        readonly HttpClient client = new HttpClient();
        public Uri endpoint { get; set; }
        public TCAPIVersion version { get; set; }
        public String auth { get; set; }
        public Dictionary<String, String> extended { get; set; }

        public void SetAuth(String username, String password)
        {
            auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
        }

        public RemoteLRS() { }
        public RemoteLRS(Uri endpoint, TCAPIVersion version, String username, String password)
        {
            this.endpoint = endpoint;
            this.version = version;
            this.SetAuth(username, password);
        }
        public RemoteLRS(String endpoint, TCAPIVersion version, String username, String password) : this(new Uri(endpoint), version, username, password) { }
        public RemoteLRS(String endpoint, String username, String password) : this(endpoint, TCAPIVersion.latest(), username, password) { }

        async Task<LRSHttpResponse> MakeRequest(LRSHttpRequest req)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            string url;

            if (req.Resource.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
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
                    if (qs != string.Empty)
                    {
                        qs += "&";
                    }

                    qs += string.Format("{0}={1}", WebUtility.UrlEncode(entry.Key), WebUtility.UrlEncode(entry.Value));
                }

                if (qs != string.Empty)
                {
                    url += "?" + qs;
                }
            }

            // TODO: handle special properties we recognize, such as content type, modified since, etc.
            var webReq = new HttpRequestMessage(req.Method, new Uri(url));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-Experience-API-Version", version.ToString());
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(req.ContentType ?? "application/content-stream"));

            if (auth != null)
            {
                client.DefaultRequestHeaders.Add("Authorization", auth);
            }
            if (req.Headers != null)
            {
                foreach (var entry in req.Headers)
                {
                    client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                }
            }

            if (req.Content != null)
            {
                webReq.Content = new ByteArrayContent(req.Content);
                webReq.Content.Headers.Add("Content-Length", req.Content.Length.ToString());

                if (!string.IsNullOrWhiteSpace(req.ContentType))
                {
                    webReq.Content.Headers.Add("Content-Type", req.ContentType);
                }
                else
                {
                    webReq.Content.Headers.Add("Content-Type", "application/json");
                }
            }

            LRSHttpResponse resp;

            try
            {
                var theResponse = await client.SendAsync(webReq).ConfigureAwait(false);
                resp = new LRSHttpResponse(theResponse);
            }
            catch (WebException ex)
            {
                resp = new LRSHttpResponse();

                if (ex.Response != null)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    {
                        resp.Content = ReadFully(stream, (int)ex.Response.ContentLength);
                    }
                }
                else
                {
                    resp.Content = Encoding.UTF8.GetBytes("Web exception without '.Response'");
                }
                resp.Exception = ex;
            }

            return resp;
        }

        /// <summary>
        /// See http://www.yoda.arachsys.com/csharp/readbinary.html no license found
        /// 
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        private static byte[] ReadFully(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }

        private async Task<LRSHttpResponse> GetDocument(String resource, Dictionary<String, String> queryParams, Document document)
        {
            var req = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = resource,
                QueryParams = queryParams
            };

            var res = await MakeRequest(req);

            if (res.Status == HttpStatusCode.OK)
            {
                document.content = res.Content;
                document.contentType = res.ContentType;
                document.timestamp = res.LastModified;
                document.etag = res.Etag;
            }

            return res;
        }

        private async Task<ProfileKeysLRSResponse> GetProfileKeys(String resource, Dictionary<String, String> queryParams)
        {
            var r = new ProfileKeysLRSResponse();

            var req = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = resource,
                QueryParams = queryParams
            };

            var res = await MakeRequest(req);
            if (res.Status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.Exception;
                r.SetErrMsgFromBytes(res.Content);
                return r;
            }

            r.success = true;

            var keys = JArray.Parse(Encoding.UTF8.GetString(res.Content));
            if (keys.Count > 0)
            {
                r.content = new List<String>();
                foreach (JToken key in keys)
                {
                    r.content.Add((String)key);
                }
            }

            return r;
        }

        private async Task<LRSResponse> SaveDocument(String resource, Dictionary<String, String> queryParams, Document document)
        {
            var r = new LRSResponse();

            var req = new LRSHttpRequest
            {
                Method = HttpMethod.Put,
                Resource = resource,
                QueryParams = queryParams,
                ContentType = document.contentType,
                Content = document.content
            };

            var res = await MakeRequest(req);
            if (res.Status != HttpStatusCode.NoContent)
            {
                r.success = false;
                r.httpException = res.Exception;
                r.SetErrMsgFromBytes(res.Content);
                return r;
            }

            r.success = true;

            return r;
        }

        private async Task<LRSResponse> DeleteDocument(String resource, Dictionary<String, String> queryParams)
        {
            var r = new LRSResponse();

            var req = new LRSHttpRequest
            {
                Method = HttpMethod.Delete,
                Resource = resource,
                QueryParams = queryParams
            };

            var res = await MakeRequest(req);
            if (res.Status != HttpStatusCode.NoContent)
            {
                r.success = false;
                r.httpException = res.Exception;
                r.SetErrMsgFromBytes(res.Content);
                return r;
            }

            r.success = true;

            return r;
        }

        private async Task<StatementLRSResponse> GetStatement(Dictionary<String, String> queryParams)
        {
            var r = new StatementLRSResponse();

            var req = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = "statements",
                QueryParams = queryParams
            };

            var res = await MakeRequest(req);
            if (res.Status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.Exception;
                r.SetErrMsgFromBytes(res.Content);
                return r;
            }

            r.success = true;
            r.content = new Statement(new Json.StringOfJSON(Encoding.UTF8.GetString(res.Content)));

            return r;
        }

        public async Task<AboutLRSResponse> About()
        {
            var r = new AboutLRSResponse();

            var req = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = "about"
            };

            var res = await MakeRequest(req);
            if (res.Status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.Exception;
                r.SetErrMsgFromBytes(res.Content);
                return r;
            }

            r.success = true;
            r.content = new About(Encoding.UTF8.GetString(res.Content));

            return r;
        }

        public async Task<StatementLRSResponse> SaveStatement(Statement statement)
        {
            var r = new StatementLRSResponse();
            var req = new LRSHttpRequest
            {
                QueryParams = new Dictionary<String, String>(),
                Resource = "statements"
            };

            if (statement.id == null)
            {
                req.Method = HttpMethod.Post;
            }
            else
            {
                req.Method = HttpMethod.Put;
                req.QueryParams.Add("statementId", statement.id.ToString());
            }

            req.ContentType = "application/json";
            req.Content = Encoding.UTF8.GetBytes(statement.ToJSON(version));

            var res = await MakeRequest(req);
            if (statement.id == null)
            {
                if (res.Status != HttpStatusCode.OK)
                {
                    r.success = false;
                    r.httpException = res.Exception;
                    r.SetErrMsgFromBytes(res.Content);
                    return r;
                }

                var ids = JArray.Parse(Encoding.UTF8.GetString(res.Content));
                statement.id = new Guid((String)ids[0]);
            }
            else
            {
                if (res.Status != HttpStatusCode.NoContent)
                {
                    r.success = false;
                    r.httpException = res.Exception;
                    r.SetErrMsgFromBytes(res.Content);
                    return r;
                }
            }

            r.success = true;
            r.content = statement;

            return r;
        }
        public async Task<StatementLRSResponse> VoidStatement(Guid id, Agent agent)
        {
            var voidStatement = new Statement
            {
                actor = agent,
                verb = new Verb
                {
                    id = new Uri("http://adlnet.gov/expapi/verbs/voided"),
                    display = new LanguageMap()
                },
                target = new StatementRef { id = id }
            };
            voidStatement.verb.display.Add("en-US", "voided");

            return await SaveStatement(voidStatement);
        }
        public async Task<StatementsResultLRSResponse> SaveStatements(List<Statement> statements)
        {
            var r = new StatementsResultLRSResponse();

            var req = new LRSHttpRequest
            {
                Resource = "statements",
                Method = HttpMethod.Post,
                ContentType = "application/json"
            };

            var jarray = new JArray();
            foreach (Statement st in statements)
            {
                jarray.Add(st.ToJObject(version));
            }
            req.Content = Encoding.UTF8.GetBytes(jarray.ToString());

            var res = await MakeRequest(req);
            if (res.Status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.Exception;
                r.SetErrMsgFromBytes(res.Content);
                return r;
            }

            var ids = JArray.Parse(Encoding.UTF8.GetString(res.Content));
            for (int i = 0; i < ids.Count; i++)
            {
                statements[i].id = new Guid((String)ids[i]);
            }

            r.success = true;
            r.content = new StatementsResult(statements);

            return r;
        }
        public async Task<StatementLRSResponse> RetrieveStatement(Guid id)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("statementId", id.ToString());

            return await GetStatement(queryParams);
        }
        public async Task<StatementLRSResponse> RetrieveVoidedStatement(Guid id)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("voidedStatementId", id.ToString());

            return await GetStatement(queryParams);
        }
        public async Task<StatementsResultLRSResponse> QueryStatements(StatementsQuery query)
        {
            var r = new StatementsResultLRSResponse();

            var req = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = "statements",
                QueryParams = query.ToParameterMap(version)
            };

            var res = await MakeRequest(req);
            if (res.Status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.Exception;
                r.SetErrMsgFromBytes(res.Content);
                return r;
            }

            r.success = true;
            r.content = new StatementsResult(new Json.StringOfJSON(Encoding.UTF8.GetString(res.Content)));

            return r;
        }
        public async Task<StatementsResultLRSResponse> MoreStatements(StatementsResult result)
        {
            var r = new StatementsResultLRSResponse();

            var req = new LRSHttpRequest
            {
                Method = HttpMethod.Get,
                Resource = endpoint.GetLeftPart(UriPartial.Authority)
            };
            if (!req.Resource.EndsWith("/", StringComparison.Ordinal))
            {
                req.Resource += "/";
            }
            req.Resource += result.more;

            var res = await MakeRequest(req);
            if (res.Status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.Exception;
                r.SetErrMsgFromBytes(res.Content);
                return r;
            }

            r.success = true;
            r.content = new StatementsResult(new Json.StringOfJSON(Encoding.UTF8.GetString(res.Content)));

            return r;
        }

        // TODO: since param
        public async Task<ProfileKeysLRSResponse> RetrieveStateIds(Activity activity, Agent agent, Nullable<Guid> registration = null)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());
            queryParams.Add("agent", agent.ToJSON(version));
            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
            }

            return await GetProfileKeys("activities/state", queryParams);
        }
        public async Task<StateLRSResponse> RetrieveState(String id, Activity activity, Agent agent, Nullable<Guid> registration = null)
        {
            var r = new StateLRSResponse();

            var queryParams = new Dictionary<String, String>();
            queryParams.Add("stateId", id);
            queryParams.Add("activityId", activity.id.ToString());
            queryParams.Add("agent", agent.ToJSON(version));

            var state = new StateDocument();
            state.id = id;
            state.activity = activity;
            state.agent = agent;

            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
                state.registration = registration;
            }

            var resp = await GetDocument("activities/state", queryParams, state);
            if (resp.Status != HttpStatusCode.OK && resp.Status != HttpStatusCode.NotFound)
            {
                r.success = false;
                r.httpException = resp.Exception;
                r.SetErrMsgFromBytes(resp.Content);
                return r;
            }
            r.success = true;
            r.content = state;

            return r;
        }
        public async Task<LRSResponse> SaveState(StateDocument state)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("stateId", state.id);
            queryParams.Add("activityId", state.activity.id.ToString());
            queryParams.Add("agent", state.agent.ToJSON(version));
            if (state.registration != null)
            {
                queryParams.Add("registration", state.registration.ToString());
            }

            return await SaveDocument("activities/state", queryParams, state);
        }
        public async Task<LRSResponse> DeleteState(StateDocument state)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("stateId", state.id);
            queryParams.Add("activityId", state.activity.id.ToString());
            queryParams.Add("agent", state.agent.ToJSON(version));
            if (state.registration != null)
            {
                queryParams.Add("registration", state.registration.ToString());
            }

            return await DeleteDocument("activities/state", queryParams);
        }
        public async Task<LRSResponse> ClearState(Activity activity, Agent agent, Nullable<Guid> registration = null)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());
            queryParams.Add("agent", agent.ToJSON(version));
            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
            }

            return await DeleteDocument("activities/state", queryParams);
        }

        // TODO: since param
        public async Task<ProfileKeysLRSResponse> RetrieveActivityProfileIds(Activity activity)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());

            return await GetProfileKeys("activities/profile", queryParams);
        }
        public async Task<ActivityProfileLRSResponse> RetrieveActivityProfile(String id, Activity activity)
        {
            var r = new ActivityProfileLRSResponse();

            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", id);
            queryParams.Add("activityId", activity.id.ToString());

            var profile = new ActivityProfileDocument();
            profile.id = id;
            profile.activity = activity;

            var resp = await GetDocument("activities/profile", queryParams, profile);
            if (resp.Status != HttpStatusCode.OK && resp.Status != HttpStatusCode.NotFound)
            {
                r.success = false;
                r.httpException = resp.Exception;
                r.SetErrMsgFromBytes(resp.Content);
                return r;
            }
            r.success = true;
            r.content = profile;

            return r;
        }
        public async Task<LRSResponse> SaveActivityProfile(ActivityProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("activityId", profile.activity.id.ToString());

            return await SaveDocument("activities/profile", queryParams, profile);
        }
        public async Task<LRSResponse> DeleteActivityProfile(ActivityProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("activityId", profile.activity.id.ToString());
            // TODO: need to pass Etag?

            return await DeleteDocument("activities/profile", queryParams);
        }

        // TODO: since param
        public async Task<ProfileKeysLRSResponse> RetrieveAgentProfileIds(Agent agent)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("agent", agent.ToJSON(version));

            return await GetProfileKeys("agents/profile", queryParams);
        }
        public async Task<AgentProfileLRSResponse> RetrieveAgentProfile(String id, Agent agent)
        {
            var r = new AgentProfileLRSResponse();

            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", id);
            queryParams.Add("agent", agent.ToJSON(version));

            var profile = new AgentProfileDocument();
            profile.id = id;
            profile.agent = agent;

            var resp = await GetDocument("agents/profile", queryParams, profile);
            if (resp.Status != HttpStatusCode.OK && resp.Status != HttpStatusCode.NotFound)
            {
                r.success = false;
                r.httpException = resp.Exception;
                r.SetErrMsgFromBytes(resp.Content);
                return r;
            }
            r.success = true;
            r.content = profile;

            return r;
        }
        public async Task<LRSResponse> SaveAgentProfile(AgentProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("agent", profile.agent.ToJSON(version));

            return await SaveDocument("agents/profile", queryParams, profile);
        }
        public async Task<LRSResponse> DeleteAgentProfile(AgentProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("agent", profile.agent.ToJSON(version));
            // TODO: need to pass Etag?

            return await DeleteDocument("agents/profile", queryParams);
        }
    }
}
