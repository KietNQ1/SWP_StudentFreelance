using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Components.Forms;

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

        //public string CreateRequestUrl(string baseUrl, string hashSecret)
        //{
        //    // Phải chắc chắn rằng "vnp_SecureHashType" đã có trong requestData từ trước!
        //    //if (!requestData.ContainsKey("vnp_SecureHashType"))
        //    //    requestData.Add("vnp_SecureHashType", "HMACSHA512");

        //    // Dictionary chứa các tham số vnp_
        //    var sortedParams = requestData
        //        .Where(x => !string.IsNullOrEmpty(x.Value) && x.Key.StartsWith("vnp_") && x.Key != "vnp_SecureHash")
        //        .OrderBy(x => x.Key)
        //        .ToList();

        //    // Ghép chuỗi dạng key1=value1&key2=value2...
        //    var signData = string.Join("&", sortedParams.Select(kv => $"{kv.Key}={HttpUtility.UrlEncode(kv.Value)}"));
        //    string secureHash = ComputeHmacSHA512("HASHSECRET", signData);
        //    // BẮT BUỘC sort theo A-Z trước khi ký
        //    var ordered = requestData.OrderBy(kvp => kvp.Key);

        //    // Tạo rawData dùng để ký
        //    //var rawData = string.Join("&", requestData.OrderBy(kvp => kvp.Key)
        //    //                                          .Select(kvp => $"{kvp.Key}={kvp.Value}"));
        //    var rawData = string.Join("&", requestData.Select(kvp => $"{kvp.Key}={kvp.Value}"));


        //    // Tạo chữ ký
        //    var hash = ComputeHmacSHA512(rawData, hashSecret);

        //    // Tạo URL để redirect
        //    var query = string.Join("&", requestData.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

        //    Console.WriteLine("======== RAW DATA FOR HASH ========");
        //    foreach (var kvp in requestData)
        //    {
        //        Console.WriteLine($"{kvp.Key} = {kvp.Value}");
        //    }
        //    Console.WriteLine("Raw data string: " + rawData);
        //    Console.WriteLine("Generated hash: " + hash);

        //    return $"{baseUrl}?{query}&vnp_SecureHash={hash}";
        //}

        private string ComputeHmacSHA512(string input, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var sortedParams = requestData
                                .Where(x => !string.IsNullOrEmpty(x.Value)
                                            && x.Key.StartsWith("vnp_")
                                            && x.Key != "vnp_SecureHash"
                                            && x.Key != "vnp_SecureHashType")
                                .OrderBy(x => x.Key)
                                .ToList();

            var signData = string.Join("&", sortedParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

            string secureHash = ComputeHmacSHA512(signData, hashSecret);

            // 3. Chỉ thêm vnp_SecureHash sau khi ký xong
            requestData["vnp_SecureHash"] = secureHash;

            // 4. LÚC NÀY mới thêm SecureHashType (sau khi ký)
            requestData["vnp_SecureHashType"] = "HMACSHA512";

            var query = string.Join("&", requestData.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

            return $"{baseUrl}?{query}";
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


        //private string ComputeHmacSHA512(string input, string key)
        //{
        //    //byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        //    //byte[] inputBytes = Encoding.UTF8.GetBytes(rawData);
        //    //using var hmac = new HMACSHA512(keyBytes);
        //    //byte[] hashBytes = hmac.ComputeHash(inputBytes);
        //    //return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();

        //    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        //    byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        //    using (var hmac = new HMACSHA512(keyBytes))
        //    {
        //        byte[] hashValue = hmac.ComputeHash(inputBytes);
        //        return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        //    }
        //}

    }
}
