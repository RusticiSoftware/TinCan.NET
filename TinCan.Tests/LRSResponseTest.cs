// <copyright file="LRSResponseTest.cs" company="Float">
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

namespace TinCan.Tests
{
    using System.Text;
    using Xunit;
    using TinCan.LRSResponses;

    /// <summary>
    /// Added this test as a logic error in 1.0.3.16 caused this to fail.
    /// </summary>
    public class LRSResponseTest
    {
        [Fact]
        public void TestSetErrMsgFromBytesNull()
        {
            var errorResponse = new StatementsResultLRSResponse { success = false };
            errorResponse.SetErrMsgFromBytes(null, (int)System.Net.HttpStatusCode.BadRequest);
            Assert.Null(errorResponse.errMsg);
            Assert.NotNull(errorResponse.Error);
            Assert.Null(errorResponse.Error?.Message);
            Assert.Equal(400, errorResponse.Error?.Code);
        }

        [Fact]
        public void TestSetErrMsgFromBytesNotNull()
        {
            var errorResponse = new StatementsResultLRSResponse { success = false };
            var encodedMessage = Encoding.UTF8.GetBytes("This is a test message.");
            errorResponse.SetErrMsgFromBytes(encodedMessage, (int)System.Net.HttpStatusCode.BadRequest);
            Assert.Equal("This is a test message.", errorResponse.errMsg);
            Assert.Equal(new LRSResponseError("This is a test message.", 400), errorResponse.Error);
        }
    }
}
