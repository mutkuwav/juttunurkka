using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OdotetaanKyselynAloitustaClient : ContentPage
    {
        private CancellationTokenSource cts;

        public OdotetaanKyselynAloitustaClient()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            cts = new CancellationTokenSource();
            _ = PollSurveyStartAsync(cts.Token);
        }

        protected override void OnDisappearing()
        {
            cts?.Cancel();
            base.OnDisappearing();
        }

        private async Task PollSurveyStartAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var result = await Main.GetInstance().Api.GetSurveyStateAsync(OnlineSession.Current.RoomId);

                    if (result.SurveyReady)
                    {
                        OnlineSession.Current.IntroMessage = result.IntroMessage;
                        OnlineSession.Current.Emojis = result.Emojis
                            .Select(e => new Emoji
                            {
                                ID = e.Id,
                                Name = e.Name,
                                ImageSource = $"emoji{e.Id}lowres.png"
                            })
                            .ToList();

                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Navigation.PushAsync(new EmojinValinta());
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
                Console.WriteLine($"PollSurveyStartAsync failed: {ex.Message}");
            }
        }

        async void PoistuClicked(object sender, EventArgs e)
        {
            cts?.Cancel();
            await Navigation.PopToRootAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}