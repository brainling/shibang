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
using System.Linq;
using System.Threading.Tasks;
using GiantBomb.Api;
using Microsoft.Practices.Prism.PubSubEvents;
using ShIBANG.Models;

namespace ShIBANG.Services {
    public interface IGameSourceService {
        bool IsReady { get; }
        Task<IEnumerable<GameResult>> FindAsync (string beginsWith, int limit = 10);
    }

    internal class GameSourceService : IGameSourceService {
        private const string DefaultImage = "http://www.giantbomb.com/bundles/phoenixsite/images/core/loose/no-image-30x30.png";
        private IGiantBombRestClient _client;

        public GameSourceService (ISettingsService settings, IEventAggregator eventAggregator) {
            var key = settings.Get ().GiantBombApiKey;
            if (!String.IsNullOrWhiteSpace (key)) {
                _client = new GiantBombRestClient (key);
                eventAggregator.GetEvent<SearchReadinessUpdated> ().Publish (IsReady);
            }

            eventAggregator.GetEvent<SettingsUpdated> ().Subscribe (s => {
                _client = null;

                if (!String.IsNullOrWhiteSpace (s.GiantBombApiKey)) {
                    _client = new GiantBombRestClient (s.GiantBombApiKey);
                }

                eventAggregator.GetEvent<SearchReadinessUpdated> ().Publish (IsReady);
            }, true);
        }

        public bool IsReady {
            get { return _client != null; }
        }

        public Task<IEnumerable<GameResult>> FindAsync (string beginsWith, int limit = 10) {
            return Task.Run (() => {
                if (!IsReady) {
                    return Enumerable.Empty<GameResult> ();
                }

                return _client.SearchForGames (beginsWith, 1, limit, new[] { "id", "name", "image", "site_detail_url" }).Select (g => new GameResult {
                    Id = g.Id,
                    SiteUrl = g.SiteDetailUrl,
                    Name = g.Name,
                    ThumbnailImageUrl = g.Image == null ? DefaultImage : g.Image.TinyUrl,
                    MediumImageUrl = g.Image == null ? DefaultImage : g.Image.MediumUrl
                });
            });
        }
    }
}
