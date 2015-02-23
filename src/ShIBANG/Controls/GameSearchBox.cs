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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ShIBANG.Models;
using ShIBANG.Services;

namespace ShIBANG.Controls {
    public class GameSearchBox : TextBox {
        private bool _searchPaused;
        private Task _searchTask;
        private readonly DispatcherTimer _settleTimer;

        public GameSearchBox () {
            _settleTimer = new DispatcherTimer (DispatcherPriority.Input) {
                Interval = TimeSpan.FromMilliseconds (SettleDuration)
            };
            _settleTimer.Tick += SettleTimerOnTick;
        }

        static GameSearchBox () {
            DefaultStyleKeyProperty.OverrideMetadata (typeof (GameSearchBox), new FrameworkPropertyMetadata (typeof (GameSearchBox)));
        }

        public bool HasSelectedGame {
            get { return (bool) GetValue (HasSelectedGameProperty); }
            set { SetValue (HasSelectedGameProperty, value); }
        }

        public int MinimumText {
            get { return (int) GetValue (MinimumTextProperty); }
            set { SetValue (MinimumTextProperty, value); }
        }

        public int SettleDuration {
            get { return (int) GetValue (SettleDurationProperty); }
            set { SetValue (SettleDurationProperty, value); }
        }

        public GameResult SelectedGame {
            get { return (GameResult) GetValue (SelectedGameProperty); }
            set { SetValue (SelectedGameProperty, value); }
        }

        public bool QueuedSearch {
            get { return (bool) GetValue (QueuedSearchProperty); }
            set { SetValue (QueuedSearchProperty, value); }
        }

        public ObservableCollection<GameResult> SearchResults {
            get { return (ObservableCollection<GameResult>) GetValue (SearchResultsProperty); }
            private set { SetValue (SearchResultsPropertyKey, value); }
        }

        public bool HasResults {
            get { return (bool) GetValue (HasResultsProperty); }
            set { SetValue (HasResultsProperty, value); }
        }

        public string SelectedGameThumbnailUrl {
            get { return (string) GetValue (SelectedGameThumbnailUrlProperty); }
            set { SetValue (SelectedGameThumbnailUrlProperty, value); }
        }

        private void SettleTimerOnTick (object sender, EventArgs eventArgs) {
            _settleTimer.Stop ();
            if (_searchTask != null) {
                QueuedSearch = true;
            }
            else {
                _searchTask = FindGameAsync (Text);
            }
        }

        protected override void OnPropertyChanged (DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged (e);

            if (e.Property == SearchResultsProperty) {
                HasResults = SearchResults != null && SearchResults.Count > 0;
            }
            else if (e.Property == SelectedGameProperty) {
                if (SelectedGame == null && Text.Length >= MinimumText) {
                    return;
                }

                HasSelectedGame = SelectedGame != null;
                if (SelectedGame == null) {
                    return;
                }


                _searchPaused = true;
                SearchResults = null;
                SelectedGameThumbnailUrl = SelectedGame.ThumbnailImageUrl;
                Text = SelectedGame.Name;
                _searchPaused = false;
            }
        }

        private static void OnSettleDurationUpdated (DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var box = (GameSearchBox) sender;
            box._settleTimer.Stop ();
            box._settleTimer.Interval = TimeSpan.FromMilliseconds (box.SettleDuration);
        }

        protected override void OnTextChanged (TextChangedEventArgs e) {
            base.OnTextChanged (e);

            if (_searchPaused) {
                return;
            }

            if (Text.Length <= MinimumText) {
                SearchResults = null;
                return;
            }

            if (_settleTimer.IsEnabled) {
                _settleTimer.Stop ();
            }

            _settleTimer.Start ();
        }

        private Task FindGameAsync (string name) {
            return App.Current.Container.GetInstance<IGameSourceService> ().FindAsync (name)
                      .ContinueWith (r => {
                          Dispatcher.Invoke (() => {
                              _searchTask = null;
                              SearchResults = new ObservableCollection<GameResult> (r.Result);
                              if (!QueuedSearch) {
                                  return;
                              }

                              QueuedSearch = false;
                              _searchTask = FindGameAsync (Text);
                          });
                      });
        }

        public static readonly DependencyProperty SelectedGameThumbnailUrlProperty = DependencyProperty.Register (
            "SelectedGameThumbnailUrl", typeof (string), typeof (GameSearchBox), new PropertyMetadata (default(string)));

        public static readonly DependencyProperty HasSelectedGameProperty = DependencyProperty.Register (
            "HasSelectedGame", typeof (bool), typeof (GameSearchBox), new FrameworkPropertyMetadata (default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty MinimumTextProperty = DependencyProperty.Register (
            "MinimumText", typeof (int), typeof (GameSearchBox), new PropertyMetadata (2));

        public static readonly DependencyProperty HasResultsProperty = DependencyProperty.Register (
            "HasResults", typeof (bool), typeof (GameSearchBox), new PropertyMetadata (default(bool)));

        public static readonly DependencyProperty QueuedSearchProperty = DependencyProperty.Register (
            "QueuedSearch", typeof (bool), typeof (GameSearchBox), new PropertyMetadata (default(bool)));

        public static readonly DependencyProperty SelectedGameProperty = DependencyProperty.Register (
            "SelectedGame", typeof (GameResult), typeof (GameSearchBox), new PropertyMetadata (default(GameResult)));

        public static readonly DependencyProperty SettleDurationProperty = DependencyProperty.Register (
            "SettleDuration", typeof (int), typeof (GameSearchBox), new PropertyMetadata (400, OnSettleDurationUpdated));

        private static readonly DependencyPropertyKey SearchResultsPropertyKey = DependencyProperty.RegisterReadOnly (
            "SearchResults", typeof (ObservableCollection<GameResult>), typeof (GameSearchBox), new PropertyMetadata (default (ObservableCollection<GameResult>)));

        public static readonly DependencyProperty SearchResultsProperty = SearchResultsPropertyKey.DependencyProperty;
    }
}
