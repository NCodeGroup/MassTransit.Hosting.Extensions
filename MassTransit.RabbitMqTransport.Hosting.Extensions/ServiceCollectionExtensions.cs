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
using MassTransit.Hosting;
using MassTransit.Hosting.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MassTransit.RabbitMqTransport.Hosting.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services)
        {
            return AddRabbitMq(services, null);
        }

        public static IServiceCollection AddRabbitMq(this IServiceCollection services, Action<RabbitMqOptions> configureOptions)
        {
            services.AddMassTransit();
            if (configureOptions != null)
            {
                services.AddOptionsSettingsProvider<RabbitMqSettings, RabbitMqOptions>(configureOptions);
            }
            services.TryAddTransient<IHostBusFactory, RabbitMqHostBusFactory>();

            return services;
        }
    }
}