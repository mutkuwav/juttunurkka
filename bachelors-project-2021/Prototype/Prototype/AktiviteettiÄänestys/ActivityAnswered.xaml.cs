
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityAnswered : ContentPage
    {
        public Activity ActivityToShow { get; set; }

        private readonly Activity? _votedActivity;
        private readonly int _remainingTime;
        private bool _voteResultReceived = false;

        public ActivityAnswered(Activity? votedActivity, int remainingTime)
        {
            _votedActivity = votedActivity;
            _remainingTime = remainingTime <= 0 ? 30 : remainingTime;

            Microsoft.Maui.Controls.NavigationPage.SetHasBackButton(this, false);
            ActivityToShow = votedActivity ?? new Activity();
            InitializeComponent();
            BindingContext = this;

            StartCountdownAndListen();
        }

        private async void StartCountdownAndListen()
        {
            int totalSeconds = _remainingTime;
            int elapsed = 0;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var listenTask = ListenForResultsAsync(cancellationTokenSource.Token);

            while (elapsed < totalSeconds)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                double progress = 1.0 - (double)elapsed / totalSeconds;
                progressBar.Progress = progress;

                await Task.Delay(1000);
                elapsed++;
            }

            progressBar.Progress = 0;
            await Task.Delay(1000);

            cancellationTokenSource.Cancel();

            if (!_voteResultReceived)
            {
                await DisplayAlert("VIRHE", "Tulosten haku epäonnistui", "OK");
                await Navigation.PushAsync(new MainPage());
            }
        }

        private async Task ListenForResultsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var candidateState = await Main.GetInstance().Api.GetActivityCandidatesAsync(OnlineSession.Current.RoomId);

                    if (!candidateState.VoteOpen)
                    {
                        OnlineSession.Current.ActivityVoteOpen = false;
                        OnlineSession.Current.ActivityResults =
                            await Main.GetInstance().Api.GetActivityResultsAsync(OnlineSession.Current.RoomId);

                        _voteResultReceived = true;
                        await Navigation.PushAsync(new AktiviteettiäänestysTulokset());
                        break;
                    }

                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while receiving results: {ex.Message}");
            }
        }
    }
}