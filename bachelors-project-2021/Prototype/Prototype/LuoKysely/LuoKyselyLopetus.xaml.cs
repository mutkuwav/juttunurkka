
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2026 Aurora Kansanoja, Matias Meriläinen

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

/* In this class, the user gives the juttunurkka a name and the juttunurkka key code is generated. */

using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LuoKyselyLopetus : ContentPage
    {
        public string introMessage { get; set; } = "Kysymys: ";

        public LuoKyselyLopetus()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            Survey s = SurveyManager.GetInstance().GetSurvey();
            introMessage += s.introMessage;
            BindingContext = this;

            NameEditor.Text = SurveyManager.GetInstance().GetSurvey().Name;
        }

        async void EdellinenButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        async void JatkaButtonClicked(object sender, EventArgs e)
        {
            if (NameEditor != null && !string.IsNullOrEmpty(NameEditor.Text))
            {
                SurveyManager man = SurveyManager.GetInstance();

                // save only survey name here
                man.GetSurvey().Name = NameEditor.Text;

                // save survey
                man.SaveSurvey(NameEditor.Text + ".txt");

                await Navigation.PushAsync(new Esikatselu());
            }
            else
            {
                await DisplayAlert("Nimi puuttuu", "Sinun on asetettava kyselylle nimi", "OK");
            }
        }

        private static void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                bool isValid = args.NewTextValue.ToCharArray().All(x => char.IsLetterOrDigit(x));
                ((Entry)sender).Text = isValid
                    ? args.NewTextValue
                    : args.NewTextValue.Remove(args.NewTextValue.Length - 1);
            }
        }
    }
}