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
    public interface IBusProvider : IDisposable
    {
        IBus Bus { get; }
    }

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