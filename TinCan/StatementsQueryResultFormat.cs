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

namespace TinCan
{
    public sealed class StatementsQueryResultFormat
    {
        public static readonly StatementsQueryResultFormat IDS = new StatementsQueryResultFormat("ids");
        public static readonly StatementsQueryResultFormat EXACT = new StatementsQueryResultFormat("exact");
        public static readonly StatementsQueryResultFormat CANONICAL = new StatementsQueryResultFormat("canonical");

        private String text;

        private StatementsQueryResultFormat(String value)
        {
            text = value;
        }

        public override String ToString()
        {
            return text;
        }
    }
}
