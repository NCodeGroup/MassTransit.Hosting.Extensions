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
using System.Threading.Tasks;
using GreenPipes;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Hosting.Extensions
{
    /// <summary>
    /// Provides an implementation of <see cref="IConsumerFactory{TConsumer}"/>
    /// that will create a new dependency scope and resolve the <typeparamref name="TConsumer"/>
    /// from the dependency container. This class should be registered in the
    /// dependency container as an open generic.
    /// </summary>
    /// <typeparam name="TConsumer"></typeparam>
    public class ResolvingConsumerFactory<TConsumer> : IConsumerFactory<TConsumer>
        where TConsumer : class
    {
        private readonly IServiceProvider _serviceProvider;

        public ResolvingConsumerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public virtual async Task Send<T>(ConsumeContext<T> context, IPipe<ConsumerConsumeContext<TConsumer, T>> next)
            where T : class
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();

                var consumerConsumeContext = context.PushConsumerScope(consumer, scope);

                await next.Send(consumerConsumeContext).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        void IProbeSite.Probe(ProbeContext context)
        {
            context.CreateConsumerFactoryScope<TConsumer>("resolving");
        }
    }
}