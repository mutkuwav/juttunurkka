
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
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Prototype
{
    public class VoteResultViewModel
    {
        public string Image { get; set; } = "";
        public string Title { get; set; } = "";
        public int Amount { get; set; }
        public double Scale { get; set; }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AktiviteettiäänestysTulokset : ContentPage
    {
        public ObservableCollection<VoteResultViewModel> Results { get; set; }

        public AktiviteettiäänestysTulokset()
        {
            InitializeComponent();
            Microsoft.Maui.Controls.NavigationPage.SetHasNavigationBar(this, false);

            Results = new ObservableCollection<VoteResultViewModel>();

            var voteResults = OnlineSession.Current.ActivityResults;
            int totalAnswers = voteResults.Values.Sum();
            int maxVotes = voteResults.Count != 0 ? voteResults.Values.Max() : 1;

            foreach (var kvp in voteResults.OrderByDescending(kvp => kvp.Value))
            {
                var matchingActivity = OnlineSession.Current.ActivityCandidates
                    .FirstOrDefault(a => a.Title == kvp.Key);

                Results.Add(new VoteResultViewModel
                {
                    Image = matchingActivity?.ImageSource ?? "",
                    Title = kvp.Key,
                    Amount = kvp.Value,
                    Scale = (kvp.Value / (double)maxVotes) * 200
                });
            }

            BindingContext = this;
        }

        async void SuljeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new JuttunurkkaSuljettu());
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (await DisplayAlert("Poistutaanko tulosten tarkastelusta ? ", "", "Kyllä", "Ei"))
                {
                    base.OnBackButtonPressed();
                    await Navigation.PushAsync(new MainPage());
                }
            });

            return true;
        }
    }
}