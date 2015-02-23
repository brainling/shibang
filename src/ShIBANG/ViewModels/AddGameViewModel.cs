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
using System.Linq;
using System.Windows.Input;
using ShIBANG.Models;
using ShIBANG.Services;

namespace ShIBANG.ViewModels {
    public class AddGameViewModel : ViewModelBase, IFlyoutViewModel {
        private IFlyoutService _flyoutService;
        private bool _hasExistingGame;
        private bool _hasNewGame;
        private bool _linkToSteam = true;
        private Game _newGame;
        private string _searchText;
        private GameResult _selectedGame;
        private readonly IStorageService _storageService;

        public AddGameViewModel (IStorageService storageService, IFlyoutService flyoutService) {
            _storageService = storageService;
            _flyoutService = flyoutService;
            WhenStateUpdated (() => SelectedGame, () => {
                if (SelectedGame == null) {
                    return;
                }

                HasExistingGame = false;
                if (storageService.Games.Any (g => String.Equals (g.Name, SelectedGame.Name, StringComparison.InvariantCultureIgnoreCase))) {
                    HasExistingGame = true;
                }
                else {
                    NewGame = SelectedGame.ToGame ();
                }

                HasNewGame = !HasExistingGame && NewGame != null;
            });
        }

        public bool HasExistingGame {
            get { return _hasExistingGame; }
            set { SetProperty (ref _hasExistingGame, value); }
        }

        public bool HasNewGame {
            get { return _hasNewGame; }
            set { SetProperty (ref _hasNewGame, value); }
        }

        public string SearchText {
            get { return _searchText; }
            set { SetProperty (ref _searchText, value); }
        }

        public Game NewGame {
            get { return _newGame; }
            set { SetProperty (ref _newGame, value); }
        }

        public GameResult SelectedGame {
            get { return _selectedGame; }
            set { SetProperty (ref _selectedGame, value); }
        }

        public bool LinkToSteam {
            get { return _linkToSteam; }
            set { SetProperty (ref _linkToSteam, value); }
        }

        public ICommand AddGame {
            get { return GetCommand ("AddGame", ExecuteAddGame); }
        }

        public void Closing () {
            SearchText = String.Empty;
        }

        public void Closed () {
        }

        public void ExecuteAddGame () {
            _storageService.Games.Add (NewGame);
            _flyoutService.CloseFlyout ("AddGame");
        }
    }
}
