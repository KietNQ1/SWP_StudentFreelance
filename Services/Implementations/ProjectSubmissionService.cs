using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Implementations
{
    public class ProjectSubmissionService : IProjectSubmissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ProjectSubmissionService(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<ProjectSubmission> GetSubmissionByIdAsync(int id)
        {
            return await _context.ProjectSubmissions
                .Include(s => s.Application)
                    .ThenInclude(a => a.Project)
                        .ThenInclude(p => p.Business)
                .Include(s => s.Application)
                    .ThenInclude(a => a.User)
                .Include(s => s.Attachments)
                .FirstOrDefaultAsync(s => s.SubmissionID == id && s.IsActive);
        }

        public async Task<IEnumerable<ProjectSubmission>> GetSubmissionsByApplicationIdAsync(int applicationId)
        {
            return await _context.ProjectSubmissions
                .Include(s => s.Attachments)
                .Where(s => s.ApplicationID == applicationId && s.IsActive)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<ProjectSubmission> CreateSubmissionAsync(ProjectSubmission submission)
        {
            submission.SubmittedAt = DateTime.Now;
            submission.Status = "Pending";
            submission.IsActive = true;

            _context.ProjectSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            // Lấy thông tin application và project
            var application = await _context.StudentApplications
                .Include(a => a.Project)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationID == submission.ApplicationID);

            if (application != null)
            {
                // Cập nhật trạng thái application thành "PendingReview"
                application.Status = "PendingReview";
                application.LastStatusUpdate = DateTime.Now;
                _context.StudentApplications.Update(application);
                await _context.SaveChangesAsync();

                // Gửi thông báo cho doanh nghiệp
                await _notificationService.SendNotificationToUserAsync(
                    application.Project.BusinessID,
                    "Nộp kết quả dự án mới",
                    $"Sinh viên {application.User.FullName} đã nộp kết quả dự án '{application.Project.Title}'.",
                    1, // Notification type (project)
                    application.ProjectID
                );
            }

            return submission;
        }

        public async Task<ProjectSubmission> UpdateSubmissionAsync(ProjectSubmission submission)
        {
            _context.ProjectSubmissions.Update(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task<bool> DeleteSubmissionAsync(int id)
        {
            var submission = await _context.ProjectSubmissions.FindAsync(id);
            if (submission == null)
                return false;

            submission.IsActive = false;
            _context.ProjectSubmissions.Update(submission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectSubmission> ApproveSubmissionAsync(int submissionId, string feedback)
        {
            var submission = await _context.ProjectSubmissions
                .Include(s => s.Application)
                    .ThenInclude(a => a.Project)
                .Include(s => s.Application)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(s => s.SubmissionID == submissionId);

            if (submission == null)
                return null;

            submission.Status = "Approved";
            submission.BusinessFeedback = feedback;
            submission.FeedbackDate = DateTime.Now;

            _context.ProjectSubmissions.Update(submission);

            // Cập nhật trạng thái application thành "Completed"
            var application = submission.Application;
            application.Status = "Completed";
            application.LastStatusUpdate = DateTime.Now;
            application.CompletionDate = DateTime.Now;
            _context.StudentApplications.Update(application);

            await _context.SaveChangesAsync();

            // Gửi thông báo cho sinh viên
            await _notificationService.SendNotificationToUserAsync(
                application.UserID,
                "Kết quả dự án được chấp nhận",
                $"Doanh nghiệp đã chấp nhận kết quả dự án '{application.Project.Title}' của bạn.",
                1, // Notification type (project)
                application.ProjectID
            );

            return submission;
        }

        public async Task<ProjectSubmission> RejectSubmissionAsync(int submissionId, string feedback)
        {
            var submission = await _context.ProjectSubmissions
                .Include(s => s.Application)
                    .ThenInclude(a => a.Project)
                .Include(s => s.Application)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(s => s.SubmissionID == submissionId);

            if (submission == null)
                return null;

            submission.Status = "Rejected";
            submission.BusinessFeedback = feedback;
            submission.FeedbackDate = DateTime.Now;

            _context.ProjectSubmissions.Update(submission);

            // Cập nhật trạng thái application thành "InProgress" để sinh viên có thể nộp lại
            var application = submission.Application;
            application.Status = "InProgress";
            application.LastStatusUpdate = DateTime.Now;
            _context.StudentApplications.Update(application);

            await _context.SaveChangesAsync();

            // Gửi thông báo cho sinh viên
            await _notificationService.SendNotificationToUserAsync(
                application.UserID,
                "Kết quả dự án bị từ chối",
                $"Doanh nghiệp đã từ chối kết quả dự án '{application.Project.Title}' của bạn. Vui lòng xem phản hồi và nộp lại.",
                1, // Notification type (project)
                application.ProjectID
            );

            return submission;
        }

        public async Task<bool> SaveSubmissionAttachmentAsync(ProjectSubmissionAttachment attachment)
        {
            try
            {
                // Check if submission exists
                var submission = await _context.ProjectSubmissions.FindAsync(attachment.SubmissionID);
                if (submission == null)
                {
                    return false;
                }
                
                _context.ProjectSubmissionAttachments.Add(attachment);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteSubmissionAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.ProjectSubmissionAttachments.FindAsync(attachmentId);
            if (attachment == null)
                return false;

            attachment.IsActive = false;
            _context.ProjectSubmissionAttachments.Update(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FinalizeProjectAsync(int applicationId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var application = await _context.StudentApplications
                    .Include(a => a.Project)
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

                if (application == null || application.Status != "Completed")
                {
                    Console.WriteLine($"FinalizeProjectAsync: Không thể thanh toán - application không tồn tại hoặc không ở trạng thái Completed. ApplicationID: {applicationId}");
                    return false;
                }

                // Kiểm tra xem đã thanh toán chưa
                if (application.IsPaid)
                {
                    Console.WriteLine($"FinalizeProjectAsync: Không thể thanh toán - application đã được thanh toán trước đó. ApplicationID: {applicationId}");
                    return false;
                }

                // Kiểm tra xem ví dự án có đủ tiền không
                if (application.Project.ProjectWallet < application.Salary)
                {
                    Console.WriteLine($"FinalizeProjectAsync: Không thể thanh toán - ví dự án không đủ tiền. ApplicationID: {applicationId}, ProjectWallet: {application.Project.ProjectWallet}, Salary: {application.Salary}");
                    return false;
                }

                // Trừ tiền từ ví dự án
                application.Project.ProjectWallet -= application.Salary;
                _context.Projects.Update(application.Project);

                // Đánh dấu đã thanh toán
                application.IsPaid = true;
                _context.StudentApplications.Update(application);

                // Cập nhật số dư ví của sinh viên
                if (application.User != null)
                {
                    application.User.WalletBalance += application.Salary;
                    _context.Users.Update(application.User);
                }

                // Tạo giao dịch thanh toán cho sinh viên
                // Lấy loại giao dịch "Thanh toán cho sinh viên"
                var paymentTypeId = _context.TransactionTypes.FirstOrDefault(t => t.TypeName == "Thanh toán cho sinh viên")?.TypeID ?? 7;
                
                var transaction = new Transaction
                {
                    UserID = application.UserID,
                    ProjectID = application.ProjectID,
                    Amount = application.Salary,
                    TypeID = paymentTypeId, // Thanh toán cho sinh viên
                    TransactionDate = DateTime.Now,
                    Description = $"Thanh toán cho dự án '{application.Project.Title}'",
                    StatusID = 2, // Thành công (StatusID = 2)
                    IsActive = true
                };
                _context.Transactions.Add(transaction);

                await _context.SaveChangesAsync();

                // Gửi thông báo cho sinh viên
                await _notificationService.SendNotificationToUserAsync(
                    application.UserID,
                    "Thanh toán dự án",
                    $"Bạn đã nhận được thanh toán cho dự án '{application.Project.Title}'.",
                    2, // Notification type (transaction)
                    application.ProjectID
                );

                // Gửi thông báo cho doanh nghiệp
                await _notificationService.SendNotificationToUserAsync(
                    application.Project.BusinessID,
                    "Thanh toán dự án",
                    $"Đã thanh toán cho sinh viên {application.User.FullName} cho dự án '{application.Project.Title}'.",
                    2, // Notification type (transaction)
                    application.ProjectID
                );

                await dbTransaction.CommitAsync();
                Console.WriteLine($"FinalizeProjectAsync: Thanh toán thành công. ApplicationID: {applicationId}, Amount: {application.Salary}");
                return true;
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                Console.WriteLine($"FinalizeProjectAsync: Lỗi khi thanh toán. ApplicationID: {applicationId}, Error: {ex.Message}");
                return false;
            }
        }
    }
} 