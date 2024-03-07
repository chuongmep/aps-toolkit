// Copyright (c) chuongmep.com. All rights reserved

using RestSharp;

namespace APSToolkit.Utils;

public class DiscordWebhook
{
    public string username { get; set; }
    public string avatar_url { get; set; }
    public string content { get; set; }

    public void SendReport(string webHookUrl,string title, string content)
    {
        var client = new RestClient(webHookUrl);
        var request = new RestRequest() {Method = Method.Post};
        DiscordWebhook discordWebhook = new DiscordWebhook()
        {
            content = content,
            username = title
        };
        // add raw content json with {content: "{content}"}'
        request.AddJsonBody(discordWebhook);
        client.ExecuteAsync(request);
    }
}