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
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.PubSubEvents;
using ShIBANG.Models;
using ShIBANG.Services;
using ShIBANG.Views;

namespace ShIBANG.ViewModels {
    internal class GamesViewModel : ViewModelBase {
        private bool _isSearchReady;
        private Game _selectedGame;
        private readonly IFlyoutService _flyoutService;
        private readonly IGamesService _gamesService;

        public GamesViewModel (IGamesService gamesService, IFlyoutService flyoutService, IGameSourceService gameSourceService,
            IEventAggregator eventAggregator) {
            _gamesService = gamesService;
            _flyoutService = flyoutService;
            IsSearchReady = gameSourceService.IsReady;

            eventAggregator.GetEvent<SearchReadinessUpdated> ().Subscribe (sr => { IsSearchReady = sr; });
        }

        public IEnumerable<Game> Games {
            get { return _gamesService.Games; }
        }

        public ICommand AddGame {
            get { return GetCommand ("AddGame", ExecuteAddGame, CanExecuteAddGame); }
        }

        public ICommand EditGame {
            get { return GetCommand ("EditGame", ExecuteEditGame, CanExecuteEditGame); }
        }

        public Game SelectedGame {
            get { return _selectedGame; }
            set { SetProperty (ref _selectedGame, value); }
        }

        public bool IsSearchReady {
            get { return _isSearchReady; }
            set { SetProperty (ref _isSearchReady, value); }
        }

        public ICommand RemoveGame {
            get { return GetCommand ("RemoveGame", ExecuteRemoveGame, CanExecuteRemoveGame); }
        }

        public void SaveGame (Game game) {
            if (game == null) {
                return;
            }

            game.LastUpdated = DateTime.UtcNow;
            //_gamesService.SaveGames ();
        }

        private void ExecuteAddGame () {
            _flyoutService.ShowFlyout ("AddGame", App.Current.Container.GetInstance<AddGameView> (), width: 450);
        }

        private void ExecuteRemoveGame () {
            var res = MessageBox.Show (String.Format ("Are you sure you want to remove {0} from your game list?", SelectedGame.Name), "Remove Game",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes) {
                _gamesService.Games.Remove (SelectedGame);
            }
        }

        private bool CanExecuteRemoveGame () {
            return SelectedGame != null;
        }

        private bool CanExecuteEditGame () {
            return SelectedGame != null;
        }

        private bool CanExecuteAddGame () {
            return IsSearchReady;
        }

        private void ExecuteEditGame () {
        }
    }
}
