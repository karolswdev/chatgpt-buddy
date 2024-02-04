using Google.Cloud.Speech.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/upload-audio", async ([FromBody] byte[] audioBytes) =>
    {
        var speechClient = SpeechClient.Create();
        var response = await speechClient.RecognizeAsync(new RecognitionConfig()
        {
            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
            SampleRateHertz = 16000,
            LanguageCode = "en",
        }, RecognitionAudio.FromBytes(audioBytes));

        var transcription = string.Join("\n", response.Results.SelectMany(result => result.Alternatives.Select(alt => alt.Transcript)));
    
        return Results.Ok(new { Transcription = transcription });
    })
    .WithName("UploadAudio");

app.UseHttpsRedirection();

app.Run();