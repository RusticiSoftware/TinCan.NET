/*
    Copyright 2014-2017 Rustici Software

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

using System.Collections.Generic;
using System.Text;
using System.Net.Http;

namespace TinCan
{
    /// <summary>
    /// LRS HTTP request.
    /// </summary>
    sealed class LRSHttpRequest
    {
        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>The method.</value>
        public HttpMethod Method { get; internal set; }

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        /// <value>The resource.</value>
        public string Resource { get; internal set; }

        /// <summary>
        /// Gets or sets the query parameters.
        /// </summary>
        /// <value>The query parameters.</value>
        public Dictionary<string, string> QueryParams { get; internal set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>The headers.</value>
        public Dictionary<string, string> Headers { get; internal set; }

        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
        public string ContentType { get; internal set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public byte[] Content { get; internal set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:TinCan.LRSHttpRequest"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:TinCan.LRSHttpRequest"/>.</returns>
        public override string ToString()
        {
            return string.Format(
                "[MyHTTPRequest: method={0}, resource={1}, queryParams={2}, headers={3}, contentType={4}, content={5}]",
                Method, Resource, string.Join(";", QueryParams), Headers, ContentType, Encoding.UTF8.GetString(Content, 0, Content.Length));
        }
    }
}
