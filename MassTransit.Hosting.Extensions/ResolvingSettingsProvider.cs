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

namespace MassTransit.Hosting.Extensions
{
    /// <summary>
    /// Provides an implementation of <see cref="ISettingsProvider"/> that will
    /// attempt to retreive the requested settings by resolving them from the
    /// dependency container using <see cref="ISettingsProvider{T}"/>.
    /// </summary>
    public class ResolvingSettingsProvider : ISettingsProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ResolvingSettingsProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public virtual bool TryGetSettings<T>(string prefix, out T settings) where T : ISettings
        {
            settings = default(T);
            var provider = _serviceProvider.GetService<ISettingsProvider<T>>();
            return provider?.TryGetSettings(prefix, out settings) ?? false;
        }

        /// <inheritdoc />
        public virtual bool TryGetSettings<T>(out T settings) where T : ISettings
        {
            settings = default(T);
            var provider = _serviceProvider.GetService<ISettingsProvider<T>>();
            return provider?.TryGetSettings(out settings) ?? false;
        }
    }
}