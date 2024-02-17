const axios = require('axios');
const GPTService = require('./gptService');

class GPTServiceAxios extends GPTService {
    constructor(apiKey) {
        super();
        this.apiKey = apiKey;
    }

    async queryModel(prompt) {
        try {
            const response = await axios.post('https://api.openai.com/v1/completions', {
                model: "text-davinci-003", // Change accordingly if using a different model
                prompt: prompt,
                temperature: 0.7,
                max_tokens: 150,
            }, {
                headers: {
                    'Authorization': `Bearer ${this.apiKey}`
                }
            });
            return response.data.choices[0].text.trim();
        } catch (error) {
            console.error("Error querying GPT model:", error);
            throw error;
        }
    }
}

module.exports = GPTServiceAxios;
