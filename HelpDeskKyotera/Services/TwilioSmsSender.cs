using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HelpDeskKyotera.Services;

public class TwilioSmsSender : ISmsSender
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsSender> _logger;

    public TwilioSmsSender(IHttpClientFactory clientFactory, IConfiguration configuration, ILogger<TwilioSmsSender> logger)
    {
        _clientFactory = clientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendSmsAsync(string to, string message)
    {
        var accountSid = _configuration["Twilio:AccountSid"];
        var authToken = _configuration["Twilio:AuthToken"];
        var from = _configuration["Twilio:FromNumber"];

        if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(from))
        {
            _logger.LogWarning("Twilio is not configured. Skipping SMS send to {To}", to);
            return;
        }

        var client = _clientFactory.CreateClient();
        var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";

        var form = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("To", to),
            new KeyValuePair<string, string>("From", from),
            new KeyValuePair<string, string>("Body", message)
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(form)
        };

        var authBytes = Encoding.UTF8.GetBytes($"{accountSid}:{authToken}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

        var resp = await client.SendAsync(request);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            _logger.LogError("Twilio API error ({Status}): {Body}", resp.StatusCode, body);
            throw new InvalidOperationException($"Twilio API returned {resp.StatusCode}");
        }
    }
}
