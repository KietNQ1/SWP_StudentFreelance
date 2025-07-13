using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.ViewModels;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentFreelance.Models.PayOS;
using System.Net.Http;

public class PayOSService : IPayOSService
{
    private readonly PayOSConfig _config;
    private readonly HttpClient _client;
    private readonly ILogger<PayOSService> _logger;

    public PayOSService(IOptions<PayOSConfig> config, HttpClient httpClient, ILogger<PayOSService> logger)
    {
        _config = config.Value;
        _client = httpClient;
        _logger = logger;

        _client.DefaultRequestHeaders.Add("x-client-id", _config.ClientId);
        _client.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);
    }

    public async Task<string> CreatePaymentLink(decimal amount, long orderCode, string description, string returnUrl, string cancelUrl)
    {
        var intAmount = (int)amount;

        // ✅ Bảo vệ: mô tả không chứa ký tự phân tách `|`
        description = description.Replace("|", "/");
        _logger.LogInformation("🧪 Desc bytes: {Bytes}", Convert.ToHexString(Encoding.UTF8.GetBytes(description)));

        // ✅ Tạo Dictionary dữ liệu và sắp xếp theo key alphabet
        var data = new SortedDictionary<string, string>
        {
            { "amount", intAmount.ToString() },
            { "cancelUrl", cancelUrl },        // ✅ thay vì _config.CancelUrl
            { "description", description },
            { "orderCode", orderCode.ToString() },
            { "returnUrl", returnUrl }         // ✅ thay vì _config.ReturnUrl
        };

        // ✅ Tạo chuỗi rawData dạng: key1=value1&key2=value2...
        var rawData = string.Join("&", data.Select(kv => $"{kv.Key}={kv.Value}"));

        // ✅ Tạo chữ ký HMAC SHA256 từ rawData với checksumKey (kết quả hex lowercase)
        string signature;
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.ChecksumKey)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            signature = BitConverter.ToString(hash).Replace("-", "").ToLower(); // HEX lowercase
        }

        // ✅ Ghi log chi tiết phục vụ debug
        _logger.LogInformation("🧾 [PayOS] DEBUG Log:");
        foreach (var kv in data)
            _logger.LogInformation("  🔸 {Key}: {Value}", kv.Key, kv.Value);
        _logger.LogInformation("  🔸 rawData: {RawData}", rawData);
        _logger.LogInformation("  🔸 checksumKey: {Key}", _config.ChecksumKey);
        _logger.LogInformation("  🔸 signature: {Signature}", signature);

        // 📦 Gửi dữ liệu đến PayOS
        var body = new
        {
            orderCode,
            amount = intAmount,
            description,
            returnUrl = returnUrl,
            cancelUrl = cancelUrl,
            signature
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        // 🛡️ Header xác thực
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("x-client-id", _config.ClientId);
        _client.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);

        // 🚀 Gửi request
        var response = await _client.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content);
        var json = await response.Content.ReadAsStringAsync();

        // 📥 Log phản hồi đẹp
        var formattedJson = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(json), new JsonSerializerOptions { WriteIndented = true });
        _logger.LogInformation("📥 [PayOS] Phản hồi:\n{Json}", formattedJson);

        // 💾 Ghi log ra file
        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "payos_response_log.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        await File.AppendAllTextAsync(logPath, $"[{DateTime.Now}] PayOS Response:\n{formattedJson}\n\n");

        // ✅ Xử lý phản hồi
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        string code = root.GetProperty("code").GetString() ?? "";
        if (code != "00")
        {
            string error = root.GetProperty("desc").GetString() ?? "Không rõ lỗi";
            _logger.LogError("❌ PayOS trả lỗi: {Error}", error);
            throw new Exception($"PayOS trả lỗi: {error}");
        }

        if (root.TryGetProperty("data", out JsonElement dataElement) &&
            dataElement.TryGetProperty("checkoutUrl", out JsonElement checkoutUrlElement))
        {
            string checkoutUrl = checkoutUrlElement.GetString() ?? "";
            _logger.LogInformation("✅ [PayOS] checkout URL: {Url}", checkoutUrl);
            return checkoutUrl;
        }

        throw new Exception("❌ 'data.checkoutUrl' không có trong phản hồi PayOS.");
    }








    public async Task<bool> TransferToBankAsync(string bankCode, string accountNumber, decimal amount, string description)
    {
        var body = new
        {
            amount = (int)amount,
            accountNumber,
            bankCode,
            description
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("https://api-merchant.payos.vn/bank/transfer", content);

        var json = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("💸 TransferToBank response:\n{Response}", json);

        return response.IsSuccessStatusCode;
    }
}

