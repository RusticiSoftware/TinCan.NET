// <copyright file="TCAPIVersion.cs" company="Float">
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace TinCan
{
    public sealed class TCAPIVersion
    {
        public static readonly TCAPIVersion V103 = new ("1.0.3");
        public static readonly TCAPIVersion V102 = new ("1.0.2");
        public static readonly TCAPIVersion V101 = new ("1.0.1");
        public static readonly TCAPIVersion V100 = new ("1.0.0");
        public static readonly TCAPIVersion V095 = new ("0.95");
        public static readonly TCAPIVersion V090 = new ("0.9");

        static Dictionary<string, TCAPIVersion> known;
        static Dictionary<string, TCAPIVersion> supported;

        readonly string text;

        public TCAPIVersion(string value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));

            var s = GetKnown();

            if (!s.ContainsKey(value))
            {
                throw new ArgumentException($"Unrecognized version: {value}");
            }

            text = value;
        }

        public static TCAPIVersion latest() => V101;

        public static Dictionary<string, TCAPIVersion> GetKnown()
        {
            known ??= new Dictionary<string, TCAPIVersion>
            {
                { "1.0.3", V103 },
                { "1.0.2", V102 },
                { "1.0.1", V101 },
                { "1.0.0", V100 },
                { "0.95", V095 },
                { "0.9", V090 },
            };

            return known;
        }

        public static Dictionary<string, TCAPIVersion> GetSupported()
        {
            supported ??= new Dictionary<string, TCAPIVersion>
            {
                { "1.0.3", V103 },
                { "1.0.2", V102 },
                { "1.0.1", V101 },
                { "1.0.0", V100 },
            };

            return supported;
        }

        public override string ToString()
        {
            return text;
        }
    }
}
