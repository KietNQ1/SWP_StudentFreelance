using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace StudentFreelance.Helpers
{
    public class VnPayLibrary
    {
        private SortedList<string, string> requestData = new SortedList<string, string>();
        private SortedList<string, string> responseData = new SortedList<string, string>();

        public void AddRequestData(string key, string value)
        {
            requestData.Add(key, value);
        }

        //test log 
        public Dictionary<string, string> GetRequestData()
        {
            return new Dictionary<string, string>(requestData);
        }

        public void AddResponseData(string key, string value)
        {
            responseData.Add(key, value);
        }

        public string GetResponseData(string key)
        {
            return responseData.TryGetValue(key, out string value) ? value : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            // Phải chắc chắn rằng "vnp_SecureHashType" đã có trong requestData từ trước!
            //if (!requestData.ContainsKey("vnp_SecureHashType"))
            //    requestData.Add("vnp_SecureHashType", "HMACSHA512");

            // Tạo rawData dùng để ký
            var rawData = string.Join("&", requestData.OrderBy(kvp => kvp.Key)
                                                      .Select(kvp => $"{kvp.Key}={kvp.Value}"));

            // Tạo chữ ký
            var hash = ComputeHmacSHA512(rawData, hashSecret);

            // Tạo URL để redirect
            var query = string.Join("&", requestData.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            return $"{baseUrl}?{query}&vnp_SecureHash={hash}";
        }


        public bool ValidateSignature(string hashSecret)
        {
            var hash = GetResponseData("vnp_SecureHash");

            var filtered = responseData
                .Where(kvp => kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType")
                .OrderBy(kvp => kvp.Key);

            var rawData = string.Join("&", filtered.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var computedHash = ComputeHmacSHA512(rawData, hashSecret); // ✅ dùng lại hàm đúng

            return string.Equals(hash, computedHash, StringComparison.InvariantCultureIgnoreCase);
        }


        private string ComputeHmacSHA512(string rawData, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(rawData);
            using var hmac = new HMACSHA512(keyBytes);
            byte[] hashBytes = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

    }
}
