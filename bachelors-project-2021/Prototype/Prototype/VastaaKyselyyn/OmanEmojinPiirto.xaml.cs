using CommunityToolkit.Maui;
using Microsoft.Maui.Graphics;
using Prototype;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;

namespace Prototype;

public partial class OmanEmojinPiirto : ContentPage, IDrawable
{
    CancellationTokenSource cts;
    private readonly List<DrawnLine> lines = new();
    private DrawnLine? currentLine;

    private Color currentColor = Colors.Black;
    private float currentThickness = 4f;
    private bool isEraser = false;
    private readonly Color drawingAreaColor = Colors.Yellow;

    private byte[]? savedImageBytes;

    public OmanEmojinPiirto()
    {
        InitializeComponent();

        drawingView.Drawable = this;

        drawingView.StartInteraction += OnStartInteraction;
        drawingView.DragInteraction += OnDragInteraction;
        drawingView.EndInteraction += OnEndInteraction;

        colorPreview.Color = currentColor;
        thicknessSlider.Value = currentThickness;
    }


    private void OnStartInteraction(object? sender, TouchEventArgs e)
    {
        var points = e.Touches;
        if (points?.Length > 0)
        {
            var p = points[0];
            currentLine = new DrawnLine
            {
                Points = new List<PointF> { p },
                Color = isEraser ? drawingAreaColor : currentColor,
                Thickness = currentThickness
            };
            lines.Add(currentLine);
            drawingView.Invalidate();
        }
    }



    private void OnDragInteraction(object? sender, TouchEventArgs e)
    {
        if (currentLine == null || e.Touches == null || e.Touches.Length == 0) return;
        currentLine.Points.Add(e.Touches[0]);
        drawingView.Invalidate();
    }



    private void OnEndInteraction(object? sender, TouchEventArgs e)
    {
        currentLine = null;
    }


    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;
        float centerX = dirtyRect.Center.X;
        float centerY = dirtyRect.Center.Y;
        float radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f - 10f;

        canvas.FillColor = drawingAreaColor;
        canvas.FillCircle(centerX, centerY, radius);

        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 4f;
        canvas.DrawCircle(centerX, centerY, radius);

        PathF clipPath = new PathF();
        clipPath.AppendCircle(centerX, centerY, radius);
        canvas.ClipPath(clipPath);

        foreach (var line in lines)
        {
            if (line.Points.Count < 2) continue;

            canvas.StrokeColor = line.Color;
            canvas.StrokeSize = line.Thickness;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;

            for (int i = 1; i < line.Points.Count; i++)
            {
                var p1 = line.Points[i - 1];
                var p2 = line.Points[i];
                canvas.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
            }
        }
    }

    private void OnColorButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button b)
        {
            currentColor = b.BackgroundColor ?? Colors.Black;
            colorPreview.Color = currentColor;
            isEraser = false;
            eraserSwitch.IsToggled = false;
        }
    }

    private void OnEraserToggled(object sender, ToggledEventArgs e)
    {
        isEraser = e.Value;
        colorPreview.Color = isEraser ? drawingAreaColor : currentColor;
    }

    private void OnThicknessChanged(object sender, ValueChangedEventArgs e)
    {
        currentThickness = (float)e.NewValue;
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        lines.Clear();
        drawingView.Invalidate();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (lines.Count == 0 || currentLine != null)
        {
            await DisplayAlert("Ei mitään tallennettavaa", "Piirrä jotain!", "OK");
            return;
        }

        try
        {
            var screenshotResult = await drawingView.CaptureAsync();

            if (screenshotResult == null)
            {
                await DisplayAlert("Error", "Virhe kuvan tallennuksessa", "OK");
                return;
            }

            using var stream = new MemoryStream();
            await screenshotResult.CopyToAsync(stream);

            savedImageBytes = stream.ToArray();

            //lisätty, hoitaa piirroksen backend classiin EmojiUploadHandler.java
            await UploadEmojiAsync(savedImageBytes);

            await Main.GetInstance().Api.SubmitEmojiVoteAsync(
                OnlineSession.Current.RoomId, OnlineSession.Current.DeviceId, 7);

            await DisplayAlert("Tallennettu", "Emoji lähetetty opettajalle", "OK");

            await Navigation.PushAsync(new EmojiAnswered(7, savedImageBytes));

        }
        catch (Exception ex)
        {
            await DisplayAlert("Virhe tallennuksessa", ex.Message, "OK");
        }

    }

    //hoitaa oman emojin pilveen
    private async Task UploadEmojiAsync(byte[] imageBytes)
    {
        using var client = new HttpClient();

        // vaihda tähän "10.0.2.2:8080" local testausta vasrten
        string url = "http://86.50.20.47:8080/emoji-upload";

        using var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = content;

        
        var response = await client.SendAsync(request);

        request.Headers.Add("X-Room-Id", OnlineSession.Current.RoomId);
        request.Headers.Add("X-Device-Id", OnlineSession.Current.DeviceId);

        if (!response.IsSuccessStatusCode)
        {
            string errorText = await response.Content.ReadAsStringAsync();
            throw new Exception($"Palvelinvirhe: {response.StatusCode} - {errorText}");
        }
    }

    async protected override void OnAppearing()
    {
        cts = new CancellationTokenSource();
        var token = cts.Token;
        base.OnAppearing();

        try
        {
            // Increase this number in order to keep the view visible for longer time
            await UpdateProgressBar(0, 60000, token);
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine("Task cancelled", e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("ex {0}", e.Message);
        }
        finally
        {
            cts.Dispose();
        }

    }

    async Task UpdateProgressBar(double Progress, uint time, CancellationToken token)
    {

        await progressBar.ProgressTo(Progress, time, Easing.Linear);
        if (token.IsCancellationRequested)
        {
            token.ThrowIfCancellationRequested();
        }
        //siirtyy eteenpäin automaattisesti 60 sekunnin jälkeen
        if (progressBar.Progress == 0)
        {
            await Main.GetInstance().host.CloseSurvey();
            await Navigation.PushAsync(new LisätiedotHost());
        }

    }

    
    async protected override void OnDisappearing()
    {
        base.OnDisappearing();

    // varmistus, että piirustus tallentuu
        if (savedImageBytes == null && lines.Count > 0)
        {
            try
            {
                var screenshotResult = await drawingView.CaptureAsync();
                if (screenshotResult != null)
                {
                    using var stream = new MemoryStream();
                    await screenshotResult.CopyToAsync(stream);
                    savedImageBytes = stream.ToArray();

                    await Navigation.PushAsync(new EmojiAnswered(7, savedImageBytes));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to auto-save drawing: " + ex.Message);
            }
        }
    }
    
    
    /*
    public byte[]? getDrawingAsImage()
    {
        if (savedImageBytes != null)
        {
            return savedImageBytes;
        }
        throw new InvalidOperationException("No drawing has been saved yet.");
    }
    */

    private class DrawnLine
    {
        public List<PointF> Points { get; set; } = new();
        public Color Color { get; set; } = Colors.Black;
        public float Thickness { get; set; } = 4f;
    }
}