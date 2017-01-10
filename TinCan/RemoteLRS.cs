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
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using TinCan.Documents;
using TinCan.LRSResponses;

namespace TinCan
{
    public class RemoteLRS : ILRS
    {
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

        private class MyHTTPRequest
        {
            public String method { get; set; }
            public String resource { get; set; }
            public Dictionary<String, String> queryParams { get; set; }
            public Dictionary<String, String> headers { get; set; }
            public String contentType { get; set; }
            public byte[] content { get; set; }
        }

        private class MyHTTPResponse
        {
            public HttpStatusCode status { get; set; }
            public String contentType { get; set; }
            public byte[] content { get; set; }
            public DateTime lastModified { get; set; }
            public String etag { get; set; }
            public Exception ex { get; set; }

            public MyHTTPResponse() { }
            public MyHTTPResponse(HttpWebResponse webResp)
            {
                status = webResp.StatusCode;
                contentType = webResp.ContentType;
                etag = webResp.Headers.Get("Etag");
                lastModified = webResp.LastModified;

                using (var stream = webResp.GetResponseStream())
                {
                    content = ReadFully(stream, (int)webResp.ContentLength);
                }
            }
        }

        private HttpWebRequest PreprocessRequest(MyHTTPRequest req)
        {
            String url;
            if (req.resource.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                url = req.resource;
            }
            else
            {
                url = endpoint.ToString();
                if (!url.EndsWith("/") && !req.resource.StartsWith("/"))
                {
                    url += "/";
                }
                url += req.resource;
            }

            if (req.queryParams != null)
            {
                String qs = "";
                foreach (KeyValuePair<String, String> entry in req.queryParams)
                {
                    if (qs != "")
                    {
                        qs += "&";
                    }
                    qs += HttpUtility.UrlEncode(entry.Key) + "=" + HttpUtility.UrlEncode(entry.Value);
                }
                if (qs != "")
                {
                    url += "?" + qs;
                }
            }

            // TODO: handle special properties we recognize, such as content type, modified since, etc.
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.Method = req.method;

            webReq.Headers.Add("X-Experience-API-Version", version.ToString());
            if (auth != null)
            {
                webReq.Headers.Add("Authorization", auth);
            }
            if (req.headers != null)
            {
                foreach (KeyValuePair<String, String> entry in req.headers)
                {
                    webReq.Headers.Add(entry.Key, entry.Value);
                }
            }

            if (req.contentType != null)
            {
                webReq.ContentType = req.contentType;
            }
            else
            {
                webReq.ContentType = "application/octet-stream";
            }

            if (req.content != null)
            {
                webReq.ContentLength = req.content.Length;
                using (var stream = webReq.GetRequestStream())
                {
                    stream.Write(req.content, 0, req.content.Length);
                }
            }

            return webReq;
        }

        private MyHTTPResponse MakeSyncRequest(MyHTTPRequest req)
        {
            HttpWebRequest webReq = PreprocessRequest(req);
            MyHTTPResponse resp;

            try
            {
                using (var webResp = (HttpWebResponse)webReq.GetResponse())
                {
                    resp = new MyHTTPResponse(webResp);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var webResp = (HttpWebResponse)ex.Response)
                    {
                        resp = new MyHTTPResponse(webResp);
                    }
                }
                else
                {
                    resp = new MyHTTPResponse();
                    resp.content = Encoding.UTF8.GetBytes("Web exception without '.Response'");
                }
                resp.ex = ex;
            }

            return resp;
        }

        private async Task<MyHTTPResponse> MakeAsyncRequest(MyHTTPRequest req)
        {
            HttpWebRequest webReq = PreprocessRequest(req);
            MyHTTPResponse resp;

            try
            {
                using (var webResp = await webReq.GetResponseAsync())
                {
                    resp = new MyHTTPResponse((HttpWebResponse)webResp);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var webResp = (HttpWebResponse)ex.Response)
                    {
                        resp = new MyHTTPResponse(webResp);
                    }
                }
                else
                {
                    resp = new MyHTTPResponse();
                    resp.content = Encoding.UTF8.GetBytes("Web exception without '.Response'");
                }
                resp.ex = ex;
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

        private MyHTTPResponse GetDocument(String resource, Dictionary<String, String> queryParams, Document document)
        {
            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = resource;
            req.queryParams = queryParams;

            var res = MakeSyncRequest(req);
            if (res.status == HttpStatusCode.OK)
            {
                document.content = res.content;
                document.contentType = res.contentType;
                document.timestamp = res.lastModified;
                document.etag = res.etag;
            }

            return res;
        }
        private ProfileKeysLRSResponse GetProfileKeys(String resource, Dictionary<String, String> queryParams)
        {
            var r = new ProfileKeysLRSResponse();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = resource;
            req.queryParams = queryParams;

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;

            var keys = JArray.Parse(Encoding.UTF8.GetString(res.content));
            if (keys.Count > 0) {
                r.content = new List<String>();
                foreach (JToken key in keys) {
                    r.content.Add((String)key);
                }
            }

            return r;
        }
        private LRSResponse SaveDocument(String resource, Dictionary<String, String> queryParams, Document document)
        {
            var r = new LRSResponse();

            var req = new MyHTTPRequest();
            req.method = "PUT";
            req.resource = resource;
            req.queryParams = queryParams;
            req.contentType = document.contentType;
            req.content = document.content;

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.NoContent)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;

            return r;
        }
        private LRSResponse DeleteDocument(String resource, Dictionary<String, String> queryParams)
        {
            var r = new LRSResponse();

            var req = new MyHTTPRequest();
            req.method = "DELETE";
            req.resource = resource;
            req.queryParams = queryParams;

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.NoContent)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;

            return r;
        }
        private StatementLRSResponse GetStatement(Dictionary<String, String> queryParams)
        {
            var r = new StatementLRSResponse();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = "statements";
            req.queryParams = queryParams;

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;
            r.content = new Statement(new Json.StringOfJSON(Encoding.UTF8.GetString(res.content)));

            return r;
        }

        private async Task<MyHTTPResponse> GetDocumentAsync(String resource, Dictionary<String, String> queryParams, Document document)
        {
            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = resource;
            req.queryParams = queryParams;

            var res = await MakeAsyncRequest(req);
            if (res.status == HttpStatusCode.OK)
            {
                document.content = res.content;
                document.contentType = res.contentType;
                document.timestamp = res.lastModified;
                document.etag = res.etag;
            }

            return res;
        }
        private async Task<ProfileKeysLRSResponse> GetProfileKeysAsync(String resource, Dictionary<String, String> queryParams)
        {
            var r = new ProfileKeysLRSResponse();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = resource;
            req.queryParams = queryParams;

            var res = await MakeAsyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;

            var keys = JArray.Parse(Encoding.UTF8.GetString(res.content));
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
        private async Task<LRSResponse> SaveDocumentAsync(String resource, Dictionary<String, String> queryParams, Document document)
        {
            var r = new LRSResponse();

            var req = new MyHTTPRequest();
            req.method = "PUT";
            req.resource = resource;
            req.queryParams = queryParams;
            req.contentType = document.contentType;
            req.content = document.content;

            var res = await MakeAsyncRequest(req);
            if (res.status != HttpStatusCode.NoContent)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;

            return r;
        }
        private async Task<LRSResponse> DeleteDocumentAsync(String resource, Dictionary<String, String> queryParams)
        {
            var r = new LRSResponse();

            var req = new MyHTTPRequest();
            req.method = "DELETE";
            req.resource = resource;
            req.queryParams = queryParams;

            var res = await MakeAsyncRequest(req);
            if (res.status != HttpStatusCode.NoContent)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;

            return r;
        }
        private async Task<StatementLRSResponse> GetStatementAsync(Dictionary<String, String> queryParams)
        {
            var r = new StatementLRSResponse();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = "statements";
            req.queryParams = queryParams;

            MyHTTPResponse res = await MakeAsyncRequest(req);

            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;
            r.content = new Statement(new Json.StringOfJSON(Encoding.UTF8.GetString(res.content)));

            return r;
        }

        public AboutLRSResponse About()
        {
            var r = new AboutLRSResponse();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = "about";

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;
            r.content = new About(Encoding.UTF8.GetString(res.content));

            return r;
        }
        public StatementLRSResponse SaveStatement(Statement statement)
        {
            var r = new StatementLRSResponse();
            var req = new MyHTTPRequest();
            req.queryParams = new Dictionary<String, String>();
            req.resource = "statements";

            if (statement.id == null)
            {
                req.method = "POST";
            }
            else
            {
                req.method = "PUT";
                req.queryParams.Add("statementId", statement.id.ToString());
            }

            req.contentType = "application/json";
            req.content = Encoding.UTF8.GetBytes(statement.ToJSON(version));

            var res = MakeSyncRequest(req);
            if (statement.id == null)
            {
                if (res.status != HttpStatusCode.OK)
                {
                    r.success = false;
                    r.httpException = res.ex;
                    r.SetErrMsgFromBytes(res.content);
                    return r;
                }

                var ids = JArray.Parse(Encoding.UTF8.GetString(res.content));
                statement.id = new Guid((String)ids[0]);
            }
            else {
                if (res.status != HttpStatusCode.NoContent)
                {
                    r.success = false;
                    r.httpException = res.ex;
                    r.SetErrMsgFromBytes(res.content);
                    return r;
                }
            }

            r.success = true;
            r.content = statement;

            return r;
        }
        public StatementLRSResponse VoidStatement(Guid id, Agent agent)
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

            return SaveStatement(voidStatement);
        }
        public StatementsResultLRSResponse SaveStatements(List<Statement> statements)
        {
            var r = new StatementsResultLRSResponse();

            var req = new MyHTTPRequest();
            req.resource = "statements";
            req.method = "POST";
            req.contentType = "application/json";

            var jarray = new JArray();
            foreach (Statement st in statements)
            {
                jarray.Add(st.ToJObject(version));
            }
            req.content = Encoding.UTF8.GetBytes(jarray.ToString());

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            var ids = JArray.Parse(Encoding.UTF8.GetString(res.content));
            for (int i = 0; i < ids.Count; i++)
            {
                statements[i].id = new Guid((String)ids[i]);
            }

            r.success = true;
            r.content = new StatementsResult(statements);

            return r;
        }
        public StatementLRSResponse RetrieveStatement(Guid id)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("statementId", id.ToString());

            return GetStatement(queryParams);
        }
        public StatementLRSResponse RetrieveVoidedStatement(Guid id)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("voidedStatementId", id.ToString());

            return GetStatement(queryParams);
        }
        public StatementsResultLRSResponse QueryStatements(StatementsQuery query)
        {
            var r = new StatementsResultLRSResponse();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = "statements";
            req.queryParams = query.ToParameterMap(version);

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;
            r.content = new StatementsResult(new Json.StringOfJSON(Encoding.UTF8.GetString(res.content)));

            return r;
        }
        public StatementsResultLRSResponse MoreStatements(StatementsResult result)
        {
            var r = new StatementsResultLRSResponse();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = endpoint.GetLeftPart(UriPartial.Authority);
            if (! req.resource.EndsWith("/")) {
                req.resource += "/";
            }
            req.resource += result.more;

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;
            r.content = new StatementsResult(new Json.StringOfJSON(Encoding.UTF8.GetString(res.content)));

            return r;
        }
        
        public async Task<AboutLRSResponse> AboutAsync()
        {
            var r = new AboutLRSResponse();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = "about";

            var res = await MakeAsyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;
            r.content = new About(Encoding.UTF8.GetString(res.content));

            return r;
        }
        public async Task<StatementLRSResponse> SaveStatementAsync(Statement statement)
        {
            var r = new StatementLRSResponse();
            var req = new MyHTTPRequest();
            req.queryParams = new Dictionary<String, String>();
            req.resource = "statements";

            if (statement.id == null)
            {
                req.method = "POST";
            }
            else
            {
                req.method = "PUT";
                req.queryParams.Add("statementId", statement.id.ToString());
            }

            req.contentType = "application/json";
            req.content = Encoding.UTF8.GetBytes(statement.ToJSON(version));

            MyHTTPResponse res = await MakeAsyncRequest(req);

            if (statement.id == null)
            {
                if (res.status != HttpStatusCode.OK)
                {
                    r.success = false;
                    r.httpException = res.ex;
                    r.SetErrMsgFromBytes(res.content);
                    return r;
                }

                var ids = JArray.Parse(Encoding.UTF8.GetString(res.content));
                statement.id = new Guid((String)ids[0]);
            }
            else
            {
                if (res.status != HttpStatusCode.NoContent)
                {
                    r.success = false;
                    r.httpException = res.ex;
                    r.SetErrMsgFromBytes(res.content);
                    return r;
                }
            }

            r.success = true;
            r.content = statement;

            return r;
        }
        public async Task<StatementLRSResponse> VoidStatementAsync(Guid id, Agent agent)
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

            return await SaveStatementAsync(voidStatement);
        }
        public async Task<StatementsResultLRSResponse> SaveStatementsAsync(List<Statement> statements)
        {
            var r = new StatementsResultLRSResponse();

            var req = new MyHTTPRequest();
            req.resource = "statements";
            req.method = "POST";
            req.contentType = "application/json";

            var jarray = new JArray();
            foreach (Statement st in statements)
            {
                jarray.Add(st.ToJObject(version));
            }
            req.content = Encoding.UTF8.GetBytes(jarray.ToString());

            MyHTTPResponse res = await MakeAsyncRequest(req);

            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            var ids = JArray.Parse(Encoding.UTF8.GetString(res.content));
            for (int i = 0; i < ids.Count; i++)
            {
                statements[i].id = new Guid((String)ids[i]);
            }

            r.success = true;
            r.content = new StatementsResult(statements);

            return r;
        }
        public async Task<StatementLRSResponse> RetrieveStatementAsync(Guid id)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("statementId", id.ToString());

            return await GetStatementAsync(queryParams);
        }
        public async Task<StatementLRSResponse> RetrieveVoidedStatementAsync(Guid id)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("voidedStatementId", id.ToString());

            return await GetStatementAsync(queryParams);
        }
        public async Task<StatementsResultLRSResponse> QueryStatementsAsync(StatementsQuery query)
        {
            var r = new StatementsResultLRSResponse();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = "statements";
            req.queryParams = query.ToParameterMap(version);

            MyHTTPResponse res = await MakeAsyncRequest(req);

            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;
            r.content = new StatementsResult(new Json.StringOfJSON(Encoding.UTF8.GetString(res.content)));

            return r;
        }
        public async Task<StatementsResultLRSResponse> MoreStatementsAsync(StatementsResult result)
        {
            var r = new StatementsResultLRSResponse();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = endpoint.GetLeftPart(UriPartial.Authority);
            if (!req.resource.EndsWith("/"))
            {
                req.resource += "/";
            }
            req.resource += result.more;

            MyHTTPResponse res = await MakeAsyncRequest(req);

            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                r.httpException = res.ex;
                r.SetErrMsgFromBytes(res.content);
                return r;
            }

            r.success = true;
            r.content = new StatementsResult(new Json.StringOfJSON(Encoding.UTF8.GetString(res.content)));

            return r;
        }

        // TODO: since param
        public ProfileKeysLRSResponse RetrieveStateIds(Activity activity, Agent agent, Nullable<Guid> registration = null)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());
            queryParams.Add("agent", agent.ToJSON(version));
            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
            }

            return GetProfileKeys("activities/state", queryParams);
        }
        public StateLRSResponse RetrieveState(String id, Activity activity, Agent agent, Nullable<Guid> registration = null)
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

            var resp = GetDocument("activities/state", queryParams, state);
            if (resp.status != HttpStatusCode.OK && resp.status != HttpStatusCode.NotFound)
            {
                r.success = false;
                r.httpException = resp.ex;
                r.SetErrMsgFromBytes(resp.content);
                return r;
            }
            r.success = true;
            r.content = state;

            return r;
        }
        public LRSResponse SaveState(StateDocument state)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("stateId", state.id);
            queryParams.Add("activityId", state.activity.id.ToString());
            queryParams.Add("agent", state.agent.ToJSON(version));
            if (state.registration != null)
            {
                queryParams.Add("registration", state.registration.ToString());
            }

            return SaveDocument("activities/state", queryParams, state);
        }
        public LRSResponse DeleteState(StateDocument state)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("stateId", state.id);
            queryParams.Add("activityId", state.activity.id.ToString());
            queryParams.Add("agent", state.agent.ToJSON(version));
            if (state.registration != null)
            {
                queryParams.Add("registration", state.registration.ToString());
            }

            return DeleteDocument("activities/state", queryParams);
        }
        public LRSResponse ClearState(Activity activity, Agent agent, Nullable<Guid> registration = null)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());
            queryParams.Add("agent", agent.ToJSON(version));
            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
            }

            return DeleteDocument("activities/state", queryParams);
        }

        public async Task<ProfileKeysLRSResponse> RetrieveStateIdsAsync(Activity activity, Agent agent, Nullable<Guid> registration = null)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());
            queryParams.Add("agent", agent.ToJSON(version));
            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
            }

            return await GetProfileKeysAsync("activities/state", queryParams);
        }
        public async Task<StateLRSResponse> RetrieveStateAsync(String id, Activity activity, Agent agent, Nullable<Guid> registration = null)
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

            var resp = await GetDocumentAsync("activities/state", queryParams, state);
            if (resp.status != HttpStatusCode.OK && resp.status != HttpStatusCode.NotFound)
            {
                r.success = false;
                r.httpException = resp.ex;
                r.SetErrMsgFromBytes(resp.content);
                return r;
            }
            r.success = true;
            r.content = state;

            return r;
        }
        public async Task<LRSResponse> SaveStateAsync(StateDocument state)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("stateId", state.id);
            queryParams.Add("activityId", state.activity.id.ToString());
            queryParams.Add("agent", state.agent.ToJSON(version));
            if (state.registration != null)
            {
                queryParams.Add("registration", state.registration.ToString());
            }

            return await SaveDocumentAsync("activities/state", queryParams, state);
        }
        public async Task<LRSResponse> DeleteStateAsync(StateDocument state)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("stateId", state.id);
            queryParams.Add("activityId", state.activity.id.ToString());
            queryParams.Add("agent", state.agent.ToJSON(version));
            if (state.registration != null)
            {
                queryParams.Add("registration", state.registration.ToString());
            }

            return await DeleteDocumentAsync("activities/state", queryParams);
        }
        public async Task<LRSResponse> ClearStateAsync(Activity activity, Agent agent, Nullable<Guid> registration = null)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());
            queryParams.Add("agent", agent.ToJSON(version));
            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
            }

            return await DeleteDocumentAsync("activities/state", queryParams);
        }

        // TODO: since param
        public ProfileKeysLRSResponse RetrieveActivityProfileIds(Activity activity)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());

            return GetProfileKeys("activities/profile", queryParams);
        }
        public ActivityProfileLRSResponse RetrieveActivityProfile(String id, Activity activity)
        {
            var r = new ActivityProfileLRSResponse();

            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", id);
            queryParams.Add("activityId", activity.id.ToString());

            var profile = new ActivityProfileDocument();
            profile.id = id;
            profile.activity = activity;

            var resp = GetDocument("activities/profile", queryParams, profile);
            if (resp.status != HttpStatusCode.OK && resp.status != HttpStatusCode.NotFound)
            {
                r.success = false;
                r.httpException = resp.ex;
                r.SetErrMsgFromBytes(resp.content);
                return r;
            }
            r.success = true;
            r.content = profile;

            return r;
        }
        public LRSResponse SaveActivityProfile(ActivityProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("activityId", profile.activity.id.ToString());

            return SaveDocument("activities/profile", queryParams, profile);
        }
        public LRSResponse DeleteActivityProfile(ActivityProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("activityId", profile.activity.id.ToString());
            // TODO: need to pass Etag?

            return DeleteDocument("activities/profile", queryParams);
        }

        public async Task<ProfileKeysLRSResponse> RetrieveActivityProfileIdsAsync(Activity activity)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());

            return await GetProfileKeysAsync("activities/profile", queryParams);
        }
        public async Task<ActivityProfileLRSResponse> RetrieveActivityProfileAsync(String id, Activity activity)
        {
            var r = new ActivityProfileLRSResponse();

            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", id);
            queryParams.Add("activityId", activity.id.ToString());

            var profile = new ActivityProfileDocument();
            profile.id = id;
            profile.activity = activity;

            var resp = await GetDocumentAsync("activities/profile", queryParams, profile);
            if (resp.status != HttpStatusCode.OK && resp.status != HttpStatusCode.NotFound)
            {
                r.success = false;
                r.httpException = resp.ex;
                r.SetErrMsgFromBytes(resp.content);
                return r;
            }
            r.success = true;
            r.content = profile;

            return r;
        }
        public async Task<LRSResponse> SaveActivityProfileAsync(ActivityProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("activityId", profile.activity.id.ToString());

            return await SaveDocumentAsync("activities/profile", queryParams, profile);
        }
        public async Task<LRSResponse> DeleteActivityProfileAsync(ActivityProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("activityId", profile.activity.id.ToString());
            // TODO: need to pass Etag?

            return await DeleteDocumentAsync("activities/profile", queryParams);
        }

        // TODO: since param
        public ProfileKeysLRSResponse RetrieveAgentProfileIds(Agent agent)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("agent", agent.ToJSON(version));

            return GetProfileKeys("agents/profile", queryParams);
        }
        public AgentProfileLRSResponse RetrieveAgentProfile(String id, Agent agent)
        {
            var r = new AgentProfileLRSResponse();

            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", id);
            queryParams.Add("agent", agent.ToJSON(version));

            var profile = new AgentProfileDocument();
            profile.id = id;
            profile.agent = agent;

            var resp = GetDocument("agents/profile", queryParams, profile);
            if (resp.status != HttpStatusCode.OK && resp.status != HttpStatusCode.NotFound)
            {
                r.success = false;
                r.httpException = resp.ex;
                r.SetErrMsgFromBytes(resp.content);
                return r;
            }
            r.success = true;
            r.content = profile;

            return r;
        }
        public LRSResponse SaveAgentProfile(AgentProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("agent", profile.agent.ToJSON(version));

            return SaveDocument("agents/profile", queryParams, profile);
        }
        public LRSResponse DeleteAgentProfile(AgentProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("agent", profile.agent.ToJSON(version));
            // TODO: need to pass Etag?

            return DeleteDocument("agents/profile", queryParams);
        }

        public async Task<ProfileKeysLRSResponse> RetrieveAgentProfileIdsAsync(Agent agent)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("agent", agent.ToJSON(version));

            return await GetProfileKeysAsync("agents/profile", queryParams);
        }
        public async Task<AgentProfileLRSResponse> RetrieveAgentProfileAsync(String id, Agent agent)
        {
            var r = new AgentProfileLRSResponse();

            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", id);
            queryParams.Add("agent", agent.ToJSON(version));

            var profile = new AgentProfileDocument();
            profile.id = id;
            profile.agent = agent;

            var resp = await GetDocumentAsync("agents/profile", queryParams, profile);
            if (resp.status != HttpStatusCode.OK && resp.status != HttpStatusCode.NotFound)
            {
                r.success = false;
                r.httpException = resp.ex;
                r.SetErrMsgFromBytes(resp.content);
                return r;
            }
            r.success = true;
            r.content = profile;

            return r;
        }
        public async Task<LRSResponse> SaveAgentProfileAsync(AgentProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("agent", profile.agent.ToJSON(version));

            return await SaveDocumentAsync("agents/profile", queryParams, profile);
        }
        public async Task<LRSResponse> DeleteAgentProfileAsync(AgentProfileDocument profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("agent", profile.agent.ToJSON(version));
            // TODO: need to pass Etag?

            return await DeleteDocumentAsync("agents/profile", queryParams);
        }
    }
}
