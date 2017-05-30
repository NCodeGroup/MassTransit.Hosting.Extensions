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
using MassTransit.Hosting.Extensions.GreenPipes;
using Xunit;

namespace MassTransit.Hosting.Extensions.Tests.GreenPipes
{
    public class DictionaryObjectValueProviderTests
    {
        private static readonly Func<IDictionary> Hashtable = () => new Hashtable();
        private static readonly Func<IDictionary> DictStringObject = () => new Dictionary<string, object>();
        private static readonly Func<IDictionary> DictStringString = () => new Dictionary<string, string>();

        public static IEnumerable<object[]> TestData_TryGetValue()
        {
            // Hashtable

            yield return new object[] {string.Empty, "int-int", 1234, Hashtable()};
            yield return new object[] {string.Empty, "int-string", "1234", Hashtable()};
            yield return new object[] {string.Empty, "string", "string", Hashtable()};
            yield return new object[] {string.Empty, "date", DateTime.Now, Hashtable()};

            yield return new object[] {Guid.NewGuid().ToString(), "int-int", 1234, Hashtable()};
            yield return new object[] {Guid.NewGuid().ToString(), "int-string", "1234", Hashtable()};
            yield return new object[] {Guid.NewGuid().ToString(), "string", "string", Hashtable()};
            yield return new object[] {Guid.NewGuid().ToString(), "date", DateTime.Now, Hashtable()};

            // DictStringObject

            yield return new object[] {string.Empty, "int-int", 1234, DictStringObject()};
            yield return new object[] {string.Empty, "int-string", "1234", DictStringObject()};
            yield return new object[] {string.Empty, "string", "string", DictStringObject()};
            yield return new object[] {string.Empty, "date", DateTime.Now, DictStringObject()};

            yield return new object[] {Guid.NewGuid().ToString(), "int-int", 1234, DictStringObject()};
            yield return new object[] {Guid.NewGuid().ToString(), "int-string", "1234", DictStringObject()};
            yield return new object[] {Guid.NewGuid().ToString(), "string", "string", DictStringObject()};
            yield return new object[] {Guid.NewGuid().ToString(), "date", DateTime.Now, DictStringObject()};

            // DictStringString

            yield return new object[] {string.Empty, "int-string", "1234", DictStringString()};
            yield return new object[] {string.Empty, "string", "string", DictStringString()};

            yield return new object[] {Guid.NewGuid().ToString(), "int-string", "1234", DictStringString()};
            yield return new object[] {Guid.NewGuid().ToString(), "string", "string", DictStringString()};
        }

        [Theory]
        [MemberData(nameof(TestData_TryGetValue))]
        public void TestTryGetValue(string prefix, string key, object value, IDictionary dictionary)
        {
            var provider = new DictionaryObjectValueProvider(dictionary, prefix);

            dictionary[prefix + key] = value;

            Assert.True(provider.TryGetValue(key, out object valueObject) && value.Equals(valueObject));
            Assert.Equal(value is int, provider.TryGetValue(key, out int valueInt) && valueInt == (int) value);
            Assert.Equal(value is string,
                provider.TryGetValue(key, out string valueString) && valueString == (string) value);
            Assert.Equal(value is DateTime,
                provider.TryGetValue(key, out DateTime valueDateTime) && valueDateTime == (DateTime) value);
        }
    }
}