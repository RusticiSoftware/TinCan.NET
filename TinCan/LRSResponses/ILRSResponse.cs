// <copyright file="ILRSResponse.cs" company="Float">
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
    using System;

    /// <summary>
    /// Interface type for responses from an LRS.
    /// </summary>
    public interface ILRSResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:TinCan.LRSResponses.ILRSResponse"/> was successful.
        /// </summary>
        /// <value><c>true</c> if successful; otherwise, <c>false</c>.</value>
        bool success { get; set; }

        /// <summary>
        /// Gets or sets the exception that occurred during communication with the LRS, if any.
        /// </summary>
        /// <value>The exception.</value>
        Exception httpException { get; set; }

        /// <summary>
        /// Gets the error that occurred during communication with the LRS, if any.
        /// </summary>
        /// <value>The error.</value>
        LRSResponseError? Error { get; }

        /// <summary>
        /// Gets the error message that occurred during communication with the LRS, if any.
        /// </summary>
        /// <value>The error message.</value>
        string errMsg { get; }

        /// <summary>
        /// Sets the error message from bytes.
        /// </summary>
        /// <param name="content">Content of the error message.</param>
        /// <param name="code">Code associated with the error message.</param>
        void SetErrMsgFromBytes(byte[] content, int code = -1);
    }
}
