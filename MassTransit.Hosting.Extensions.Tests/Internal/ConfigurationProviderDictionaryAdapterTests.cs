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
using MassTransit.Hosting.Extensions.Internal;
using Moq;
using Xunit;

namespace MassTransit.Hosting.Extensions.Tests.Internal
{
    public class ConfigurationProviderDictionaryAdapterTests
    {
        public static IEnumerable<object[]> TestData_EndpointSettings()
        {
            yield return new object[] {null};
            yield return new object[] {string.Empty};
            yield return new object[] {Guid.NewGuid().ToString()};
        }

        [Theory]
        [MemberData(nameof(TestData_EndpointSettings))]
        public void TestEndpointSettings(string prefix)
        {
            var configurationProviderMock = new Mock<IConfigurationProvider>(MockBehavior.Strict);

            const string keyQueueName = nameof(EndpointSettings.QueueName);
            const string keyConsumerLimit = nameof(EndpointSettings.ConsumerLimit);

            var valueQueueName = Guid.NewGuid().ToString();
            var valueConsumerLimit = Guid.NewGuid().ToString();

            // ReSharper disable once RedundantAssignment
            // Yes it is used in the lambda setup
            // Output variables must be assigned before setup
            var tempString = valueQueueName;
            configurationProviderMock
                .Setup(_ => _.TryGetSetting(prefix + keyQueueName, out tempString))
                .Returns(() => true);
            tempString = valueConsumerLimit;
            configurationProviderMock
                .Setup(_ => _.TryGetSetting(prefix + keyConsumerLimit, out tempString))
                .Returns(() => true);

            var adapter =
                new ConfigurationProviderDictionaryAdapter<EndpointSettings>(configurationProviderMock.Object, prefix);

            Assert.Equal(2, adapter.Count);
            Assert.Equal(2, adapter.Keys.Count);
            Assert.Equal(2, adapter.Values.Count);

            Assert.False(adapter.ContainsKey("foo"));
            Assert.True(adapter.ContainsKey(prefix + keyQueueName));
            Assert.True(adapter.ContainsKey(prefix + keyConsumerLimit));
            if (!string.IsNullOrEmpty(prefix))
            {
                Assert.False(adapter.ContainsKey(keyQueueName));
                Assert.False(adapter.ContainsKey(keyConsumerLimit));
            }

            Assert.False(adapter.ContainsValue("bar"));
            Assert.True(adapter.ContainsValue(valueQueueName));
            Assert.True(adapter.ContainsValue(valueConsumerLimit));

            Assert.DoesNotContain("foo", adapter.Keys);
            Assert.Contains(prefix + keyQueueName, adapter.Keys);
            Assert.Contains(prefix + keyConsumerLimit, adapter.Keys);
            if (!string.IsNullOrEmpty(prefix))
            {
                Assert.DoesNotContain(keyQueueName, adapter.Keys);
                Assert.DoesNotContain(keyConsumerLimit, adapter.Keys);
            }

            Assert.DoesNotContain("bar", adapter.Values);
            Assert.Contains(valueQueueName, adapter.Values);
            Assert.Contains(valueConsumerLimit, adapter.Values);

            Assert.Equal(null, adapter["foo"]);
            Assert.Equal(valueQueueName, adapter[prefix + keyQueueName]);
            Assert.Equal(valueConsumerLimit, adapter[prefix + keyConsumerLimit]);
            if (!string.IsNullOrEmpty(prefix))
            {
                Assert.Equal(null, adapter[keyQueueName]);
                Assert.Equal(null, adapter[keyConsumerLimit]);
            }

            Assert.False(adapter.TryGetValue("foo", out tempString));
            Assert.Null(tempString);

            Assert.True(adapter.TryGetValue(prefix + keyQueueName, out tempString) && tempString == valueQueueName);
            Assert.True(adapter.TryGetValue(prefix + keyConsumerLimit, out tempString) &&
                        tempString == valueConsumerLimit);
            if (!string.IsNullOrEmpty(prefix))
            {
                Assert.False(adapter.TryGetValue(keyQueueName, out tempString));
                Assert.False(adapter.TryGetValue(keyConsumerLimit, out tempString));
            }

            var kvpFooBar = new KeyValuePair<string, string>("foo", "bar");
            var kvpQueueName = new KeyValuePair<string, string>(prefix + keyQueueName, valueQueueName);
            var kvpConsumerLimit = new KeyValuePair<string, string>(prefix + keyConsumerLimit, valueConsumerLimit);
            Assert.False(adapter.Contains(kvpFooBar));
            Assert.True(adapter.Contains(kvpQueueName));
            Assert.True(adapter.Contains(kvpConsumerLimit));
            Assert.DoesNotContain(kvpFooBar, adapter);
            Assert.Contains(kvpQueueName, adapter);
            Assert.Contains(kvpConsumerLimit, adapter);

            Assert.True(adapter.IsReadOnly);
            Assert.True(adapter.IsFixedSize);
            Assert.Same(adapter.SyncRoot, ((ICollection) adapter.Keys).SyncRoot);
            Assert.Same(adapter.SyncRoot, ((ICollection) adapter.Values).SyncRoot);
        }
    }
}