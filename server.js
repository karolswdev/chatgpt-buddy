const express = require('express');
const multer = require('multer');
const speech = require('@google-cloud/speech');
const app = express();
const port = 3000;

// Initialize Google Cloud Speech client
const speechClient = new speech.SpeechClient();

// Configure multer for memory storage
const storage = multer.memoryStorage();
const upload = multer({ storage: storage });

app.post('/upload-audio', upload.single('audio'), async (req, res) => {
    if (!req.file) {
        return res.status(400).send('No file uploaded.');
    }

    // Convert the audio file buffer to a base64 string
    const audioBytes = req.file.buffer.toString('base64');

    // Configure the request for Google Cloud Speech-to-Text
    const audio = { content: audioBytes };
    const config = {
        encoding: 'LINEAR16', // Adjust according to your audio file's encoding
        sampleRateHertz: 16000, // Adjust according to your audio file's sample rate
        languageCode: 'en-US', // Adjust according to your audio file's language
    };
    const request = { audio: audio, config: config };

    try {
        // Send the audio file to Google Cloud Speech-to-Text for transcription
        const [response] = await speechClient.recognize(request);
        const transcription = response.results
            .map(result => result.alternatives[0].transcript)
            .join('\n');
        console.log(`Transcription: ${transcription}`);
        res.send({ transcription: transcription });
    } catch (err) {
        console.error('ERROR:', err);
        res.status(500).send('Error processing audio file.');
    }
});

app.listen(port, () => {
    console.log(`Server running on port ${port}`);
});