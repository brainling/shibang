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
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ShIBANG.Storage;

namespace ShIBANG.Services {
    public interface IStorageService {
        IEnumerable<TObject> LoadObjects<TObject> ();
        void SaveObjects<TObject> (IEnumerable<TObject> objects);
    }

    internal class StorageService : IStorageService {
        private readonly ISettingsService _settingsService;
        private readonly Dictionary<int, string> _databaseVersionUpdates = new Dictionary<int, string> {
            { 0, "CreateDatabase.sql" }
        };

        public StorageService (ISettingsService settingsService) {
            _settingsService = settingsService;

            using (var ctx = new StorageContext (String.Format ("Data Source={0}", Path.Combine (EnsureFolder (), "storage.db")))) {
                var version = ctx.Database.SqlQuery<int> ("PRAGMA user_version;").Single ();
                UpdateDatabase (ctx, version);
            }
        }

        public IEnumerable<TObject> LoadObjects<TObject> () {
            using (var reader = new StreamReader (ObjectsFile<TObject> ())) {
                return JsonConvert.DeserializeObject<List<TObject>> (reader.ReadToEnd (), new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver ()
                });
            }
        }

        public void SaveObjects<TObject> (IEnumerable<TObject> objects) {
            var file = ObjectsFile<TObject> ();
            if (_settingsService.Get ().BackupDataOnSave && File.Exists (file)) {
                File.Copy (file, String.Format ("{0}.bak", file), true);
            }

            using (var writer = new StreamWriter (ObjectsFile<TObject> (), false)) {
                writer.WriteLine (JsonConvert.SerializeObject (objects, Formatting.Indented, new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver (),
                    NullValueHandling = NullValueHandling.Ignore
                }));
            }
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

        private string ObjectsFile<TObject> () {
            var name = typeof (TObject).Name.Pluralize ().ToLower ();
            var file = Path.Combine (EnsureFolder (), String.Format ("{0}.json", name));
            if (!File.Exists (file)) {
                // Write a default enmpty array to the objects file
                using (var writer = new StreamWriter (file, false)) {
                    writer.WriteLine ("[]");
                }
            }
            return file;
        }

        private string GetUpdateSource (string name) {
            using (var reader = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream (String.Format ("ShIBANG.Storage.{0}", name)))) {
                return reader.ReadToEnd ();
            }
        }

        private void UpdateDatabase (DbContext ctx, int currentVersion) {
            if (_databaseVersionUpdates.ContainsKey (currentVersion)) {
                var sql = GetUpdateSource (_databaseVersionUpdates[currentVersion]);
                ctx.Database.ExecuteSqlCommand (sql);
                ctx.Database.ExecuteSqlCommand (String.Format ("PRAGMA user_version = {0};", currentVersion + 1));
                UpdateDatabase (ctx, currentVersion + 1);
            }
        }
    }
}
