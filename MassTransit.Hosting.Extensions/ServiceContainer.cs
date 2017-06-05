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
    /// <summary>
    /// Provides an implementation of <see cref="IServiceScope"/> to represent
    /// and manage the very first (i.e. root) instance of a dependency container.
    /// </summary>
    public class ServiceContainer : IServiceScope
    {
        private readonly IDisposable _lifetime;
        private readonly IServiceProvider _serviceProvider;
        private int _isDisposed;

        public ServiceContainer(IServiceProvider serviceProvider, IDisposable lifetime)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        }

        /// <inheritdoc />
        public virtual IServiceProvider ServiceProvider
        {
            get
            {
                var serviceProvider = _serviceProvider;
                if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1)
                    throw new ObjectDisposedException(GetType().FullName);

                return serviceProvider;
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
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0) return;
            _lifetime.Dispose();
        }
    }
}