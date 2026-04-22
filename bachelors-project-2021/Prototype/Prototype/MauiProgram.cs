using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Prototype;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseSharedMauiApp();
        return builder.Build();
    }
}