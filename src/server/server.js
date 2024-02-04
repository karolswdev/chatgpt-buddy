const express = require('express');
const multer = require('multer');
const speech = require('@google-cloud/speech');
const app = express();
const cors = require('cors');
const port = 3000;
const bodyParser = require('body-parser');

// Initialize Google Cloud Speech client
const speechClient = new speech.SpeechClient();

// Configure multer for memory storage
const storage = multer.memoryStorage();
const upload = multer({ storage: storage });

app.use(cors());
app.use(bodyParser.raw({ type: 'audio/mp4', limit: '50mb' }));

app.post('/upload-audio', upload.single('audio'), async (req, res) => {
    console.log("request received");
    console.log(req.body);
    if (!req.body) {
        console.log("no bad request");
        return res.status(400).send('No file uploaded.');
    }

    const audioData = req.body;

    console.log({ audioData });

    // Convert the audio file buffer to a base64 string
    console.log("converting audio bytes to base64");
    const audioBytes = audioData.toString('base64');
    console.log(audioBytes);

    return res.status(201).send("all good");

    // // Configure the request for Google Cloud Speech-to-Text
    // const audio = { content: audioBytes };
    // const config = {
    //     encoding: 'LINEAR16', // Adjust according to your audio file's encoding
    //     sampleRateHertz: 16000, // Adjust according to your audio file's sample rate
    //     languageCode: 'en-US', // Adjust according to your audio file's language
    // };
    // const request = { audio: audio, config: config };

    // try {
    //     // Send the audio file to Google Cloud Speech-to-Text for transcription
    //     const [response] = await speechClient.recognize(request);
    //     const transcription = response.results
    //         .map(result => result.alternatives[0].transcript)
    //         .join('\n');
    //     console.log(`Transcription: ${transcription}`);
    //     res.send({ transcription: transcription });
    // } catch (err) {
    //     console.error('ERROR:', err);
    //     res.status(500).send('Error processing audio file.');
    // }
});

app.listen(port, () => {
    console.log(`Server running on port ${port}`);
});