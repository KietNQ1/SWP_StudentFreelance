using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Interfaces
{
    public interface ILocationApiService
    {
        Task<List<LocationItemDto>> GetProvincesAsync(int page = 0, int size = 100, string query = null);
        Task<List<LocationItemDto>> GetDistrictsByProvinceAsync(string provinceId, int page = 0, int size = 100, string query = null);
        Task<List<LocationItemDto>> GetWardsByDistrictAsync(string districtId, int page = 0, int size = 100, string query = null);
    }
    
    public class LocationItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; } // ProvinceId for districts, DistrictId for wards
        public int Type { get; set; }
        public string TypeText { get; set; }
        public string Slug { get; set; }
    }
} 