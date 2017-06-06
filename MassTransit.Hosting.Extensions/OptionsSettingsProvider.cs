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
using Microsoft.Extensions.Options;

namespace MassTransit.Hosting.Extensions
{
    /// <summary>
    /// Provides an implementation of <see cref="ISettingsProvider{T}"/> that
    /// will try to retrieve the settings using Microsoft's <see cref="IOptions{TOptions}"/>
    /// abstractions.
    /// </summary>
    /// <typeparam name="TSettings">The settings type to return.</typeparam>
    /// <typeparam name="TOptions">The type of options being requested.</typeparam>
    public class OptionsSettingsProvider<TSettings, TOptions> : ISettingsProvider<TSettings>
        where TSettings : ISettings
        where TOptions : class, TSettings, new()
    {
        private readonly IServiceProvider _serviceProvider;

        public OptionsSettingsProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public virtual bool TryGetSettings(out TSettings settings)
        {
            var provider = _serviceProvider.GetService<IOptions<TOptions>>();
            if (provider != null)
            {
                settings = provider.Value;
                return true;
            }
            settings = default(TOptions);
            return false;
        }

        /// <inheritdoc />
        public virtual bool TryGetSettings(string prefix, out TSettings settings)
        {
            return TryGetSettings(out settings);
        }

    }
}