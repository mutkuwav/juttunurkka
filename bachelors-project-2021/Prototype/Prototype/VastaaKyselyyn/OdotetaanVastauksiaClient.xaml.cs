
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen

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
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OdotetaanVastauksiaClient : ContentPage
    {
        private CancellationTokenSource cts;

        public OdotetaanVastauksiaClient()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            cts = new CancellationTokenSource();
            _ = WaitForResultsAsync(cts.Token);

            _ = Navigation.PushAsync(new LisätiedotClient());
        }

        protected override void OnDisappearing()
        {
            
            base.OnDisappearing();
            cts?.Cancel();
        }

        private async Task WaitForResultsAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var status = await Main.GetInstance().Api.GetRoomStatusAsync(OnlineSession.Current.RoomId);

                    OnlineSession.Current.JoinedCount = status.JoinedCount;
                    OnlineSession.Current.VotedCount = status.VotedCount;
                    OnlineSession.Current.SurveyClosed = status.SurveyClosed;

                    if (status.SurveyClosed)
                    {
                        OnlineSession.Current.EmojiResults =
                            await Main.GetInstance().Api.GetEmojiResultsAsync(OnlineSession.Current.RoomId);

                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            var top = Navigation.NavigationStack.LastOrDefault();
                            if (top is LisätiedotClient lis)
                            {
                                lis.ProcessEmojiResults();
                            }
                            else
                            {
                                _ = Navigation.PushAsync(new LisätiedotClient());
                            }
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
                Console.WriteLine($"WaitForResultsAsync failed: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Virhe", "Tulosten odottaminen epäonnistui.", "OK");
                    await Navigation.PopToRootAsync();
                });
            }
        }

        private async void Poistu(object sender, EventArgs e)
        {
            var res = await DisplayAlert("Oletko varma että tahdot poistua juttunurkasta?", "", "Kyllä", "Ei");

            if (res)
            {
                cts?.Cancel();
                await Navigation.PopToRootAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}