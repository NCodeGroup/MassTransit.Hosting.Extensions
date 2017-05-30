#region Copyright Preamble
// 
//    Copyright © 2017 NCode Group
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MassTransit.Hosting.Extensions.Tests
{
    public class DictionaryConfigurationProvider : IConfigurationProvider
    {
        private IDictionary<string, string> Dictionary { get; } = new Dictionary<string, string>();

        public string this[string key]
        {
            get => Dictionary[key];
            set => Dictionary[key] = value;
        }

        public bool TryGetSetting(string name, out string value)
        {
            return Dictionary.TryGetValue(name, out value);
        }

        public bool TryGetConnectionString(string name, out string connectionString, out string providerName)
        {
            throw new NotImplementedException();
        }

        public bool TryGetNameValueCollectionSection(string section, out NameValueCollection collection)
        {
            throw new NotImplementedException();
        }
    }
}