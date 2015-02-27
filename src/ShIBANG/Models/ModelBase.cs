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
using System.Linq.Expressions;
using Microsoft.Practices.Prism.Mvvm;

namespace ShIBANG.Models {
    public abstract class ModelBase : BindableBase {
        private readonly Dictionary<string, List<Action>> _stateHandlers = new Dictionary<string, List<Action>> ();

        protected ModelBase () {
            PropertyChanged += (o, e) => {
                if (_stateHandlers.ContainsKey (e.PropertyName)) {
                    _stateHandlers[e.PropertyName].ForEach (a => a ());
                }
            };
        }

        protected void WhenStateUpdated<TProperty> (Expression<Func<TProperty>> property, Action action) {
            var name = PropertySupport.ExtractPropertyName (property);
            if (!_stateHandlers.ContainsKey (name)) {
                _stateHandlers[name] = new List<Action> ();
            }

            _stateHandlers[name].Add (action);
        }
    }
}
