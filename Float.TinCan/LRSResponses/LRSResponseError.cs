// <copyright file="LRSResponseError.cs" company="Float">
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

namespace TinCan.LRSResponses
{
    public struct LRSResponseError
    {
        public LRSResponseError(string message, int code = -1)
        {
            Message = message;
            Code = code;
        }

        public string Message { get; }

        public int Code { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is LRSResponseError error && error.Message == Message && error.Code == Code;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Message.GetHashCode() ^ Code.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[LRSResponseError: Message={Message}, Code={Code}]";
        }
    }
}
