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
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MassTransit.Hosting.Extensions.Tests
{
    public class OptionsSettingsProviderTests
    {
        [Fact]
        public void ConfigureFromServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services
                .AddTransient<ISettingsProvider<EndpointSettings>,
                    OptionsSettingsProvider<EndpointSettings, EndpointOptions>>();

            var prefix = Guid.NewGuid().ToString();
            var valueQueueName = Guid.NewGuid().ToString();
            var valueConsumerLimit = new Random().Next();
            services.Configure<EndpointOptions>(options =>
            {
                options.QueueName = valueQueueName;
                options.ConsumerLimit = valueConsumerLimit;
            });

            var serviceProvider = services.BuildServiceProvider();
            using ((IDisposable) serviceProvider)
            {
                var provider = serviceProvider.GetRequiredService<ISettingsProvider<EndpointSettings>>();

                var successFlag = provider.TryGetSettings(out EndpointSettings settings);
                Assert.True(successFlag);
                Assert.Equal(valueQueueName, settings.QueueName);
                Assert.Equal(valueConsumerLimit, settings.ConsumerLimit);

                successFlag = provider.TryGetSettings(prefix, out settings);
                Assert.True(successFlag);
                Assert.Equal(valueQueueName, settings.QueueName);
                Assert.Equal(valueConsumerLimit, settings.ConsumerLimit);
            }
        }
    }
}