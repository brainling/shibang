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
using System.Windows;
using System.Windows.Media;

namespace ShIBANG {
    public static class GlyphHelper {
        #region Public Methods

        public static ImageSource CreateGlyph (string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, Brush foreBrush) {
            if (fontFamily != null && !String.IsNullOrEmpty (text)) {
                var typeface = new Typeface (fontFamily, fontStyle, fontWeight, fontStretch);

                GlyphTypeface glyphTypeface;
                if (!typeface.TryGetGlyphTypeface (out glyphTypeface)) {
                    typeface = new Typeface (new FontFamily (new Uri ("pack://application:,,,"), fontFamily.Source), fontStyle, fontWeight, fontStretch);
                    if (!typeface.TryGetGlyphTypeface (out glyphTypeface)) {
                        throw new InvalidOperationException ("No glyphtypeface found");
                    }
                }

                var glyphIndexes = new ushort[text.Length];
                var advanceWidths = new double[text.Length];

                for (var n = 0; n < text.Length; n++) {
                    ushort glyphIndex;
                    try {
                        glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];
                    }
                    catch (Exception) {
                        glyphIndex = 42;
                    }
                    glyphIndexes[n] = glyphIndex;

                    var width = glyphTypeface.AdvanceWidths[glyphIndex]*1.0;
                    advanceWidths[n] = width;
                }

                try {
                    var gr = new GlyphRun (glyphTypeface, 0, false, 1.0, glyphIndexes,
                        new Point (0, 0), advanceWidths, null, null, null, null, null, null);

                    var glyphRunDrawing = new GlyphRunDrawing (foreBrush, gr);
                    return new DrawingImage (glyphRunDrawing);
                }
                catch (Exception ex) {
                    // ReSharper disable LocalizableElement
                    Console.WriteLine ("Error in generating Glyphrun : " + ex.Message);
                    // ReSharper restore LocalizableElement
                }
            }
            return null;
        }

        #endregion
    }
}
