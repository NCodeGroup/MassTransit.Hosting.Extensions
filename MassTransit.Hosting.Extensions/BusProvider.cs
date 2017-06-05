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
using System.Threading;

namespace MassTransit.Hosting.Extensions
{
    /// <summary>
    /// Initializes and manages the lifetime for an <see cref="IBus"/> in the
    /// service host.
    /// </summary>
    public interface IBusProvider : IDisposable
    {
        /// <summary>
        /// Contains the <see cref="IBus"/> instance that this class manages.
        /// </summary>
        IBus Bus { get; }
    }

    /// <summary>
    /// Provides an implementation of <see cref="IBusProvider"/> that initializes
    /// an <see cref="IBus"/> for use in the consuming service host. This class
    /// should be registered in the dependency container with scoped lifetime.
    /// </summary>
    /// <remarks>
    /// Additional Dependencies:
    /// <para>
    /// IHostBusFactory: Resolved by '{Transport}HostBusFactory' where {Transport}
    /// can be either 'RabbitMq' or 'ServiceBus', an implementation provided by
    /// MassTransit. By this time, the IHostBusFactory will already have loaded
    /// its settings from ISettingsProvider.
    /// </para>
    /// <para>
    /// IBusServiceConfigurator: Resolved by 'BusServiceConfigurator', an
    /// implementation provided by this library.
    /// </para>
    /// </remarks>
    public class BusProvider : IBusProvider
    {
        private IBusControl _busControl;

        public BusProvider(IHostBusFactory hostBusFactory, IBusServiceConfigurator busServiceConfigurator)
        {
            // ReSharper disable once JoinNullCheckWithUsage
            // Justification: This formatting looks better
            if (hostBusFactory == null)
                throw new ArgumentNullException(nameof(hostBusFactory));
            if (busServiceConfigurator == null)
                throw new ArgumentNullException(nameof(busServiceConfigurator));

            // The 'serviceName' argument is not used in RabbitMq and AzureBus
            // only uses it when its settings are null. So assume their settings
            // are not null and therefore our argument can be empty.
            _busControl = hostBusFactory.CreateBus(busServiceConfigurator, string.Empty);
            _busControl.Start();
        }

        /// <inheritdoc />
        public virtual IBus Bus
        {
            get
            {
                var bus = Interlocked.CompareExchange(ref _busControl, null, null);
                if (bus == null)
                    throw new ObjectDisposedException(GetType().FullName);

                return bus;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            var busControl = Interlocked.Exchange(ref _busControl, null);
            busControl?.Stop();
        }

    }
}