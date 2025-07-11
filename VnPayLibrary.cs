using System.Net;
using System.Security.Cryptography;
using System.Text;

public class VnPayLibrary
{
    private readonly SortedList<string, string> requestData = new();

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
            requestData[key] = value;
    }

    public string CreateRequestUrl(string baseUrl, string hashSecret)
    {
        var rawData = string.Join("&", requestData.Select(x => $"{x.Key}={x.Value}"));
        var queryString = string.Join("&", requestData.Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
        var secureHash = ComputeSha256(hashSecret + rawData);
        return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
    }

    public bool ValidateResponse(IQueryCollection query, string hashSecret)
    {
        var vnpSecureHash = query["vnp_SecureHash"];
        var data = query.Where(x => x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType")
                        .OrderBy(x => x.Key)
                        .ToDictionary(x => x.Key, x => x.Value.ToString());

        var rawData = string.Join("&", data.Select(x => $"{x.Key}={x.Value}"));
        var myHash = ComputeSha256(hashSecret + rawData);
        return myHash == vnpSecureHash;
    }

    private string ComputeSha256(string rawData)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}
