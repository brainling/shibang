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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ShIBANG.Models;

namespace ShIBANG.Services {
    public interface IStorageService {
        IList<Game> Games { get; }
        IList<Category> Categories { get; }
    }

    internal class StorageService : IStorageService {
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Game> _games;
        private ISettingsService _settingsService;

        public StorageService (ISettingsService settingsService) {
            _settingsService = settingsService;
        }

        public IList<Game> Games {
            get { return _games ?? (_games = LoadObjects<Game> ()); }
        }

        public IList<Category> Categories {
            get { return _categories ?? (_categories = LoadObjects<Category> ()); }
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

        private void ObjectsCollectionChanged<TObject> (object sender, NotifyCollectionChangedEventArgs e) {
            var file = ObjectsFile<TObject> ();
            if (_settingsService.Get ().BackupDataOnSave && File.Exists (file)) {
                File.Copy (file, String.Format ("{0}.bak", file), true);
            }

            using (var writer = new StreamWriter (ObjectsFile<TObject> (), false)) {
                writer.WriteLine (JsonConvert.SerializeObject ((IList<TObject>) sender, Formatting.Indented, new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver (),
                    NullValueHandling = NullValueHandling.Ignore
                }));
            }
        }

        private ObservableCollection<TObject> LoadObjects<TObject> () {
            ObservableCollection<TObject> objects;
            using (var reader = new StreamReader (ObjectsFile<TObject> ())) {
                objects = new ObservableCollection<TObject> (JsonConvert.DeserializeObject<List<TObject>> (reader.ReadToEnd (), new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver ()
                }));
            }

            objects.CollectionChanged += ObjectsCollectionChanged<TObject>;

            return objects;
        }
    }
}
