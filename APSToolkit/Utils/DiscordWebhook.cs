// Copyright (c) chuongmep.com. All rights reserved

using RestSharp;

namespace APSToolkit.Utils;

public class DiscordWebhook
{
    private string WebHookUrl { get; set; } =
        "https://discordapp.com/api/webhooks/1182501348734943272/2xI-zV7JGQjatZgBC8w1-SKTZlHxg5-hnvDfza8qi5OK-842K2RuZRtbAE2A7zH5VCld";
    public string username { get; set; }
    public string avatar_url { get; set; }
    public string content { get; set; }

    public void SendReport(string title, string content)
    {
        string url = WebHookUrl;
        var client = new RestClient(url);
        var request = new RestRequest() {Method = Method.Post};
        DiscordWebhook discordWebhook = new DiscordWebhook()
        {
            content = content,
            username = title
        };
        // add raw content json with {content: "{content}"}'
        request.AddJsonBody(discordWebhook);
        client.ExecuteAsync(request);
        Console.WriteLine("Done report to discord");
    }
}