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
using System.Threading.Tasks;
using GreenPipes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MassTransit.Hosting.Extensions.Tests
{
    public class TestConsumer : IConsumer<string>, IDisposable
    {
        public TestConsumer(IServiceProvider serviceProvider)
        {
            Instances.Add(this);
            Providers.Add(serviceProvider);
        }

        public static IList<TestConsumer> Instances { get; } = new List<TestConsumer>();
        public static IList<IServiceProvider> Providers { get; } = new List<IServiceProvider>();
        public bool IsDisposed { get; private set; }

        public Task Consume(ConsumeContext<string> context)
        {
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public class ResolvingConsumerFactoryTests
    {
        [Fact]
        public async void ServiceLifetime()
        {
            var services = new ServiceCollection();
            services.AddTransient<IConsumerFactory<TestConsumer>, ResolvingConsumerFactory<TestConsumer>>();
            services.AddTransient<TestConsumer>();

            var outerProvider = services.BuildServiceProvider();
            using ((IDisposable) outerProvider)
            using (var outerScope = outerProvider.CreateScope())
            {
                var provider = outerScope.ServiceProvider;
                var factory = provider.GetRequiredService<IConsumerFactory<TestConsumer>>();

                IServiceScope tempServiceScope = null;
                var mockContext = new Mock<ConsumeContext<string>>(MockBehavior.Strict);
                mockContext
                    .Setup(_ => _.TryGetPayload(out tempServiceScope))
                    .Returns(() => false);

                ConsumerConsumeContext<TestConsumer, string> context = null;
                var mockNext = new Mock<IPipe<ConsumerConsumeContext<TestConsumer, string>>>(MockBehavior.Strict);
                mockNext
                    .Setup(_ => _.Send(It.IsAny<ConsumerConsumeContext<TestConsumer, string>>()))
                    .Returns((ConsumerConsumeContext<TestConsumer, string> argContext) =>
                    {
                        context = argContext;
                        return Task.FromResult(0);
                    });

                Assert.Equal(0, TestConsumer.Instances.Count);
                Assert.Equal(0, TestConsumer.Providers.Count);

                await factory.Send(mockContext.Object, mockNext.Object);

                Assert.Equal(1, TestConsumer.Instances.Count);
                Assert.Equal(1, TestConsumer.Providers.Count);

                Assert.True(TestConsumer.Instances[0].IsDisposed);

                var hasPayload = context.TryGetPayload(out tempServiceScope);
                Assert.True(hasPayload);
                Assert.Same(tempServiceScope.ServiceProvider, TestConsumer.Providers[0]);

                var consumer = provider.GetRequiredService<TestConsumer>();
                Assert.NotNull(consumer);

                Assert.Equal(2, TestConsumer.Instances.Count);
                Assert.Equal(2, TestConsumer.Providers.Count);

                Assert.Same(provider, TestConsumer.Providers[1]);
                Assert.False(TestConsumer.Instances[1].IsDisposed);
            }
            Assert.True(TestConsumer.Instances[1].IsDisposed);
        }
    }
}