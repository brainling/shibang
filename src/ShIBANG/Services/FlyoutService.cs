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

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Humanizer;
using MahApps.Metro.Controls;

namespace ShIBANG.Services {
    public interface IFlyoutViewModel {
        void Closing ();
        void Closed ();
    }

    public interface IFlyoutService {
        void SetHost (MetroWindow host);
        void ShowFlyout (string name, FrameworkElement content, Position position = Position.Right, int width = 300);
        void CloseFlyout (string name);
    }

    internal class FlyoutData {
        public IFlyoutViewModel ViewModel { get; set; }
        public string Name { get; set; }
    }

    internal class FlyoutService : IFlyoutService {
        private readonly ObservableCollection<Flyout> _flyouts = new ObservableCollection<Flyout> ();

        public void SetHost (MetroWindow host) {
            if (host.Flyouts == null) {
                host.Flyouts = new FlyoutsControl ();
            }


            var binding = new Binding {
                Source = _flyouts
            };
            BindingOperations.SetBinding (host.Flyouts, ItemsControl.ItemsSourceProperty, binding);
        }

        public void ShowFlyout (string name, FrameworkElement content, Position position = Position.Right, int width = 300) {
            var flyout = GetFlyout (name);
            if (flyout == null) {
                flyout = new Flyout {
                    Header = name.Humanize(LetterCasing.Title),
                    Content = content,
                    Position = position,
                    Width = width,
                    Theme = FlyoutTheme.Inverse,
                    Tag = new FlyoutData {
                        Name = name
                    }
                };

                if (content.DataContext is IFlyoutViewModel) {
                    ((FlyoutData) flyout.Tag).ViewModel = content.DataContext as IFlyoutViewModel;
                    flyout.IsOpenChanged += (o, e) => {
                        if (!flyout.IsOpen) {
                            GetViewModel (flyout).Closing ();
                        }
                    };
                    flyout.ClosingFinished += (o, e) => { GetViewModel (flyout).Closed (); };
                }

                _flyouts.Add (flyout);
            }

            flyout.IsOpen = true;
        }

        public void CloseFlyout (string name) {
            var flyout = GetFlyout (name);
            if (flyout != null) {
                flyout.IsOpen = false;
            }
        }

        private Flyout GetFlyout (string name) {
            return _flyouts.FirstOrDefault (f => ((FlyoutData) f.Tag).Name == name);
        }

        private IFlyoutViewModel GetViewModel (Flyout flyout) {
            return ((FlyoutData) flyout.Tag).ViewModel;
        }
    }
}
