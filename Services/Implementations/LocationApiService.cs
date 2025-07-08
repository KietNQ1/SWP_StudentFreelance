using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StudentFreelance.Services.Interfaces;
using System.Linq;

namespace StudentFreelance.Services.Implementations
{
    public class LocationApiService : ILocationApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LocationApiService> _logger;
        private readonly string _baseUrl = "https://open.oapi.vn/location";

        public LocationApiService(HttpClient httpClient, ILogger<LocationApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<LocationItemDto>> GetProvincesAsync(int page = 0, int size = 100, string query = null)
        {
            try
            {
                string url = $"{_baseUrl}/provinces?page={page}&size={size}";
                if (!string.IsNullOrEmpty(query))
                    url += $"&query={Uri.EscapeDataString(query)}";

                _logger.LogInformation($"Fetching provinces from: {url}");
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<ProvinceApiModel>>(url);
                
                if (response?.Code != "success" || response.Data == null)
                {
                    _logger.LogWarning("Failed to fetch provinces or empty response");
                    return new List<LocationItemDto>();
                }

                return response.Data.Select(p => new LocationItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Type = p.Type,
                    TypeText = p.TypeText,
                    Slug = p.Slug
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching provinces");
                return new List<LocationItemDto>();
            }
        }

        public async Task<List<LocationItemDto>> GetDistrictsByProvinceAsync(string provinceId, int page = 0, int size = 100, string query = null)
        {
            try
            {
                string url = $"{_baseUrl}/districts/{provinceId}?page={page}&size={size}";
                if (!string.IsNullOrEmpty(query))
                    url += $"&query={Uri.EscapeDataString(query)}";

                _logger.LogInformation($"Fetching districts from: {url}");
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<DistrictApiModel>>(url);
                
                if (response?.Code != "success" || response.Data == null)
                {
                    _logger.LogWarning($"Failed to fetch districts for province {provinceId} or empty response");
                    return new List<LocationItemDto>();
                }

                return response.Data.Select(d => new LocationItemDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    ParentId = d.ProvinceId,
                    Type = d.Type,
                    TypeText = d.TypeText
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching districts for province {provinceId}");
                return new List<LocationItemDto>();
            }
        }

        public async Task<List<LocationItemDto>> GetWardsByDistrictAsync(string districtId, int page = 0, int size = 100, string query = null)
        {
            try
            {
                string url = $"{_baseUrl}/wards/{districtId}?page={page}&size={size}";
                if (!string.IsNullOrEmpty(query))
                    url += $"&query={Uri.EscapeDataString(query)}";

                _logger.LogInformation($"Fetching wards from: {url}");
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<WardApiModel>>(url);
                
                if (response?.Code != "success" || response.Data == null)
                {
                    _logger.LogWarning($"Failed to fetch wards for district {districtId} or empty response");
                    return new List<LocationItemDto>();
                }

                return response.Data.Select(w => new LocationItemDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    ParentId = w.DistrictId,
                    Type = w.Type,
                    TypeText = w.TypeText
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching wards for district {districtId}");
                return new List<LocationItemDto>();
            }
        }
    }

    // API response models
    public class ApiResponse<T>
    {
        public int Total { get; set; }
        public List<T> Data { get; set; }
        public string Code { get; set; }
    }

    public class ProvinceApiModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string TypeText { get; set; }
        public string Slug { get; set; }
    }

    public class DistrictApiModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProvinceId { get; set; }
        public int Type { get; set; }
        public string TypeText { get; set; }
    }

    public class WardApiModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DistrictId { get; set; }
        public int Type { get; set; }
        public string TypeText { get; set; }
    }
} 