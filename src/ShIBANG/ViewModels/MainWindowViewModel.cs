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
using System.Windows.Media;
using Microsoft.Practices.Prism.PubSubEvents;
using ShIBANG.Services;
using ShIBANG.Views;

namespace ShIBANG.ViewModels {
    internal class ShouldIState {
        public string Message { get; set; }
        public Brush Brush { get; set; }
    }

    internal class MainWindowViewModel : ViewModelBase {
        private static readonly ShouldIState[] States = new ShouldIState[] {
            new ShouldIState {
                Brush = Brushes.ForestGreen,
                Message = "Your back log is looking pretty clear. You could stand to buy a new game."
            },
            new ShouldIState {
                Brush = Brushes.Goldenrod,
                Message = "You have plenty you could be playing, but another game wouldn't hurt either."
            },
            new ShouldIState {
                Brush = Brushes.DarkRed,
                Message = "No. Just no. Stop. Put the credit card down."
            }
        };

        private bool _isSearchReady;
        private string _searchReadinessMessage;
        private ShouldIState _shouldI;
        private readonly IFlyoutService _flyoutService;
        private readonly IGamesService _gamesService;

        public MainWindowViewModel (IFlyoutService flyoutService, IEventAggregator eventAggregator, IGameSourceService gameSourceService,
            IGamesService gamesService) {
            _flyoutService = flyoutService;
            _gamesService = gamesService;

            eventAggregator.GetEvent<SearchReadinessUpdated> ().Subscribe (sr => { IsSearchReady = sr; }, true);
            eventAggregator.GetEvent<GameCompletionUpdated> ().Subscribe (game => { CalculateShouldIStatus (); });
            eventAggregator.GetEvent<GameListUpdated> ().Subscribe (game => { CalculateShouldIStatus (); });

            WhenStateUpdated (() => IsSearchReady, () => {
                SearchReadinessMessage = IsSearchReady ? "GiantBomb search is ready." :
                    "GiantBomb search is NOT ready. Please senter an API key in the settings or check your internet connection.";
            });
            IsSearchReady = gameSourceService.IsReady;
            CalculateShouldIStatus ();
        }

        public ICommand ShowSettings {
            get { return GetCommand ("ShowSettings", ExecuteShowSettings); }
        }

        public bool IsSearchReady {
            get { return _isSearchReady; }
            set { SetProperty (ref _isSearchReady, value); }
        }

        public string SearchReadinessMessage {
            get { return _searchReadinessMessage; }
            set { SetProperty (ref _searchReadinessMessage, value); }
        }

        public ShouldIState ShouldI {
            get { return _shouldI; }
            set { SetProperty (ref _shouldI, value); }
        }

        private void ExecuteShowSettings () {
            _flyoutService.ShowFlyout ("Settings", App.Current.Container.GetInstance<SettingsView> ());
        }

        private void CalculateShouldIStatus () {
            var comp = _gamesService.Games.Sum (g => Math.Max (0.0, Math.Min (1.0, g.CompletionPercent / 100.0f)));
            var percent = (comp / _gamesService.Games.Count) * 100.0;
            if (percent <= 45.0) {
                ShouldI = States[2];
            }
            else if (percent <= 75.0) {
                ShouldI = States[1];
            }
            else {
                ShouldI = States[0];
            }
        }
    }
}
