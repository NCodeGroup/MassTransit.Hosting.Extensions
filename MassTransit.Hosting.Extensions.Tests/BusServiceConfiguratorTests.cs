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
using MassTransit.Builders;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MassTransit.Hosting.Extensions.Tests
{
    public class BusServiceConfiguratorTests
    {
        [Fact]
        public void TestConfigure()
        {
            var mocksToVerify = new List<Mock>();

            var mockReceiveEndpointConfigurator = new Mock<IReceiveEndpointConfigurator>(MockBehavior.Strict);
            mocksToVerify.Add(mockReceiveEndpointConfigurator);

            mockReceiveEndpointConfigurator
                .SetupGet(_ => _.InputAddress)
                .Returns(new Uri("http://localhost"))
                .Verifiable();

            var mockServiceConfigurator = new Mock<IServiceConfigurator>(MockBehavior.Strict);
            mocksToVerify.Add(mockServiceConfigurator);

            var mockServiceSpecification = new Mock<IServiceSpecification>(MockBehavior.Strict);
            mocksToVerify.Add(mockServiceSpecification);

            mockServiceSpecification
                .Setup(_ => _.Configure(mockServiceConfigurator.Object))
                .Verifiable();

            var mockEndpointSpecification = new Mock<IEndpointSpecification>(MockBehavior.Strict);
            mocksToVerify.Add(mockEndpointSpecification);

            mockEndpointSpecification
                .SetupGet(_ => _.QueueName)
                .Returns("queueName")
                .Verifiable();
            mockEndpointSpecification
                .SetupGet(_ => _.ConsumerLimit)
                .Returns(10)
                .Verifiable();
            mockEndpointSpecification
                .Setup(_ => _.Configure(mockReceiveEndpointConfigurator.Object))
                .Verifiable();

            var mockBusObserver = new Mock<IBusObserver>(MockBehavior.Strict);
            // nothing to setup

            mockServiceConfigurator
                .Setup(_ => _.ReceiveEndpoint("queueName", 10, It.IsAny<Action<IReceiveEndpointConfigurator>>()))
                .Callback(
                    (string queueName, int consumerLimit, Action<IReceiveEndpointConfigurator> configureEndpoint) =>
                        configureEndpoint(mockReceiveEndpointConfigurator.Object))
                .Verifiable();
            mockServiceConfigurator
                .Setup(_ => _.AddBusFactorySpecification(It.IsAny<IBusFactorySpecification>()))
                .Verifiable();

            var services = new ServiceCollection();
            services.AddTransient<IBusServiceConfigurator, BusServiceConfigurator>();
            services.AddSingleton(mockServiceConfigurator.Object);
            services.AddSingleton(mockServiceSpecification.Object);
            services.AddSingleton(mockEndpointSpecification.Object);
            services.AddSingleton(mockBusObserver.Object);

            var serviceProvider = services.BuildServiceProvider();
            using ((IDisposable) serviceProvider)
            {
                var configurator = serviceProvider.GetRequiredService<IBusServiceConfigurator>();
                configurator.Configure(mockServiceConfigurator.Object);

                foreach (var mock in mocksToVerify)
                {
                    mock.VerifyAll();
                }
            }
        }
    }
}