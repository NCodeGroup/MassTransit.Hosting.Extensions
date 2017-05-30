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
using GreenPipes.Internals.Mapping;
using GreenPipes.Internals.Reflection;
using MassTransit.Hosting.Extensions.GreenPipes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MassTransit.Hosting.Extensions.Tests
{
    public class ConfigurationSettingsProviderTests
    {
        private static readonly Random Random = new Random();

        private static readonly Func<IObjectMapper> GreenPipes =
            () => new GreenPipesObjectMapper(new DynamicObjectConverterCache(new DynamicImplementationBuilder()));

        private static readonly Func<IObjectMapper> Castle =
            () => new CastleObjectMapper();

        private static readonly Func<string> RandomString =
            () => Guid.NewGuid().ToString();

        private static readonly Func<int> RandomNumber =
            () => Random.Next();

        public static IEnumerable<object[]> TestData_EndpointSettings()
        {
            yield return new object[] {GreenPipes(), null, RandomString(), RandomNumber()};
            yield return new object[] {GreenPipes(), string.Empty, RandomString(), RandomNumber()};
            yield return new object[] {GreenPipes(), RandomString(), RandomString(), RandomNumber()};
            yield return new object[] {GreenPipes(), RandomString(), RandomString(), null};
            yield return new object[] {GreenPipes(), RandomString(), null, RandomNumber()};

            yield return new object[] {Castle(), null, RandomString(), RandomNumber()};
            yield return new object[] {Castle(), string.Empty, RandomString(), RandomNumber()};
            yield return new object[] {Castle(), RandomString(), RandomString(), RandomNumber()};
            yield return new object[] {Castle(), RandomString(), RandomString(), null};
            yield return new object[] {Castle(), RandomString(), null, RandomNumber()};
        }

        [Theory]
        [MemberData(nameof(TestData_EndpointSettings))]
        public void TestEndpointSettings_FromDirect(IObjectMapper objectMapper, string prefix, string valueQueueName,
            int? valueConsumerLimit)
        {
            var confgurationProviderMock = new Mock<IConfigurationProvider>(MockBehavior.Strict);

            const string keyQueueName = nameof(EndpointSettings.QueueName);
            const string keyConsumerLimit = nameof(EndpointSettings.ConsumerLimit);

            // ReSharper disable once RedundantAssignment
            // Yes it is used in the lambda setup
            // Output variables must be assigned before setup
            var tempString = valueQueueName;
            confgurationProviderMock
                .Setup(_ => _.TryGetSetting(prefix + keyQueueName, out tempString))
                .Returns(() => valueQueueName != null);
            tempString = valueConsumerLimit?.ToString();
            confgurationProviderMock
                .Setup(_ => _.TryGetSetting(prefix + keyConsumerLimit, out tempString))
                .Returns(() => valueConsumerLimit.HasValue);

            if (!string.IsNullOrEmpty(prefix))
            {
                string tempString2;
                confgurationProviderMock
                    .Setup(_ => _.TryGetSetting(keyQueueName, out tempString2))
                    .Returns(() => false);
                confgurationProviderMock
                    .Setup(_ => _.TryGetSetting(keyConsumerLimit, out tempString2))
                    .Returns(() => false);
            }

            var settingsProvider =
                new ConfigurationSettingsProvider<EndpointSettings>(confgurationProviderMock.Object, objectMapper);

            var successFlag = settingsProvider.TryGetSettings(prefix, out EndpointSettings settings);
            Assert.True(successFlag);
            Assert.Equal(valueQueueName, settings.QueueName);
            Assert.Equal(valueConsumerLimit, settings.ConsumerLimit);

            if (string.IsNullOrEmpty(prefix)) return;

            successFlag = settingsProvider.TryGetSettings(null, out settings);
            Assert.False(successFlag);
        }

        [Theory]
        [MemberData(nameof(TestData_EndpointSettings))]
        public void TestEndpointSettings_FromServiceCollection(IObjectMapper objectMapper, string prefix,
            string valueQueueName, int? valueConsumerLimit)
        {
            var confgurationProviderMock = new Mock<IConfigurationProvider>(MockBehavior.Strict);

            const string keyQueueName = nameof(EndpointSettings.QueueName);
            const string keyConsumerLimit = nameof(EndpointSettings.ConsumerLimit);

            // ReSharper disable once RedundantAssignment
            // Yes it is used in the lambda setup
            // Output variables must be assigned before setup
            var tempString = valueQueueName;
            confgurationProviderMock
                .Setup(_ => _.TryGetSetting(prefix + keyQueueName, out tempString))
                .Returns(() => valueQueueName != null);
            tempString = valueConsumerLimit?.ToString();
            confgurationProviderMock
                .Setup(_ => _.TryGetSetting(prefix + keyConsumerLimit, out tempString))
                .Returns(() => valueConsumerLimit.HasValue);

            if (!string.IsNullOrEmpty(prefix))
            {
                string tempString2;
                confgurationProviderMock
                    .Setup(_ => _.TryGetSetting(keyQueueName, out tempString2))
                    .Returns(() => false);
                confgurationProviderMock
                    .Setup(_ => _.TryGetSetting(keyConsumerLimit, out tempString2))
                    .Returns(() => false);
            }

            var services = new ServiceCollection();
            services.AddSingleton(objectMapper);
            services.AddSingleton(confgurationProviderMock.Object);
            services.AddTransient<ISettingsProvider, ResolvingSettingsProvider>();
            services.AddTransient(typeof(ISettingsProvider<>), typeof(ConfigurationSettingsProvider<>));

            var provider = services.BuildServiceProvider();
            using ((IDisposable) provider)
            {
                var settingsProvider = provider.GetRequiredService<ISettingsProvider>();

                var successFlag = settingsProvider.TryGetSettings(prefix, out EndpointSettings settings);
                Assert.True(successFlag);
                Assert.Equal(valueQueueName, settings.QueueName);
                Assert.Equal(valueConsumerLimit, settings.ConsumerLimit);

                if (string.IsNullOrEmpty(prefix)) return;

                successFlag = settingsProvider.TryGetSettings(null, out settings);
                Assert.False(successFlag);
            }
        }

        public static IEnumerable<object[]> TestData_EmptyConfiguration()
        {
            yield return new object[] {GreenPipes(), null};
            yield return new object[] {GreenPipes(), string.Empty};
            yield return new object[] {GreenPipes(), RandomString()};
            yield return new object[] {GreenPipes(), RandomString()};

            yield return new object[] {Castle(), null};
            yield return new object[] {Castle(), string.Empty};
            yield return new object[] {Castle(), RandomString()};
            yield return new object[] {Castle(), RandomString()};
        }

        [Theory]
        [MemberData(nameof(TestData_EmptyConfiguration))]
        public void EmptyConfiguration(IObjectMapper objectMapper, string prefix)
        {
            var confgurationProviderMock = new Mock<IConfigurationProvider>(MockBehavior.Strict);

            string tempString;
            confgurationProviderMock
                .Setup(_ => _.TryGetSetting(It.IsAny<string>(), out tempString))
                .Returns(() => false);

            var settingsProvider =
                new ConfigurationSettingsProvider<EndpointSettings>(confgurationProviderMock.Object, objectMapper);

            var successFlag = settingsProvider.TryGetSettings(prefix, out EndpointSettings settings);
            Assert.False(successFlag);
            Assert.Null(settings);
        }
    }
}