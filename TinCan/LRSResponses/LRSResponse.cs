// <copyright file="LRSResponse.cs" company="Float">
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
using System.Text;

namespace TinCan.LRSResponses
{
    // this isn't abstract because some responses for an LRS won't have content
    // so in those cases we can get by just returning this base response
    public class LRSResponse : ILRSResponse
    {
        public LRSResponse()
        {
        }

        public LRSResponse(bool success)
        {
            this.success = success;
        }

        public bool success { get; set; }

        public Exception httpException { get; set; }

        public LRSResponseError? Error { get; protected set; }

        public string errMsg => Error?.Message;

        public void SetErrMsgFromBytes(byte[] content, int code = -1)
        {
            Error = new LRSResponseError(content == null ? null : Encoding.UTF8.GetString(content, 0, content.Length), code);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[LRSResponse: success={success}, httpException={httpException}, errMsg={errMsg}]";
        }
    }
}
