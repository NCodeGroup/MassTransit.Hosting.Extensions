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
using System.Collections;
using System.Collections.Generic;
using GreenPipes.Internals.Mapping;

namespace MassTransit.Hosting.Extensions.GreenPipes
{
    public class DictionaryObjectValueProvider : IObjectValueProvider
    {
        private readonly IDictionary _dictionary;
        private readonly string _prefix;

        public DictionaryObjectValueProvider(IDictionary dictionary, string prefix = null)
        {
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            _prefix = prefix ?? string.Empty;
        }

        public virtual bool TryGetValue(string name, out object value)
        {
            var key = _prefix + name;

            var dictionaryObject = _dictionary as IDictionary<string, object>;
            if (dictionaryObject != null)
                return dictionaryObject.TryGetValue(key, out value);

            var dictionaryString = _dictionary as IDictionary<string, string>;
            if (dictionaryString != null && dictionaryString.TryGetValue(key, out string stringValue))
            {
                value = stringValue;
                return true;
            }

            if (_dictionary.Contains(key))
            {
                value = _dictionary[key];
                return value != null;
            }

            value = null;
            return false;
        }

        public virtual bool TryGetValue<T>(string name, out T value)
        {
            if (TryGetValue(name, out object obj) && obj is T)
            {
                value = (T) obj;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}