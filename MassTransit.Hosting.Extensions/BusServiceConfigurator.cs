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
using MassTransit.Internals.Extensions;
using MassTransit.Logging;

namespace MassTransit.Hosting.Extensions
{
    /// <summary>
    /// Provides a default implementation of <see cref="IBusServiceConfigurator"/>
    /// that configures the service by using the following configurator types
    /// from the dependency container.
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="IServiceSpecification"/> to configure the service</description>
    /// </item>
    /// <item>
    /// <description><see cref="IEndpointSpecification"/> to configure a receive endpoint with consumers</description>
    /// </item>
    /// <item>
    /// <description><see cref="IBusObserver"/> to observe events produced by the bus</description>
    /// </item>
    /// </list>
    /// </summary>
    public class BusServiceConfigurator : IBusServiceConfigurator
    {
        private readonly ILog _log = Logger.Get<BusServiceConfigurator>();
        private readonly IEnumerable<IServiceSpecification> _serviceSpecifications;
        private readonly IEnumerable<IEndpointSpecification> _endpointSpecifications;
        private readonly IEnumerable<IBusObserver> _busObservers;

        /// <summary>
        /// Initializes a new instance of the <see cref="BusServiceConfigurator"/> class.
        /// </summary>
        /// <param name="serviceSpecifications">A collection of <see cref="IServiceSpecification"/> to configure the service.</param>
        /// <param name="endpointSpecifications">A collection of <see cref="IEndpointSpecification"/> to configure receive endpoints with consumers.</param>
        /// <param name="busObservers">A collection of <see cref="IBusObserver"/> tp observe events produced by the bus.</param>
        public BusServiceConfigurator(IEnumerable<IServiceSpecification> serviceSpecifications, IEnumerable<IEndpointSpecification> endpointSpecifications, IEnumerable<IBusObserver> busObservers)
        {
            _serviceSpecifications = serviceSpecifications ?? throw new ArgumentNullException(nameof(serviceSpecifications));
            _endpointSpecifications = endpointSpecifications ?? throw new ArgumentNullException(nameof(endpointSpecifications));
            _busObservers = busObservers ?? throw new ArgumentNullException(nameof(busObservers));
        }

        /// <inheritdoc />
        public virtual void Configure(IServiceConfigurator configurator)
        {
            foreach (var serviceSpecification in _serviceSpecifications)
            {
                _log.Info($"Configuring Service: {serviceSpecification.GetType().GetTypeName()}");

                serviceSpecification.Configure(configurator);
            }

            foreach (var specification in _endpointSpecifications)
            {
                var queueName = specification.QueueName;
                var consumerLimit = specification.ConsumerLimit;

                _log.Info($"Configuring Endpoint: {specification.GetType().GetTypeName()} (queue-name: {queueName}, consumer-limit: {consumerLimit})");

                configurator.ReceiveEndpoint(queueName, consumerLimit, x =>
                {
                    specification.Configure(x);

                    _log.Info($"Configured Endpoint: {specification.GetType().GetTypeName()} (address: {x.InputAddress})");
                });
            }

            foreach (var observer in _busObservers)
            {
                _log.Info($"Configuring Bus Observer: {observer.GetType().GetTypeName()}");

                configurator.BusObserver(observer);
            }
        }
    }
}