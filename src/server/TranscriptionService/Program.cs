using Google.Cloud.Speech.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/upload-audio", async (HttpRequest req) =>
    {
        // Ensure the request has the correct Content-Type
        if (!req.ContentType.Equals("audio/mp4", StringComparison.OrdinalIgnoreCase))
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
                // Note: You might need to adjust these settings based on your audio file
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16, // Since encoding is not explicitly set, let Google auto-detect
                SampleRateHertz = 16000, // Set to 0 to let Google auto-detect
                LanguageCode = "en-US",
            }, RecognitionAudio.FromBytes(audioBytes));

            var transcription = string.Join("\n", response.Results.SelectMany(result => result.Alternatives.Select(alt => alt.Transcript)));

            return Results.Ok(new { Transcription = transcription });
        }
        catch (Exception ex)
        {
            // Log the exception details for debugging
            Console.WriteLine($"ERROR: {ex.Message}");
            return Results.Problem("Error processing the audio file.");
        }
    })
    .WithName("UploadAudio");

//app.UseHttpsRedirection();
app.UseCors("AllowAnyOrigin");

app.Run();