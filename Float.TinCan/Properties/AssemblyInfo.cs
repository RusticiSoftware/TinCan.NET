// <copyright file="Activity.cs" company="Float">
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

using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

// allow testing of internal classes, only on debug builds
#if DEBUG
[assembly: InternalsVisibleTo("Float.Core.Tests")]
#endif

[assembly: NeutralResourcesLanguage("en")]
[assembly: AssemblyVersion("0.0.1")]
