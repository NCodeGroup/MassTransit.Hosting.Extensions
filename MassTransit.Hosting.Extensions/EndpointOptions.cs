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

namespace MassTransit.Hosting.Extensions
{
    /// <summary>
    /// A simple POCO with public getter and setter properties for the
    /// <see cref="EndpointSettings"/> interface.
    /// </summary>
    public class EndpointOptions : EndpointSettings
    {
        /// <summary>
        /// Provides a default value for <see cref="QueueName"/> for when it is <c>null</c>.
        /// </summary>
        public static int DefaultConsumerLimit { get; set; } = Environment.ProcessorCount * 4;

        /// <inheritdoc />
        public string QueueName { get; set; }

        /// <inheritdoc />
        public int? ConsumerLimit { get; set; }
    }
}