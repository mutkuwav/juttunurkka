/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Emmi Poutanen, Riina Kaipia

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
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific;
using Microsoft.Maui.Controls.Xaml;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OdotetaanVastauksiaOpe : ContentPage, INotifyPropertyChanged
    {
        private CancellationTokenSource cts;

        public string RoomCode => OnlineSession.Current.RoomCode;

        private int participantsCount;
        public int ParticipantsCount
        {
            get => participantsCount;
            set
            {
                if (participantsCount != value)
                {
                    participantsCount = value;
                    OnPropertyChanged(nameof(ParticipantsCount));
                    OnPropertyChanged(nameof(RespondentsDisplay));
                }
            }
        }

        private int respondentsCount;
        public int RespondentsCount
        {
            get => respondentsCount;
            set
            {
                if (respondentsCount != value)
                {
                    respondentsCount = value;
                    OnPropertyChanged(nameof(RespondentsCount));
                    OnPropertyChanged(nameof(RespondentsDisplay));
                }
            }
        }

        public string RespondentsDisplay => $"{RespondentsCount} / {ParticipantsCount}";

        public OdotetaanVastauksiaOpe()
        {
            InitializeComponent();
            Microsoft.Maui.Controls.NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = this;
            StartUpdatingCounts();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            cts = new CancellationTokenSource();
            var token = cts.Token;

            try
            {
                await UpdateProgressBar(0, 60000, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnAppearing failed: {ex.Message}");
            }
        }

        private async Task UpdateProgressBar(double progress, uint time, CancellationToken token)
        {
            var countdownTask = UpdateCountdownLabel(time, token);

            await progressBar.ProgressTo(progress, time, Easing.Linear);

            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            await countdownTask;

            if (progressBar.Progress == 0)
            {
                await FinishSurveyAndOpenResultsAsync();
            }
        }

        private async Task UpdateCountdownLabel(uint totalTimeMs, CancellationToken token)
        {
            int totalSeconds = (int)(totalTimeMs / 1000);

            for (int i = totalSeconds; i >= 0; i--)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    countdownLabel.Text = i + " s";
                });

                await Task.Delay(1000, token);
            }
        }

        private void StartUpdatingCounts()
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                _ = PollStatusOnceAsync();
                return !OnlineSession.Current.SurveyClosed;
            });
        }

        private async Task PollStatusOnceAsync()
        {
            try
            {
                var status = await Main.GetInstance().Api.GetRoomStatusAsync(OnlineSession.Current.RoomId);

                OnlineSession.Current.JoinedCount = status.JoinedCount;
                OnlineSession.Current.VotedCount = status.VotedCount;
                OnlineSession.Current.SurveyClosed = status.SurveyClosed;

                ParticipantsCount = status.JoinedCount;
                RespondentsCount = status.VotedCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PollStatusOnceAsync failed: {ex.Message}");
            }
        }

        private async Task FinishSurveyAndOpenResultsAsync()
        {
            try
            {
                await Main.GetInstance().Api.CloseSurveyAsync(OnlineSession.Current.RoomId);
                OnlineSession.Current.SurveyClosed = true;
                OnlineSession.Current.EmojiResults = await Main.GetInstance().Api.GetEmojiResultsAsync(OnlineSession.Current.RoomId);

                await Navigation.PushAsync(new LisätiedotHost());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FinishSurveyAndOpenResultsAsync failed: {ex.Message}");
                await DisplayAlert("Virhe", "Tulosten hakeminen epäonnistui.", "OK");
            }
        }

        private async void LopetaClicked(object sender, EventArgs e)
        {
            cts?.Cancel();
            await FinishSurveyAndOpenResultsAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
