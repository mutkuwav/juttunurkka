
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Button = Microsoft.Maui.Controls.Button;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AktiviteettiäänestysEka : ContentPage
    {
        private CancellationTokenSource _cancellationTokenSource;
        private int _countSeconds = 30;
        private const double DefaultBorderWidth = 0;
        private const double SelectedBorderWidth = 6;
        private Button _selectedButton;
        private readonly QuestionToSpeech _questionToSpeechClient = new();

        public IList<CollectionItem> Items { get; set; }

        public class CollectionItem
        {
            public int ID;
            public string ImageSource { get; set; }
            public IList<Activity> ActivityChoises { get; set; }
            public string Selected { get; set; }

            public CollectionItem(int id, string imageSource, IList<Activity> activityChoises)
            {
                ID = id;
                ImageSource = imageSource;
                ActivityChoises = activityChoises;
                Selected = null;
            }
        }

        public AktiviteettiäänestysEka()
        {
            Microsoft.Maui.Controls.NavigationPage.SetHasBackButton(this, false);
            InitializeComponent();

            Items = new List<CollectionItem>
            {
                new CollectionItem(0, string.Empty, OnlineSession.Current.ActivityCandidates)
            };

            BindingContext = this;

            ActivityButton1.Clicked += OnButton1Clicked;
            ActivityButton2.Clicked += OnButton2Clicked;

            ConfigureButtons();
            Vote1();
        }

        private void ConfigureButtons()
        {
            var activities = OnlineSession.Current.ActivityCandidates;

            if (activities.Count > 0)
            {
                ActivityButton1.Text = activities[0].Title;
            }

            if (activities.Count > 1)
            {
                ActivityButton2.Text = activities[1].Title;
                ActivityButton2.IsVisible = true;
            }
            else
            {
                ActivityButton2.IsVisible = false;
            }
        }

        private async void OnButton1Clicked(object sender, EventArgs e)
        {
            SelectButton(ActivityButton1);

            if (OnlineSession.Current.ActivityCandidates.Count > 0)
            {
                var title = OnlineSession.Current.ActivityCandidates[0].Title;
                if (!string.IsNullOrEmpty(title))
                {
                    await _questionToSpeechClient.Speak(title);
                }
            }
        }

        private async void OnButton2Clicked(object sender, EventArgs e)
        {
            SelectButton(ActivityButton2);

            if (OnlineSession.Current.ActivityCandidates.Count > 1)
            {
                var title = OnlineSession.Current.ActivityCandidates[1].Title;
                if (!string.IsNullOrEmpty(title))
                {
                    await _questionToSpeechClient.Speak(title);
                }
            }
        }

        private void SelectButton(Button selectedButton)
        {
            if (_selectedButton != null)
            {
                _selectedButton.BorderWidth = DefaultBorderWidth;
            }

            _selectedButton = selectedButton;
            _selectedButton.BorderWidth = SelectedBorderWidth;
            SaveButton.IsEnabled = true;
        }

        private async void Vote1()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            double totalSeconds = _countSeconds;

            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (token.IsCancellationRequested)
                {
                    return false;
                }

                _countSeconds--;
                progressBar.Progress = Math.Max(0, _countSeconds / totalSeconds);

                return _countSeconds > 0;
            });

            try
            {
                await Task.Delay(_countSeconds * 1000, token);

                if (!token.IsCancellationRequested)
                {
                    await DisplayAlert("Aika loppui", "Et ehtinyt äänestää aktiviteettia.", "OK");
                    await Navigation.PushAsync(new ActivityAnswered(null, 0));
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Vote1 timer was canceled.");
            }
        }

        private async void SaveAnswer(object sender, EventArgs e)
        {
            if (_selectedButton == null)
            {
                Console.WriteLine("No activity selected");
                return;
            }

            _cancellationTokenSource?.Cancel();

            var answer = await SendActivityVote();
            await Navigation.PushAsync(new ActivityAnswered(answer, _countSeconds));
        }

        private async Task<Activity?> SendActivityVote()
        {
            Activity answer = null;

            if (_selectedButton == ActivityButton1 && OnlineSession.Current.ActivityCandidates.Count > 0)
            {
                answer = OnlineSession.Current.ActivityCandidates[0];
            }
            else if (_selectedButton == ActivityButton2 && OnlineSession.Current.ActivityCandidates.Count > 1)
            {
                answer = OnlineSession.Current.ActivityCandidates[1];
            }

            if (answer != null)
            {
                await Main.GetInstance().Api.SubmitActivityVoteAsync(
                    OnlineSession.Current.RoomId,
                    OnlineSession.Current.DeviceId,
                    answer.Title);
            }
            else
            {
                Console.WriteLine("No answer was given or something went wrong.");
            }

            return answer;
        }
    }
}