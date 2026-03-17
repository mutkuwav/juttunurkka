
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Prototype;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmojinValinta : ContentPage
    {
        private CancellationTokenSource cts;
        private readonly QuestionToSpeech _questionToSpeechClient = new();
        public string introMessage { get; set; }
        private int answer = -1;
        private ImageButton _lastClickedEmoji;

        public IList<CollectionItem> Emojit { get; private set; }

        public class CollectionItem
        {
            public Emoji Item { get; set; }
            public string Selected { get; set; }
        }

        public EmojinValinta()
        {
            InitializeComponent();
            Microsoft.Maui.Controls.NavigationPage.SetHasBackButton(this, false);

            Emojit = OnlineSession.Current.Emojis
                .Select(e => new CollectionItem { Item = e, Selected = null })
                .ToList();

            introMessage = OnlineSession.Current.IntroMessage;

            BindingContext = this;
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
                Console.WriteLine($"EmojinValinta OnAppearing failed: {ex.Message}");
            }
        }

        private async Task UpdateProgressBar(double progress, uint time, CancellationToken token)
        {
            await progressBar.ProgressTo(progress, time, Easing.Linear);

            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            if (progressBar.Progress == 0)
            {
                await DisplayAlert("Aika loppui", "Et ehtinyt vastata kyselyyn.", "OK");
                await Navigation.PopToRootAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var emoji = sender as ImageButton;
            if (emoji == null) return;

            if (_lastClickedEmoji != null && _lastClickedEmoji != emoji)
            {
                _lastClickedEmoji.Scale = 1.0;
            }

            answer = int.Parse(emoji.ClassId);
            emoji.Scale = 1.75;
            _lastClickedEmoji = emoji;
            Vastaus.IsEnabled = true;
        }

        private async void QuestionToSpeech_Clicked(object sender, EventArgs e)
        {
            await _questionToSpeechClient.Speak(introMessage);
        }

        private async void Vastaa_Clicked(object sender, EventArgs e)
        {
            cts.Cancel(); //cancel task if button clicked

            await Main.GetInstance().client.SendResult(answer.ToString());
            
            if (answer == 7) { 
                await Navigation.PushAsync(new OmanEmojinPiirto());
            } else { 
                await Navigation.PushAsync(new EmojiAnswered(answer)); 
            }

        }
            if (answer < 0)
            {
                await DisplayAlert("Huom", "Valitse emoji ennen vastaamista.", "OK");
                return;
            }

            try
            {
                cts?.Cancel();

                await Main.GetInstance().Api.SubmitEmojiVoteAsync(
                    OnlineSession.Current.RoomId,
                    OnlineSession.Current.DeviceId,
                    answer);

                await Navigation.PushAsync(new EmojiAnswered(answer));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Submit vote failed: {ex.Message}");
                await DisplayAlert("Virhe", "Vastauksen lähetys epäonnistui.", "OK");
            }
        }
    }
}