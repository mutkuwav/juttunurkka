
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Emmi Poutanen
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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TulostenOdotus : ContentPage, INotifyPropertyChanged
    {
        public string RoomCode { get; set; }
        public int ParticipantsCount { get; set; }

        private int _answerCount;
        public int AnswerCount
        {
            get => _answerCount;
            set
            {
                if (_answerCount != value)
                {
                    _answerCount = value;
                    OnPropertyChanged(nameof(AnswerCount));
                }
            }
        }

        private int _countSeconds = 35;
        private int _timeLeft = 35;
        public int TimeLeft
        {
            get => _timeLeft;
            set
            {
                if (_timeLeft != value)
                {
                    _timeLeft = value;
                    OnPropertyChanged(nameof(TimeLeft));
                }
            }
        }

        public TulostenOdotus()
        {
            InitializeComponent();

            RoomCode = OnlineSession.Current.RoomCode;
            ParticipantsCount = OnlineSession.Current.JoinedCount;
            AnswerCount = 0;
            BindingContext = this;

            Microsoft.Maui.Controls.NavigationPage.SetHasNavigationBar(this, false);

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                _countSeconds--;
                TimeLeft--;

                _ = RefreshCountsAsync();

                if (_countSeconds <= 0)
                {
                    return false;
                }

                return true;
            });
        }

        async protected override void OnAppearing()
        {
            base.OnAppearing();
            await UpdateProgressBar(0, 35000);
        }

        private async Task RefreshCountsAsync()
        {
            try
            {
                var roomStatus = await Main.GetInstance().Api.GetRoomStatusAsync(OnlineSession.Current.RoomId);
                ParticipantsCount = roomStatus.JoinedCount;
                OnPropertyChanged(nameof(ParticipantsCount));

                var activityResults = await Main.GetInstance().Api.GetActivityResultsAsync(OnlineSession.Current.RoomId);
                AnswerCount = activityResults.Values.Sum();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RefreshCountsAsync failed: {ex.Message}");
            }
        }

        async Task UpdateProgressBar(double progress, uint time)
        {
            await progressBar.ProgressTo(progress, time, Easing.Linear);

            if (progressBar.Progress == 0)
            {
                await CloseVoteAndShowResults();
            }
        }

        private async Task CloseVoteAndShowResults()
        {
            try
            {
                await Main.GetInstance().Api.CloseActivityVoteAsync(OnlineSession.Current.RoomId);
                OnlineSession.Current.ActivityVoteOpen = false;
                OnlineSession.Current.ActivityResults =
                    await Main.GetInstance().Api.GetActivityResultsAsync(OnlineSession.Current.RoomId);

                await Navigation.PushAsync(new AktiviteettiäänestysTulokset());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CloseVoteAndShowResults failed: {ex.Message}");
                await DisplayAlert("Virhe", "Tulosten haku epäonnistui.", "OK");
            }
        }

        async void GoToResultsClicked(object sender, EventArgs e)
        {
            await CloseVoteAndShowResults();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
