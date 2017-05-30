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
    public class BusServiceConfigurator : IBusServiceConfigurator
    {
        private readonly IEnumerable<IBusObserver> _busObservers;
        private readonly IEnumerable<IEndpointSpecification> _endpointSpecifications;
        private readonly ILog _log = Logger.Get<BusServiceConfigurator>();
        private readonly IEnumerable<IServiceSpecification> _serviceSpecifications;

        public BusServiceConfigurator(IEnumerable<IServiceSpecification> serviceSpecifications, IEnumerable<IEndpointSpecification> endpointSpecifications, IEnumerable<IBusObserver> busObservers)
        {
            _serviceSpecifications = serviceSpecifications ?? throw new ArgumentNullException(nameof(serviceSpecifications));
            _endpointSpecifications = endpointSpecifications ?? throw new ArgumentNullException(nameof(endpointSpecifications));
            _busObservers = busObservers ?? throw new ArgumentNullException(nameof(busObservers));
        }

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