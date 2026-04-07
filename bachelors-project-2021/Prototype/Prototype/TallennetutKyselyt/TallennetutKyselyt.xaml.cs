
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TallennetutKyselyt : ContentPage
    {

        public enum ToolbarItemPosition { Start, End }

        private bool _deleteMode = false;
        private List<string> _selectedForDeletion = new();

        public string SelectedSurvey { get; set; }
        public List<string> Surveys { get; set; }
        public TallennetutKyselyt()
        {
            InitializeComponent();
            Surveys = new List<String>();
            SurveyManager manager = SurveyManager.GetInstance();

            NavigationPage.SetHasBackButton(this, false);


            Surveys = manager.GetTemplates();



            BindingContext = this;
        }

        //Device back button navigation 
        protected override bool OnBackButtonPressed()
        {
            Navigation.PushAsync(new MainPage());
            return true;

        }


        void OnListSelection(object sender, SelectionChangedEventArgs e)
        {
            if (_deleteMode)
            {
                // CurrentSelection contains ALL currently-selected items (multi-select)
                _selectedForDeletion = e.CurrentSelection
                                          .Cast<string>()
                                          .ToList();
                return;
            }

            // --- NORMAL MODE (single selection) ---
            SelectedSurvey = e.CurrentSelection.FirstOrDefault() as string;
            TButton.IsEnabled = SelectedSurvey != null;
        }

        async void OletusClicked(object sender, EventArgs e)
        {
            SurveyManager.GetInstance().SetDefaultSurvey();

            KyselynTarkastelu.canDelete = false;
            KyselynTarkastelu.canEdit = false;
            await Navigation.PushAsync(new KyselynTarkastelu());
        }

        async void AvaaClicked(object sender, EventArgs e)
        {
            string surveyName = SelectedSurvey + ".txt";
            SurveyManager manager = SurveyManager.GetInstance();
            manager.LoadSurvey(surveyName);

            Console.WriteLine(surveyName);

            KyselynTarkastelu.canDelete = true;
            KyselynTarkastelu.canEdit = true;
            await Navigation.PushAsync(new KyselynTarkastelu());

        }


        async void DeleteClicked(object sender, EventArgs e)
        {
            if (!_deleteMode)
            {
                // first click -> enter delete mode
                // second clicl -> delete selected items
                _deleteMode = true;
                DeleteButton.Text = "Poista valitut";
                TButton.IsEnabled = false;

                SurveyList.SelectionMode = SelectionMode.Multiple;
                _selectedForDeletion.Clear();

                return;
            }

            if (_selectedForDeletion.Count == 0)
            {
                await DisplayAlert("Ei valintaa", "Valitse ainakin yksi poistettava kysely.", "OK");
                return;
            }

            var manager = SurveyManager.GetInstance();
            foreach (var s in _selectedForDeletion)
            {
                manager.DeleteSurvey(s + ".txt");
                Surveys.Remove(s);
            }

            // refresh UI
            Surveys = manager.GetTemplates();
            BindingContext = null;
            BindingContext = this;

            // exit delete mode
            _deleteMode = false;
            DeleteButton.Text = "Poista kyselyitä";
            SurveyList.SelectionMode = SelectionMode.Single;
        }



        async void BackBtnClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}