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
using GreenPipes.Internals.Mapping;
using GreenPipes.Internals.Reflection;
using MassTransit.Hosting.Extensions.GreenPipes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MassTransit.Hosting.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMassTransit(this IServiceCollection services)
        {
            services.TryAddTransient<IBusServiceConfigurator, BusServiceConfigurator>();
            services.TryAddTransient(typeof(IConsumerFactory<>), typeof(ResolvingConsumerFactory<>));
            services.TryAddTransient(typeof(ISettingsProvider<>), typeof(ConfigurationSettingsProvider<>));
            services.TryAddTransient<ISettingsProvider, ResolvingSettingsProvider>();
            services.TryAddSingleton<IConfigurationProvider, ConfigurationManagerProvider>();
            services.TryAddSingleton<IObjectMapper, GreenPipesObjectMapper>();
            services.TryAddSingleton<IObjectConverterCache, DynamicObjectConverterCache>();
            services.TryAddSingleton<IImplementationBuilder, DynamicImplementationBuilder>();

            return services;
        }

        public static IServiceCollection AddOptionsSettingsProvider<TSettings, TOptions>(
            this IServiceCollection services)
            where TSettings : ISettings
            where TOptions : class, TSettings, new()
        {
            return AddOptionsSettingsProvider<TSettings, TOptions>(services, null);
        }

        public static IServiceCollection AddOptionsSettingsProvider<TSettings, TOptions>(
            this IServiceCollection services, Action<TOptions> configureOptions)
            where TSettings : ISettings
            where TOptions : class, TSettings, new()
        {
            services.AddOptions();
            services.TryAddTransient<ISettingsProvider<TSettings>, OptionsSettingsProvider<TSettings, TOptions>>();
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            return services;
        }
    }
}