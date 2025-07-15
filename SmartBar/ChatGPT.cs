using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;

public class ChatGPT
{
    private readonly string _apiKey = "api_key";
    private readonly string _endpoint = "https://api.openai.com/v1/chat/completions";

    public async Task<string> AskChatGPT(string message)
    {
        var client = new RestClient(_endpoint);
        var request = new RestRequest("", Method.Post);

        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Content-Type", "application/json");

        var body = new
        {
            model = "gpt-3.5-turbo",
            messages = new[] { new { role = "user", content = message } }
        };

        request.AddStringBody(JsonConvert.SerializeObject(body), DataFormat.Json);

        try
        {
            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                return $"HTTP Error: {response.StatusCode} - {response.ErrorMessage}";

            dynamic json = JsonConvert.DeserializeObject(response.Content);
            return json.choices[0].message.content.ToString();
        }
        catch (Exception ex)
        {
            return $"API Error: {ex.Message}";
        }
    }
}