using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Interfaces
{
    public interface IApplicationService
    {
        Task<IEnumerable<StudentApplication>> GetAllApplicationsAsync();
        Task<StudentApplication> GetApplicationByIdAsync(int id);
        Task<List<ApplicationDetailViewModel>> GetApplicationsByProjectIdAsync(int projectId);
        Task<IEnumerable<StudentApplication>> GetApplicationsByStudentIdAsync(int studentId);
        Task<StudentApplication> CreateApplicationAsync(StudentApplication application);
        Task<StudentApplication> UpdateApplicationStatusAsync(int applicationId, string status);
        Task<bool> DeleteApplicationAsync(int id);
        Task<bool> HasStudentAppliedAsync(int projectId, int studentId);
        Task<StudentApplication> ApplyToProjectAsync(int projectId, int studentId, string coverLetter, decimal salary);
        
        // Các phương thức mới
        Task<StudentApplication> ScheduleInterviewAsync(int applicationId, DateTime interviewTime);
        Task<bool> UpdateBusinessFeedbackAsync(int applicationId, string notes);
        Task<StudentApplication> UpdateInterviewResultAsync(int applicationId, string result);

        // Lấy chi tiết đơn ứng tuyển
        Task<ApplicationDetailViewModel> GetApplicationDetailAsync(int applicationId);
    }
} 