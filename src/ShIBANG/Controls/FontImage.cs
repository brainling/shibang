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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShIBANG.Controls {
    public class FontImage : Image {
        #region Protected Methods

        protected override void OnPropertyChanged (DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged (e);

            if (!IsInitialized
                || (e.Property != BrushProperty && e.Property != GlyphProperty && e.Property != FontFamilyProperty &&
                    e.Property != FontStretchProperty && e.Property != FontStyleProperty && e.Property != FontWeightProperty)
                || e.NewValue == e.OldValue) {
                return;
            }

            BeginInit ();
            Source = CreateGlyph ();
            EndInit ();
        }

        #endregion

        #region Fields

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register (
            "Brush", typeof (Brush), typeof (FontImage), new PropertyMetadata (new SolidColorBrush (Color.FromRgb (0, 0, 0)), BrushChanged));

        public static readonly DependencyProperty DisabledBrushProperty = DependencyProperty.Register (
            "DisabledBrush", typeof (Brush), typeof (FontImage), new PropertyMetadata (new SolidColorBrush (Color.FromRgb (64, 64, 64))));

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register (
            "FontFamily", typeof (FontFamily), typeof (FontImage), new PropertyMetadata (new FontFamily ("Segoe UI Symbol")));

        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register (
            "FontStretch", typeof (FontStretch), typeof (FontImage), new PropertyMetadata (FontStretches.Normal));

        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register (
            "FontStyle", typeof (FontStyle), typeof (FontImage), new PropertyMetadata (FontStyles.Normal));

        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register (
            "FontWeight", typeof (FontWeight), typeof (FontImage), new PropertyMetadata (FontWeights.Normal));

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register (
            "Glyph", typeof (string), typeof (FontImage), new PropertyMetadata (default(string)));

        #endregion

        #region Constructors

        static FontImage () {
            DefaultStyleKeyProperty.OverrideMetadata (typeof (FontImage), new FrameworkPropertyMetadata (typeof (FontImage)));
        }

        public FontImage () {
            IsEnabledChanged += (o, e) => {
                if (!Equals (Brush, DisabledBrush)) {
                    Source = CreateGlyph ();
                }
            };
        }

        #endregion

        #region Properties

        public Brush Brush {
            get { return (Brush) GetValue (BrushProperty); }
            set { SetValue (BrushProperty, value); }
        }

        public Brush DisabledBrush {
            get { return (Brush) GetValue (DisabledBrushProperty); }
            set { SetValue (DisabledBrushProperty, value); }
        }

        public FontFamily FontFamily {
            get { return (FontFamily) GetValue (FontFamilyProperty); }
            set { SetValue (FontFamilyProperty, value); }
        }

        public FontStretch FontStretch {
            get { return (FontStretch) GetValue (FontStretchProperty); }
            set { SetValue (FontStretchProperty, value); }
        }

        public FontStyle FontStyle {
            get { return (FontStyle) GetValue (FontStyleProperty); }
            set { SetValue (FontStyleProperty, value); }
        }

        public FontWeight FontWeight {
            get { return (FontWeight) GetValue (FontWeightProperty); }
            set { SetValue (FontWeightProperty, value); }
        }

        public string Glyph {
            get { return (string) GetValue (GlyphProperty); }
            set { SetValue (GlyphProperty, value); }
        }

        #endregion

        #region Private Methods

        private static void BrushChanged (object sender, DependencyPropertyChangedEventArgs e) {
            var image = sender as FontImage;
            if (image != null) {
                image.Source = image.CreateGlyph ();
            }
        }

        private ImageSource CreateGlyph () {
            if (Glyph != null && FontFamily != null && Brush != null) {
                return GlyphHelper.CreateGlyph (Glyph, FontFamily, FontStyle, FontWeight, FontStretch, IsEnabled ? Brush : DisabledBrush);
            }

            return null;
        }

        #endregion
    }
}
