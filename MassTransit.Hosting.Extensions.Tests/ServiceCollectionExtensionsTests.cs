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
using Microsoft.Extensions.Options;
using Xunit;

namespace MassTransit.Hosting.Extensions.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        public static IEnumerable<object[]> TestData_ServiceTypes()
        {
            yield return new object[] { typeof(IBusServiceConfigurator), typeof(BusServiceConfigurator) };
            yield return new object[] { typeof(ISettingsProvider), typeof(SettingsProvider) };
            yield return new object[] { typeof(IConfigurationProvider), typeof(ConfigurationManagerProvider) };
            yield return new object[] { typeof(IObjectMapper), typeof(GreenPipesObjectMapper) };
            yield return new object[] { typeof(IObjectConverterCache), typeof(DynamicObjectConverterCache) };
            yield return new object[] { typeof(IImplementationBuilder), typeof(DynamicImplementationBuilder) };
        }

        [Theory]
        [MemberData(nameof(TestData_ServiceTypes))]
        public void TestServiceTypes(Type serviceType, Type expectedType)
        {
            var services = new ServiceCollection();
            services.AddMassTransit();

            var provider = services.BuildServiceProvider();
            using ((IDisposable)provider)
            {
                var instance = provider.GetRequiredService(serviceType);
                Assert.IsType(expectedType, instance);
            }
        }

        [Fact]
        public void AddOptionsSettingsProvider_Configure_Options()
        {
            var valueQueueName = Guid.NewGuid().ToString();
            var valueConsumerLimit = new Random().Next();

            var services = new ServiceCollection();
            services.AddMassTransit();
            services.AddOptionsSettingsProvider<EndpointSettings, EndpointOptions>(options =>
            {
                options.QueueName = valueQueueName;
                options.ConsumerLimit = valueConsumerLimit;
            });

            var provider = services.BuildServiceProvider();
            using ((IDisposable)provider)
            {
                var optionsProvider = provider.GetRequiredService<IOptions<EndpointOptions>>();

                Assert.Equal(valueQueueName, optionsProvider.Value.QueueName);
                Assert.Equal(valueConsumerLimit, optionsProvider.Value.ConsumerLimit);
            }
        }

        [Fact]
        public void AddOptionsSettingsProvider_Configure_Settings()
        {
            var valueQueueName = Guid.NewGuid().ToString();
            var valueConsumerLimit = new Random().Next();

            var services = new ServiceCollection();
            services.AddMassTransit();
            services.AddOptionsSettingsProvider<EndpointSettings, EndpointOptions>(options =>
            {
                options.QueueName = valueQueueName;
                options.ConsumerLimit = valueConsumerLimit;
            });

            var provider = services.BuildServiceProvider();
            using ((IDisposable)provider)
            {
                var settingsProvider = provider.GetRequiredService<ISettingsProvider<EndpointSettings>>();

                var exists = settingsProvider.TryGetSettings(out EndpointSettings settings);
                Assert.True(exists);
                Assert.Equal(valueQueueName, settings.QueueName);
                Assert.Equal(valueConsumerLimit, settings.ConsumerLimit);

                exists = settingsProvider.TryGetSettings(Guid.NewGuid().ToString(), out EndpointSettings settings2);
                Assert.True(exists);
                Assert.Same(settings, settings2);
                Assert.Equal(valueQueueName, settings2.QueueName);
                Assert.Equal(valueConsumerLimit, settings2.ConsumerLimit);
            }
        }

        [Fact]
        public void SettingsProvider_NoConfigure_UsesConfigurationProvider()
        {
            var prefix = Guid.NewGuid().ToString();
            var valueQueueName = Guid.NewGuid().ToString();
            var valueConsumerLimit = new Random().Next();

            var configurationProvider = new DictionaryConfigurationProvider
            {
                [nameof(EndpointSettings.QueueName)] = valueQueueName,
                [nameof(EndpointSettings.ConsumerLimit)] = valueConsumerLimit.ToString(),

                [prefix + nameof(EndpointSettings.QueueName)] = valueQueueName,
                [prefix + nameof(EndpointSettings.ConsumerLimit)] = valueConsumerLimit.ToString()
            };

            var services = new ServiceCollection();
            services.AddSingleton<IConfigurationProvider>(configurationProvider);
            services.AddMassTransit();

            var provider = services.BuildServiceProvider();
            using ((IDisposable)provider)
            {
                var settingsProvider = provider.GetRequiredService<ISettingsProvider>();

                var exists = settingsProvider.TryGetSettings(out EndpointSettings settings);
                Assert.True(exists);
                Assert.Equal(valueQueueName, settings.QueueName);
                Assert.Equal(valueConsumerLimit, settings.ConsumerLimit);

                exists = settingsProvider.TryGetSettings(prefix, out EndpointSettings settings2);
                Assert.True(exists);
                Assert.NotSame(settings, settings2);
                Assert.Equal(valueQueueName, settings2.QueueName);
                Assert.Equal(valueConsumerLimit, settings2.ConsumerLimit);
            }
        }
    }
}