using StudentFreelance.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Interfaces
{
    public interface IProjectSubmissionService
    {
        Task<ProjectSubmission> GetSubmissionByIdAsync(int id);
        Task<IEnumerable<ProjectSubmission>> GetSubmissionsByApplicationIdAsync(int applicationId);
        Task<ProjectSubmission> CreateSubmissionAsync(ProjectSubmission submission);
        Task<ProjectSubmission> UpdateSubmissionAsync(ProjectSubmission submission);
        Task<bool> DeleteSubmissionAsync(int id);
        Task<ProjectSubmission> ApproveSubmissionAsync(int submissionId, string feedback);
        Task<ProjectSubmission> RejectSubmissionAsync(int submissionId, string feedback);
        Task<bool> SaveSubmissionAttachmentAsync(ProjectSubmissionAttachment attachment);
        Task<bool> DeleteSubmissionAttachmentAsync(int attachmentId);
        Task<bool> FinalizeProjectAsync(int applicationId);
    }
} 