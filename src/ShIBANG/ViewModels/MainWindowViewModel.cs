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

using System.Windows.Input;
using Microsoft.Practices.Prism.PubSubEvents;
using ShIBANG.Services;
using ShIBANG.Views;

namespace ShIBANG.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        private bool _isSearchReady;
        private string _searchReadinessMessage;
        private readonly IFlyoutService _flyoutService;

        public MainWindowViewModel (IFlyoutService flyoutService, IEventAggregator eventAggregator, IGameSourceService gameSourceService) {
            _flyoutService = flyoutService;            
            eventAggregator.GetEvent<SearchReadinessUpdated> ().Subscribe (sr => { IsSearchReady = sr; }, true);

            WhenStateUpdated (() => IsSearchReady, () => {
                SearchReadinessMessage = IsSearchReady ? "GiantBomb search is ready." :
                    "GiantBomb search is NOT ready. Please senter an API key in the settings or check your internet connection.";
            });
            IsSearchReady = gameSourceService.IsReady;
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

        private void ExecuteShowSettings () {
            _flyoutService.ShowFlyout ("Settings", App.Current.Container.GetInstance<SettingsView> ());
        }
    }
}
