
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Emmi Poutanen, Joni Lapinkoski
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Prototype
{
    public partial class LisätiedotClient : ContentPage
    {
        private CancellationTokenSource cts;
        public ResultsViewModel ViewModel { get; set; }

        public LisätiedotClient()
        {
            InitializeComponent();
            Microsoft.Maui.Controls.NavigationPage.SetHasNavigationBar(this, false);
            ViewModel = new ResultsViewModel();
            BindingContext = ViewModel;

            ProcessEmojiResults();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            cts = new CancellationTokenSource();
            // Immediately try to fetch current emoji results so the user sees up-to-date counts
            _ = FetchInitialEmojiResultsAsync();

            // Start background tasks to keep activity and emoji results updated in real time
            _ = WaitForActivityVoteAsync(cts.Token);
            _ = WaitForEmojiUpdatesAsync(cts.Token);
        }

        protected override void OnDisappearing()
        {
            cts?.Cancel();
            base.OnDisappearing();
        }

        public void ProcessEmojiResults()
        {
            ViewModel.Results.Clear();

            var emojiResults = OnlineSession.Current?.EmojiResults?
                .OrderByDescending(kvp => kvp.Value)
                ?? Enumerable.Empty<KeyValuePair<int, int>>();

            int totalVotes = emojiResults.Sum(kvp => kvp.Value);

            foreach (var kvp in emojiResults)
            {
                var image = $"emoji{kvp.Key}lowres.png";
                var amount = kvp.Value;
                var height = totalVotes == 0
                    ? 0
                    : (int)((double)amount / totalVotes * 200);

                ViewModel.Results.Add(new ResultItem
                {
                    Image = image,
                    Amount = amount.ToString(),
                    Scale = height
                });
            }
        }

        private async Task FetchInitialEmojiResultsAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(OnlineSession.Current.RoomId))
                {
                    var results = await Main.GetInstance().Api.GetEmojiResultsAsync(OnlineSession.Current.RoomId);
                    OnlineSession.Current.EmojiResults = results;
                    await MainThread.InvokeOnMainThreadAsync(() => ProcessEmojiResults());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FetchInitialEmojiResultsAsync failed: {ex.Message}");
            }
        }

        private async Task WaitForEmojiUpdatesAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(OnlineSession.Current.RoomId))
                        {
                            var results = await Main.GetInstance().Api.GetEmojiResultsAsync(OnlineSession.Current.RoomId);
                            OnlineSession.Current.EmojiResults = results;

                            await MainThread.InvokeOnMainThreadAsync(() => ProcessEmojiResults());
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore individual fetch errors and continue polling
                        Console.WriteLine($"WaitForEmojiUpdatesAsync fetch error: {ex.Message}");
                    }

                    await Task.Delay(1000, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WaitForEmojiUpdatesAsync failed: {ex.Message}");
            }
        }

        async Task WaitForActivityVoteAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var response = await Main.GetInstance().Api.GetActivityCandidatesAsync(OnlineSession.Current.RoomId);

                    if (response.Activities.Count > 0)
                    {
                        OnlineSession.Current.ActivityCandidates = response.Activities
                            .Select(a => new Activity
                            {
                                Title = a.Title,
                                ImageSource = a.ImageSource
                            })
                            .ToList();

                        OnlineSession.Current.ActivityVoteOpen = response.VoteOpen;

                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Navigation.PushAsync(new AktiviteettiäänestysEka());
                        });

                        return;
                    }

                    await Task.Delay(1000, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WaitForActivityVoteAsync failed: {ex.Message}");
            }
        }

        async void PoistuClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
              "Oletko varma että tahdot poistua kyselystä?",
              "",
              "Kyllä",
              "Ei");

            if (answer)
            {
                cts?.Cancel();
                await Navigation.PopToRootAsync();
            }
        }
    }

    public class ResultsViewModel
    {
        public ObservableCollection<ResultItem> Results { get; }
            = new ObservableCollection<ResultItem>();
    }

    public class ResultItem
    {
        public string Image { get; set; }
        public string Amount { get; set; }
        public int Scale { get; set; }

        // XAML expects ScalePx binding for HeightRequest. Provide a computed property.
        public double ScalePx => Scale;
    }
}