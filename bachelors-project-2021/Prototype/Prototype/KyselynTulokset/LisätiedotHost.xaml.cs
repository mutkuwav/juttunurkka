/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Joni Lapinkoski
          2026 Matias Meriläinen

This file is part of "Juttunurkka".

Juttunurkka is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License.

Juttunurkka is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Juttunurkka.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Prototype
{
    public partial class LisätiedotHost : ContentPage
    {
        public ObservableCollection<HostResultItem> Results { get; } = new();

        public LisätiedotHost()
        {
            InitializeComponent();
            Microsoft.Maui.Controls.NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = this;
            LoadHostEmojiResults();
        }

        private void LoadHostEmojiResults()
        {
            var emojiMap = OnlineSession.Current.Emojis.ToDictionary(e => e.ID, e => e);

            var raw = OnlineSession.Current.EmojiResults
                .OrderByDescending(kv => kv.Value)
                .ToList();

            double maxCount = raw.Any() ? raw.Max(kv => kv.Value) : 1;
            const double maxBarHeight = 200;
            const double minBarHeight = 30;

            var barColors = new[] { Colors.Blue, Colors.Red, Colors.Green, Colors.Orange, Colors.Purple, Colors.Teal, Colors.Gray };

            for (int i = 0; i < raw.Count; i++)
            {
                var kv = raw[i];

                string title = emojiMap.TryGetValue(kv.Key, out var emoji) ? emoji.Name : $"Emoji {kv.Key}";
                string image = emojiMap.TryGetValue(kv.Key, out var emoji2) ? emoji2.ImageSource : $"emoji{kv.Key}lowres.png";

                double rawHeight = (kv.Value / maxCount) * maxBarHeight;
                double heightPx = Math.Max(rawHeight, minBarHeight);

                Results.Add(new HostResultItem
                {
                    Image = image,
                    Title = title,
                    Amount = kv.Value.ToString(),
                    ScalePx = heightPx,
                    Color = barColors.Length > i ? barColors[i] : Colors.Gray
                });
            }
        }

        private List<Activity> GetWinningEmojiActivities()
        {
            var winningEmojiId = OnlineSession.Current.EmojiResults
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .FirstOrDefault();

            var emoji = OnlineSession.Current.Emojis.FirstOrDefault(e => e.ID == winningEmojiId);
            return emoji?.Activities?.ToList() ?? new List<Activity>();
        }

        async void KeskeytäClicked(object sender, EventArgs e)
        {
            bool ok = await DisplayAlert("Haluatko varmasti sulkea huoneen?", "", "Kyllä", "Ei");
            if (!ok) return;

            await Navigation.PopToRootAsync();
        }

        async void JatkaClicked(object sender, EventArgs e)
        {
            try
            {
                var activities = GetWinningEmojiActivities();
                if (activities.Count == 0)
                {
                    await DisplayAlert("Virhe", "Aktiviteettivaihtoehtoja ei löytynyt.", "OK");
                    return;
                }

                OnlineSession.Current.ActivityCandidates = activities;
                OnlineSession.Current.ActivityResults = new Dictionary<string, int>();
                OnlineSession.Current.ActivityVoteOpen = true;

                await Main.GetInstance().Api.StartActivityVoteAsync(OnlineSession.Current.RoomId, activities);
                await Navigation.PushAsync(new TulostenOdotus());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JatkaClicked failed: {ex.Message}");
                await DisplayAlert("Virhe", "Aktiviteettiäänestyksen käynnistys epäonnistui.", "OK");
            }
        }
    }

    public class HostResultItem
    {
        public string Image { get; set; }
        public string Title { get; set; }
        public string Amount { get; set; }
        public double ScalePx { get; set; }
        public Color Color { get; set; }
    }
}