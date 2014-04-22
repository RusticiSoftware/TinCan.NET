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
using System.Web;
using Newtonsoft.Json.Linq;

namespace TinCan
{
    public class RemoteLRS : LRS
    {
        // TODO: add trailing slash
        public Uri endpoint { get; set; }
        public TCAPIVersion version { get; set; }
        // TODO: set auth with username/password
        public String auth { get; set; }
        public Dictionary<String, String> extended { get; set; }

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
        }

        private MyHTTPResponse MakeSyncRequest(MyHTTPRequest req)
        {
            String url;
            if (req.resource.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                url = req.resource;
            }
            else
            {
                url = endpoint.ToString() + req.resource;
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
            Console.WriteLine("RemoteLRS.MakeSyncRequest - url: " + url);
            var webReq = (HttpWebRequest) WebRequest.Create(url);
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

            var resp = new MyHTTPResponse();

            HttpWebResponse webResp;

            try
            {
                webResp = (HttpWebResponse)webReq.GetResponse();
            }
            catch (WebException ex)
            {
                webResp = (HttpWebResponse)ex.Response;
                // TODO: need to grab something from the exception, but what?
            }

            resp.status = webResp.StatusCode;
            Console.WriteLine("RemoteLRS.MakeSyncRequest - status: " + webResp.StatusCode);
            resp.contentType = webResp.ContentType;
            Console.WriteLine("RemoteLRS.MakeSyncRequest - content type: " + webResp.ContentType);
            resp.etag = webResp.Headers.Get("Etag");
            resp.lastModified = webResp.LastModified;
            Console.WriteLine("RemoteLRS.MakeSyncRequest - content length: " + webResp.ContentLength);

            using (var stream = webResp.GetResponseStream())
            {
                resp.content = ReadFully(stream, (int)webResp.ContentLength);
            }
            Console.WriteLine("RemoteLRS.MakeSyncRequest - content: " + System.Text.Encoding.UTF8.GetString(resp.content));

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

        private TinCan.LRSResponse.ProfileKeys GetProfileKeys(String resource, Dictionary<String, String> queryParams)
        {
            var r = new LRSResponse.ProfileKeys();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = resource;
            req.queryParams = queryParams;

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                // TODO: capture the failure reason
                r.success = false;
                return r;
            }

            r.success = true;

            var keys = JArray.Parse(System.Text.Encoding.UTF8.GetString(res.content));
            if (keys.Count > 0) {
                r.content = new List<String>();
                foreach (JToken key in keys) {
                    r.content.Add((String)key);
                }
            }

            return r;
        }

        private MyHTTPResponse GetDocument(String resource, Dictionary<String, String> queryParams, Document.Base document)
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

        private TinCan.LRSResponse.Base SaveDocument(String resource, Dictionary<String, String> queryParams, Document.Base document)
        {
            var r = new LRSResponse.Base();

            var req = new MyHTTPRequest();
            req.method = "PUT";
            req.resource = resource;
            req.queryParams = queryParams;
            req.contentType = document.contentType;
            req.content = document.content;

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.NoContent)
            {
                // TODO: capture the failure reason
                r.success = false;
                return r;
            }

            r.success = true;

            return r;
        }

        private TinCan.LRSResponse.Base DeleteDocument(String resource, Dictionary<String, String> queryParams)
        {
            var r = new LRSResponse.Base();

            var req = new MyHTTPRequest();
            req.method = "DELETE";
            req.resource = resource;
            req.queryParams = queryParams;

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.NoContent)
            {
                // TODO: capture the failure reason
                r.success = false;
                return r;
            }

            r.success = true;

            return r;
        }

        private TinCan.LRSResponse.Statement GetStatement(Dictionary<String, String> queryParams)
        {
            var r = new LRSResponse.Statement();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = "statements";
            req.queryParams = queryParams;

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                // TODO: capture the failure reason
                r.success = false;
                return r;
            }

            r.success = true;
            r.content = new Statement(new json.StringOfJSON(System.Text.Encoding.UTF8.GetString(res.content)));

            return r;
        }

        public TinCan.LRSResponse.About About()
        {
            var r = new LRSResponse.About();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = "about";

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                r.success = false;
                return r;
            }

            r.success = true;
            r.content = new About(System.Text.Encoding.UTF8.GetString(res.content));

            return r;
        }

        public TinCan.LRSResponse.Statement SaveStatement(Statement statement)
        {
            var r = new LRSResponse.Statement();
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
            req.content = System.Text.Encoding.UTF8.GetBytes(statement.toJSON(version));

            var res = MakeSyncRequest(req);
            if (statement.id == null)
            {
                if (res.status != HttpStatusCode.OK)
                {
                    // TODO: capture the failure reason
                    r.success = false;
                    return r;
                }

                var ids = JArray.Parse(System.Text.Encoding.UTF8.GetString(res.content));
                statement.id = new Guid((String)ids[0]);
            }
            else {
                if (res.status != HttpStatusCode.NoContent)
                {
                    // TODO: capture the failure reason
                    r.success = false;
                    return r;
                }
            }

            r.success = true;
            r.content = statement;

            return r;
        }
        public TinCan.LRSResponse.StatementsResult SaveStatements(List<Statement> statements)
        {
            var r = new LRSResponse.StatementsResult();

            var req = new MyHTTPRequest();
            req.resource = "statements";
            req.method = "POST";
            req.contentType = "application/json";

            var jarray = new JArray();
            foreach (Statement st in statements)
            {
                jarray.Add(st.toJObject(version));
            }
            req.content = System.Text.Encoding.UTF8.GetBytes(jarray.ToString());

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                // TODO: capture the failure reason
                r.success = false;
                return r;
            }

            var ids = JArray.Parse(System.Text.Encoding.UTF8.GetString(res.content));
            for (int i = 0; i < ids.Count; i++)
            {
                statements[i].id = new Guid((String)ids[i]);
            }

            r.success = true;
            r.content = new StatementsResult(statements);

            return r;
        }
        public TinCan.LRSResponse.Statement RetrieveStatement(Guid id)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("statementId", id.ToString());

            return GetStatement(queryParams);
        }
        public TinCan.LRSResponse.Statement RetrieveVoidedStatement(Guid id)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("voidedStatementId", id.ToString());

            return GetStatement(queryParams);
        }
        public TinCan.LRSResponse.StatementsResult QueryStatements(StatementsQuery query)
        {
            var r = new LRSResponse.StatementsResult();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = "statements";
            req.queryParams = query.ToParameterMap(version);

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                // TODO: capture the failure reason
                r.success = false;
                return r;
            }

            r.success = true;
            r.content = new StatementsResult(new json.StringOfJSON(System.Text.Encoding.UTF8.GetString(res.content)));

            return r;
        }
        public TinCan.LRSResponse.StatementsResult MoreStatements(StatementsResult result)
        {
            var r = new LRSResponse.StatementsResult();

            var req = new MyHTTPRequest();
            req.method = "GET";
            req.resource = endpoint.GetLeftPart(UriPartial.Authority) + result.more;

            var res = MakeSyncRequest(req);
            if (res.status != HttpStatusCode.OK)
            {
                // TODO: capture the failure reason
                r.success = false;
                return r;
            }

            r.success = true;
            r.content = new StatementsResult(new json.StringOfJSON(System.Text.Encoding.UTF8.GetString(res.content)));

            return r;
        }

        // TODO: since param
        public TinCan.LRSResponse.ProfileKeys RetrieveStateIds(Activity activity, Agent agent, Nullable<Guid> registration = null)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());
            queryParams.Add("agent", agent.toJSON(version));
            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
            }

            return GetProfileKeys("activities/state", queryParams);
        }
        public TinCan.LRSResponse.State RetrieveState(String id, Activity activity, Agent agent, Nullable<Guid> registration = null)
        {
            var r = new LRSResponse.State();

            var queryParams = new Dictionary<String, String>();
            queryParams.Add("stateId", id);
            queryParams.Add("activityId", activity.id.ToString());
            queryParams.Add("agent", agent.toJSON(version));

            var state = new Document.State();
            state.id = id;
            state.activity = activity;
            state.agent = agent;

            var resp = GetDocument("activities/state", queryParams, state);
            if (resp.status != HttpStatusCode.OK && resp.status != HttpStatusCode.NotFound)
            {
                // TODO: capture message from resp.content?
                r.success = false;
            }
            else
            {
                r.success = true;
            }

            return r;
        }
        public TinCan.LRSResponse.Base SaveState(Document.State state)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("stateId", state.id);
            queryParams.Add("activityId", state.activity.id.ToString());
            queryParams.Add("agent", state.agent.toJSON(version));

            return SaveDocument("activities/state", queryParams, state);
        }
        public TinCan.LRSResponse.Base DeleteState(Document.State state)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("stateId", state.id);
            queryParams.Add("activityId", state.activity.id.ToString());
            queryParams.Add("agent", state.agent.toJSON(version));
            if (state.registration != null)
            {
                queryParams.Add("registration", state.registration.ToString());
            }

            return DeleteDocument("activities/state", queryParams);
        }
        public TinCan.LRSResponse.Base ClearState(Activity activity, Agent agent, Nullable<Guid> registration = null)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());
            queryParams.Add("agent", agent.toJSON(version));
            if (registration != null)
            {
                queryParams.Add("registration", registration.ToString());
            }

            return DeleteDocument("activities/state", queryParams);
        }

        // TODO: since param
        public TinCan.LRSResponse.ProfileKeys RetrieveActivityProfileIds(Activity activity)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("activityId", activity.id.ToString());

            return GetProfileKeys("activities/profile", queryParams);
        }
        public TinCan.LRSResponse.ActivityProfile RetrieveActivityProfile(String id, Activity activity)
        {
            var r = new LRSResponse.ActivityProfile();

            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", id);
            queryParams.Add("activityId", activity.id.ToString());

            var profile = new Document.ActivityProfile();
            profile.id = id;
            profile.activity = activity;

            var resp = GetDocument("activities/profile", queryParams, profile);
            if (resp.status != HttpStatusCode.OK && resp.status != HttpStatusCode.NotFound)
            {
                // TODO: capture message from resp.content?
                r.success = false;
            }
            else
            {
                r.success = true;
            }

            return r;
        }
        public TinCan.LRSResponse.Base SaveActivityProfile(Document.ActivityProfile profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("activityId", profile.activity.id.ToString());

            return SaveDocument("activities/profile", queryParams, profile);
        }
        public TinCan.LRSResponse.Base DeleteActivityProfile(Document.ActivityProfile profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("activityId", profile.activity.id.ToString());
            // TODO: need to pass Etag?

            return DeleteDocument("activities/profile", queryParams);
        }

        // TODO: since param
        public TinCan.LRSResponse.ProfileKeys RetrieveAgentProfileIds(Agent agent)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("agent", agent.toJSON(version));

            return GetProfileKeys("agents/profile", queryParams);
        }
        public TinCan.LRSResponse.AgentProfile RetrieveAgentProfile(String id, Agent agent)
        {
            var r = new LRSResponse.AgentProfile();

            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", id);
            queryParams.Add("agent", agent.toJSON(version));

            var profile = new Document.AgentProfile();
            profile.id = id;
            profile.agent = agent;

            var resp = GetDocument("agents/profile", queryParams, profile);
            if (resp.status != HttpStatusCode.OK && resp.status != HttpStatusCode.NotFound)
            {
                // TODO: capture message from resp.content?
                r.success = false;
            }
            else
            {
                r.success = true;
            }

            return r;
        }
        public TinCan.LRSResponse.Base SaveAgentProfile(Document.AgentProfile profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("agent", profile.agent.toJSON(version));

            return SaveDocument("agents/profile", queryParams, profile);
        }
        public TinCan.LRSResponse.Base DeleteAgentProfile(Document.AgentProfile profile)
        {
            var queryParams = new Dictionary<String, String>();
            queryParams.Add("profileId", profile.id);
            queryParams.Add("agent", profile.agent.toJSON(version));
            // TODO: need to pass Etag?

            return DeleteDocument("agents/profile", queryParams);
        }
    }
}
