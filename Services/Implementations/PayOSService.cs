using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StudentFreelance.Models.PayOS;

public class PayOSService : IPayOSService
{
    private readonly PayOSConfig _config;
    private readonly HttpClient _client;

    public PayOSService(IOptions<PayOSConfig> config)
    {

        _config = config.Value;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("x-client-id", _config.ClientId);
        _client.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);
    }

    //public async Task<string> CreatePaymentLink(decimal amount, string description, string orderCode)
    //{
    //    var body = new
    //    {
    //        amount = (int)(amount * 1000),
    //        description,
    //        orderCode,
    //        returnUrl = _config.ReturnUrl,
    //        cancelUrl = _config.CancelUrl
    //    };

    //    var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    //    var response = await _client.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content);

    //    var json = await response.Content.ReadAsStringAsync();
    //    Console.WriteLine("PayOS RESPONSE: " + json); // để debug khi cần

    //    if (response.IsSuccessStatusCode)
    //    {
    //        using var doc = JsonDocument.Parse(json);

    //        if (doc.RootElement.TryGetProperty("data", out var dataElement) &&
    //            dataElement.TryGetProperty("checkoutUrl", out var checkoutProp))
    //        {
    //            return checkoutProp.GetString()!;
    //        }

    //        throw new Exception("checkoutUrl not found in PayOS response.");
    //    }

    //    throw new Exception("Failed to create payment link: " + json);
    //}
    public async Task<string> CreatePaymentLink(decimal amount, string description, string orderCode)
    {
        var body = new
        {
            amount = (int)(amount * 1000),
            description,
            orderCode,
            returnUrl = _config.ReturnUrl,
            cancelUrl = _config.CancelUrl
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content);

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine("💡 PayOS raw response:\n" + json);  // Rất quan trọng để debug

        if (response.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("data", out JsonElement dataElement) &&
                dataElement.ValueKind == JsonValueKind.Object &&
                dataElement.TryGetProperty("checkoutUrl", out JsonElement checkoutProp))
            {
                return checkoutProp.GetString()!;
            }

            throw new Exception("❌ 'data.checkoutUrl' not found in PayOS response.");
        }

        throw new Exception($"❌ Failed to create payment link: {json}");
    }



    public async Task<bool> TransferToBankAsync(string bankCode, string accountNumber, decimal amount, string description)
    {
        var body = new
        {
            amount = (int)(amount * 1000),
            accountNumber,
            bankCode,
            description
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("https://api-merchant.payos.vn/bank/transfer", content);

        return response.IsSuccessStatusCode;
    }
}
