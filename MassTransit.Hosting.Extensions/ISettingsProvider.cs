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

namespace MassTransit.Hosting.Extensions
{
    /// <summary>
    /// Provides the ability to resolve <see cref="ISettings"/> from the
    /// dependency container.
    /// </summary>
    /// <typeparam name="T">The settings type to return.</typeparam>
    public interface ISettingsProvider<T>
        where T : ISettings
    {
        /// <summary>
        /// Try to get the settings of the specified type, without a prefix.
        /// </summary>
        /// <param name="settings">The output settings value.</param>
        /// <returns><c>True</c> if the settings could be resolved.</returns>
        bool TryGetSettings(out T settings);

        /// <summary>
        /// Try to get the settings of the specified type, using the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to use when resolving configuration values from the property names.</param>
        /// <param name="settings">The output settings value.</param>
        /// <returns><c>True</c> if the settings could be resolved.</returns>
        bool TryGetSettings(string prefix, out T settings);
    }
}