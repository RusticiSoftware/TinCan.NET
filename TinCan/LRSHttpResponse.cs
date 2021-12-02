// <copyright file="LRSHttpResponse.cs" company="Float">
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
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace TinCan
{
    /// <summary>
    /// LRS HTTP response.
    /// </summary>
    internal struct LRSHttpResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LRSHttpResponse"/> struct.
        /// </summary>
        /// <param name="response">Web resp.</param>
        internal LRSHttpResponse(HttpResponseMessage response)
        {
            Contract.Requires(response != null);

            Status = response.StatusCode;
            ContentType = response.Content?.Headers?.ContentType?.ToString();
            Etag = response.Headers?.ETag?.ToString();

            if (response.Content?.Headers?.LastModified != null)
            {
                LastModified = response.Content.Headers.LastModified.Value.LocalDateTime;
            }
            else
            {
                LastModified = null;
            }

            Content = response.Content?.ReadAsByteArrayAsync().Result;
            Exception = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSHttpResponse"/> struct.
        /// </summary>
        /// <param name="exception">Web exception.</param>
        internal LRSHttpResponse(WebException exception)
        {
            Contract.Requires(exception != null);

            if (exception.Response == null)
            {
                Content = Encoding.UTF8.GetBytes("Web exception without '.Response'");
                ContentType = null;
            }
            else
            {
                using (var stream = exception.Response.GetResponseStream())
                {
                    Content = ReadFully(stream, (int)exception.Response.ContentLength);
                }

                ContentType = exception.Response.ContentType;
            }

            Status = null;
            Exception = exception;
            LastModified = null;
            Etag = null;
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        internal HttpStatusCode? Status { get; }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
        internal string ContentType { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>The content.</value>
        internal byte[] Content { get; }

        /// <summary>
        /// Gets the last modified.
        /// </summary>
        /// <value>The last modified.</value>
        internal DateTime? LastModified { get; }

        /// <summary>
        /// Gets the etag.
        /// </summary>
        /// <value>The etag.</value>
        internal string Etag { get; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>The exception.</value>
        internal Exception Exception { get; }

        /// <summary>
        /// See http://www.yoda.arachsys.com/csharp/readbinary.html no license found
        ///
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from.</param>
        /// <param name="initialLength">The initial buffer length.</param>
        static byte[] ReadFully(Stream stream, int initialLength)
        {
            Contract.Requires(stream != null);

            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            var buffer = new byte[initialLength];
            var read = 0;
            int chunk;

            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    var nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    var newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }

            // Buffer is now too big. Shrink it.
            var ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}
