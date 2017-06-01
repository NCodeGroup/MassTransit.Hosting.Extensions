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
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Hosting.Extensions
{
    public interface IServiceHost
    {
        void Start();
        void Stop();
    }

    public class ServiceHost : IServiceHost
    {
        private readonly IServiceProvider _serviceProvider;
        private IServiceScope _serviceScope;

        public ServiceHost(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected virtual IServiceScope GetOrCreateScope()
        {
            var serviceScope = Interlocked.CompareExchange(ref _serviceScope, null, null);
            if (serviceScope != null) return serviceScope;

            serviceScope = _serviceProvider.CreateScope();
            var prevScope = Interlocked.CompareExchange(ref _serviceScope, serviceScope, null);
            if (prevScope == null) return serviceScope;

            serviceScope.Dispose();
            serviceScope = prevScope;

            return serviceScope;
        }

        public virtual void Start()
        {
            var serviceScope = GetOrCreateScope();

            // resolve the bus provider in a new scope which will automatically start the bus
            // no need to store the return value since the bus provider should be registered with scope lifetime
            serviceScope.ServiceProvider.GetRequiredService<IBusProvider>();
        }

        public virtual void Stop()
        {
            // dispose the scope which will dispose the bus provider and stop the bus
            var serviceScope = Interlocked.Exchange(ref _serviceScope, null);
            serviceScope?.Dispose();
        }

    }
}