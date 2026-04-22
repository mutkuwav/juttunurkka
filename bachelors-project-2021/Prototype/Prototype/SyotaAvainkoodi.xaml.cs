/*
Copyright 2025 Petri Pentinpuro

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
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SyotaAvainkoodi : ContentPage
    {
        private bool isProcessing = false; // Estetään kaksoistuotos

        public SyotaAvainkoodi()
        {
            InitializeComponent();

            // Lukko-kuvan napautustapahtuman määrittäminen
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += LukkoImage_Tapped;
            LukkoImage.GestureRecognizers.Add(tapGestureRecognizer);
        }

        private async void LukkoImage_Tapped(object sender, EventArgs e)
        {
            if (isProcessing) return;
            isProcessing = true;

            if (entry != null && !string.IsNullOrEmpty(entry.Text) && await Main.GetInstance().JoinSurvey(entry.Text))
            {
                var oikeaAvainkoodiPage = new AvainkoodiOikein();
                await Navigation.PushAsync(oikeaAvainkoodiPage);

                await Task.Delay(2000);
                await Navigation.PopAsync();

                if (string.IsNullOrWhiteSpace(OnlineSession.Current.IntroMessage) || OnlineSession.Current.Emojis.Count == 0)
                {
                    await Navigation.PushAsync(new OdotetaanKyselynAloitustaClient());
                }
                else
                {
                    await Navigation.PushAsync(new EmojinValinta());
                }

                entry.Text = null;
            }
            else
            {
                await DisplayAlert("Virheellinen avainkoodi", "Syöttämälläsi avainkoodilla ei löydy avointa juttunurkkaa", "OK");
                entry.Text = null; // Tyhjennetään kenttä virheen jälkeen
            }

            isProcessing = false;
        }

        //Device back button disabled
        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        async void AlkuunClicked(object sender, EventArgs e)
        {
            //palataan etusivulle
            Console.WriteLine("Alkuun clicked");
            await Navigation.PopToRootAsync();
        }
    }
}
