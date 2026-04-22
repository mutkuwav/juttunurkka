
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

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OdotetaanOsallistujiaOpettaja : ContentPage, System.ComponentModel.INotifyPropertyChanged
    {
        private string roomCode = "Luodaan juttunurkkaa...";
        public string RoomCode
        {
            get => roomCode;
            set
            {
                if (roomCode != value)
                {
                    roomCode = value;
                    OnPropertyChanged(nameof(RoomCode));
                }
            }
        }

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
                }
            }
        }

        private bool pollingStarted = false;

        public OdotetaanOsallistujiaOpettaja()
        {
            InitializeComponent();
            Microsoft.Maui.Controls.NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = this;

            _ = InitializeRoomAsync();
        }

        private async Task InitializeRoomAsync()
        {
            var ok = await Main.GetInstance().HostSurvey();
            if (!ok)
            {
                await DisplayAlert("Virhe", "Juttunurkan luominen epäonnistui.", "OK");
                await Navigation.PopToRootAsync();
                return;
            }

            RoomCode = "Avainkoodi: " + OnlineSession.Current.RoomCode;

            if (!pollingStarted)
            {
                pollingStarted = true;
                StartPollingStatus();
            }
        }

        private void StartPollingStatus()
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Status polling failed: {ex.Message}");
            }
        }

        private async void AloitaButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var survey = SurveyManager.GetInstance().GetSurvey();

                await Main.GetInstance().Api.PublishSurveyAsync(
                    OnlineSession.Current.RoomId,
                    survey);

                await Navigation.PushAsync(new OdotetaanVastauksiaOpe());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Survey publish failed: {ex.Message}");
                await DisplayAlert("Virhe", "Kyselyn aloitus epäonnistui.", "OK");
            }
        }

        private async void KeskeytaButtonClicked(object sender, EventArgs e)
        {
            var res = await DisplayAlert("Oletko varma että tahdot keskeyttää juttunurkan?", "", "Kyllä", "Ei");
            if (!res) return;

            try
            {
                if (!string.IsNullOrWhiteSpace(OnlineSession.Current.RoomId))
                {
                    await Main.GetInstance().Api.CloseSurveyAsync(OnlineSession.Current.RoomId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Close survey failed: {ex.Message}");
            }

            await Navigation.PopToRootAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}