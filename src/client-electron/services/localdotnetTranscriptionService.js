require('dotenv').config();
const axios = require('axios');

class LocalDotnetTranscriptionService {
    constructor() {
        this.endpointUri = process.env.TRANSCRIPTION_ENDPOINT_URI;
        if (!this.endpointUri) {
            throw new Error("TRANSCRIPTION_ENDPOINT_URI is not defined in environment variables.");
        }
    }
    async transcribeAudio(audioBuffer) {
        try {
            const response = await axios.post(this.endpointUri, audioBuffer, {
                headers: {
                    'Content-Type': 'audio/mp3',
                },
                responseType: 'json',
            });

            if (response.status === 200 && response.data) {
                return response.data.Transcription;
            } else {
                throw new Error("Failed to transcribe audio.");
            }
        } catch (error) {
            console.error("Error transcribing audio:", error);
            throw error;
        }
    }
}

module.exports = LocalDotnetTranscriptionService;