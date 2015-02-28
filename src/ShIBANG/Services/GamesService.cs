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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;
using ShIBANG.Models;

namespace ShIBANG.Services {
	public interface IGamesService {
		IList<Game> Games { get; }
		Task CommitAsync ();
	}

	public class GamesService : IGamesService {
		private readonly IEventAggregator _eventAggregator;
		private readonly IStorageService _storageService;
		private ObservableCollection<Game> _games;

		public GamesService (IStorageService storageService, IEventAggregator eventAggregator) {
			_storageService = storageService;
			_eventAggregator = eventAggregator;
		}

		public IList<Game> Games {
			get { return _games ?? (_games = LoadGames ()); }
		}

		public Task CommitAsync () {
			return _storageService.CommitAsync ();
		}

		private ObservableCollection<Game> LoadGames () {
			var games = new ObservableCollection<Game> (_storageService.LoadObjects<Game> ());
			games.CollectionChanged += GamesCollectionChanged;
			foreach (var game in games) {
				game.PropertyChanged += GameStateChanged;
			}

			return games;
		}

		private void GameStateChanged (object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == "CompletionPercent") {
				_eventAggregator.GetEvent<GameCompletionUpdated> ().Publish ((Game) sender);
			}
		}

		private void GamesCollectionChanged (object sender, NotifyCollectionChangedEventArgs e) {
			_eventAggregator.GetEvent<GameListUpdated> ().Publish (Events.EmptyArg);
			switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
					e.NewItems.OfType<Game> ().ToList ().ForEach (g => {
						g.PropertyChanged += GameStateChanged;
						_storageService.AddObject (g);
					});
					break;

				case NotifyCollectionChangedAction.Remove:
					e.OldItems.OfType<Game> ().ToList ().ForEach (g => {
						g.PropertyChanged -= GameStateChanged;
						_storageService.RemoveObject (g);
					});
					break;
			}

			_storageService.CommitAsync ();
		}
	}
}
