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
using System.Threading.Tasks;
using ShIBANG.Storage;

namespace ShIBANG.Services {
	public interface IStorageService {
		IEnumerable<TObject> LoadObjects<TObject> ()
			where TObject : class;

		void AddObject<TObject> (TObject obj)
			where TObject : class;

		void RemoveObject<TObject> (TObject obj)
			where TObject : class;

		Task CommitAsync ();
	}

	internal class StorageService : IStorageService {
		private readonly StorageContext _context;

		private readonly Dictionary<int, string> _databaseVersionUpdates = new Dictionary<int, string> {
			{ 0, "CreateDatabase.sql" }
		};

		private readonly ISettingsService _settingsService;

		public StorageService (ISettingsService settingsService) {
			_settingsService = settingsService;

			_context = new StorageContext (String.Format ("Data Source={0}", Path.Combine (EnsureFolder (), "storage.db")));
			var version = _context.Database.SqlQuery<int> ("PRAGMA user_version;").Single ();
			UpdateDatabase (_context, version);
		}

		public IEnumerable<TObject> LoadObjects<TObject> ()
			where TObject : class {
			return _context.Set<TObject> ().ToList ();
		}

		public void AddObject<TObject> (TObject obj)
			where TObject : class {
			_context.Set<TObject> ().Add (obj);
		}

		public void RemoveObject<TObject> (TObject obj)
			where TObject : class {
			_context.Set<TObject> ().Remove (obj);
		}

		public Task CommitAsync () {
			return _context.SaveChangesAsync ();
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

		private string GetUpdateSource (string name) {
			using (var reader = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream (String.Format ("ShIBANG.Storage.{0}", name)))) {
				return reader.ReadToEnd ();
			}
		}

		private void UpdateDatabase (DbContext ctx, int currentVersion) {
			if (!_databaseVersionUpdates.ContainsKey (currentVersion)) {
				return;
			}

			var sql = GetUpdateSource (_databaseVersionUpdates[currentVersion]);
			ctx.Database.ExecuteSqlCommand (sql);
			ctx.Database.ExecuteSqlCommand (String.Format ("PRAGMA user_version = {0};", currentVersion + 1));
			UpdateDatabase (ctx, currentVersion + 1);
		}
	}
}
