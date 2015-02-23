#region License

// Copyright (c) 2011, Matt Holmes
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the project nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT  LIMITED TO, THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
// THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT  LIMITED TO, PROCUREMENT 
// OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR 
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.IO;
using LightInject;
using Microsoft.Practices.Prism.PubSubEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ShIBANG.Models;

namespace ShIBANG.Services {
    public interface ISettingsService {
        Settings Get ();
        void Update (Settings options);
    }

    internal class SettingsService : ISettingsService {
        private Settings _currentOptions;
        private readonly IEventAggregator _eventAggregator;
        private readonly IServiceContainer _serviceContainer;

        public SettingsService (IServiceContainer serviceContainer, IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;
            _serviceContainer = serviceContainer;
        }

        public Settings Get () {
            if (_currentOptions == null) {
                _currentOptions = Load ();
                _serviceContainer.RegisterInstance (_currentOptions);
            }

            return _currentOptions;
        }

        public void Update (Settings options) {
            var path = Path.Combine (EnsureFolder (), "config.json");
            var json = JsonConvert.SerializeObject (options, Formatting.Indented, new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver ()
            });
            using (var writer = new StreamWriter (path, false)) {
                writer.WriteLine (json);
            }

            _currentOptions = options;
            _eventAggregator.GetEvent<SettingsUpdated> ().Publish (_currentOptions);
        }

        private Settings Load () {
            var path = Path.Combine (EnsureFolder (), "config.json");
            if (!File.Exists (path)) {
                var config = BuildDefault ();
                Update (config);
                return config;
            }

            var serializer = new JsonSerializer ();
            using (var reader = new StreamReader (path)) {
                return serializer.Deserialize<Settings> (new JsonTextReader (reader));
            }
        }

        private Settings BuildDefault () {
            var conf = new Settings { BackupDataOnSave = true };
            return conf;
        }

        private static string EnsureFolder () {
            var folder = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "ShIBANG");
            if (!Directory.Exists (folder)) {
                Directory.CreateDirectory (folder);
            }

#if DEBUG
            folder = Path.Combine (folder, "dev");
            if (!Directory.Exists (folder)) {
                Directory.CreateDirectory (folder);
            }
#endif

            return folder;
        }
    }
}
