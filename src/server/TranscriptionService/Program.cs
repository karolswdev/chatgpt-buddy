using Google.Cloud.Speech.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAnyOrigin");

app.MapPost("/upload-audio", async (HttpRequest req) =>
{
    // Ensure the request has the correct Content-Type
    if (!req.ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest("Unsupported Media Type");
    }

    // Read the raw request body into a byte array
    using var ms = new MemoryStream();
    await req.Body.CopyToAsync(ms);
    byte[] audioBytes = ms.ToArray();

    try
    {
        var speechClient = SpeechClient.Create();
        var response = await speechClient.RecognizeAsync(new RecognitionConfig()
        {
            Encoding = RecognitionConfig.Types.AudioEncoding.Mp3, // Let Google auto-detect
            SampleRateHertz = 16000, // Let Google auto-detect
            LanguageCode = "en-US",
        }, RecognitionAudio.FromBytes(audioBytes));

        var transcription = string.Join("\n", response.Results.SelectMany(result => result.Alternatives.Select(alt => alt.Transcript)));
        return Results.Ok(new { Transcription = transcription });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: {ex.Message}");
        return Results.Problem("Error processing the audio file.");
    }
}).WithName("UploadAudio");

app.Run();