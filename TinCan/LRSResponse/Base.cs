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

namespace TinCan.LRSResponse
{
    //
    // this isn't abstract because some responses for an LRS won't have content
    // so in those cases we can get by just returning this base response
    //
    public class Base
    {
        public bool success { get; set; }
        //public HttpWebResponse httpResponse { get; set; }
        //public Exception httpException { get; set; }

        // TODO: can we do a generic content property here?
        // see: http://stackoverflow.com/questions/2587236/generic-property-in-c-sharp
        // would need to wrap the various possible content types: Document, StatementsResult, About, ?
    }
}
