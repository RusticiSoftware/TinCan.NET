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

using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace TinCan
{
    /// <summary>
    /// LRS HTTP response.
    /// </summary>
    sealed class LRSHttpResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:TinCan.MyHTTPResponse"/> class.
        /// </summary>
        public LRSHttpResponse() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TinCan.MyHTTPResponse"/> class.
        /// </summary>
        /// <param name="response">Web resp.</param>
        public LRSHttpResponse(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            Status = response.StatusCode;

            if (response.Content?.Headers?.ContentType != null)
            {
                ContentType = response.Content.Headers.ContentType.ToString();
            }

            if (response.Headers.ETag != null)
            {
                Etag = response.Headers.ETag.ToString();
            }

            if (response.Content?.Headers?.LastModified != null)
            {
                LastModified = response.Content.Headers.LastModified.Value.LocalDateTime;
            }

            Content = response.Content.ReadAsByteArrayAsync().Result;
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public HttpStatusCode Status { get; internal set; }

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
        /// Gets or sets the last modified.
        /// </summary>
        /// <value>The last modified.</value>
        public DateTime LastModified { get; internal set; }

        /// <summary>
        /// Gets or sets the etag.
        /// </summary>
        /// <value>The etag.</value>
        public string Etag { get; internal set; }

        /// <summary>
        /// Gets or sets the ex.
        /// </summary>
        /// <value>The ex.</value>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:TinCan.LRSHttpResponse"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:TinCan.LRSHttpResponse"/>.</returns>
        public override string ToString()
        {
            return string.Format("[MyHTTPResponse: Status={0}, ContentType={1}, Content={2}, LastModified={3}, Etag={4}, Exception={5}]",
                                 Status, ContentType, Encoding.UTF8.GetString(Content, 0, Content.Length), LastModified, Etag, Exception);
        }
    }
}
