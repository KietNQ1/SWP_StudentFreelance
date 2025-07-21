using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Implementations
{
    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentApplication>> GetAllApplicationsAsync()
        {
            return await _context.StudentApplications
                .Include(a => a.User)
                .Include(a => a.Project)
                .OrderByDescending(a => a.DateApplied)
                .ToListAsync();
        }

        public async Task<StudentApplication> GetApplicationByIdAsync(int id)
        {
            return await _context.StudentApplications
                .Include(a => a.User)
                .Include(a => a.Project)
                .FirstOrDefaultAsync(a => a.ApplicationID == id);
        }

        public async Task<List<ApplicationDetailViewModel>> GetApplicationsByProjectIdAsync(int projectId)
        {
            var applications = await _context.StudentApplications
                .Include(a => a.User)
                .Include(a => a.Project)
                .Where(a => a.ProjectID == projectId)
                .OrderByDescending(a => a.DateApplied)
                .AsNoTracking()
                .ToListAsync();

            var result = new List<ApplicationDetailViewModel>();
            
            foreach (var app in applications)
            {
                // Lấy danh sách kỹ năng của sinh viên
                var studentSkills = await _context.StudentSkills
                    .Include(ss => ss.Skill)
                    .Include(ss => ss.ProficiencyLevel)
                    .Where(ss => ss.UserID == app.UserID && ss.IsActive)
                    .AsNoTracking()
                    .Select(ss => new SkillViewModel
                    {
                        SkillID = ss.SkillID,
                        SkillName = ss.Skill.SkillName,
                        ProficiencyLevelName = ss.ProficiencyLevel.LevelName
                    })
                    .ToListAsync();
                
                // Tính điểm đánh giá trung bình của sinh viên
                var ratings = await _context.Ratings
                    .Where(r => r.RevieweeID == app.UserID && r.IsActive)
                    .AsNoTracking()
                    .Select(r => r.Score)
                    .ToListAsync();
                
                decimal? averageRating = null;
                if (ratings.Any())
                {
                    averageRating = Math.Round((decimal)ratings.Average(), 1);
                }
                
                // Tính thời gian đã trôi qua kể từ khi ứng tuyển
                string timeAgo = GetTimeAgo(app.DateApplied);
                
                // Chuyển đổi sang ViewModel
                var viewModel = new ApplicationDetailViewModel
                {
                    ApplicationID = app.ApplicationID,
                    ProjectID = app.ProjectID,
                    ProjectTitle = app.Project.Title,
                    UserID = app.UserID,
                    UserName = app.User.UserName,
                    UserFullName = app.User.FullName,
                    UserEmail = app.User.Email,
                    UserPhone = app.User.PhoneNumber,
                    UserAvatar = app.User.Avatar ?? "/images/default-avatar.png",
                    UserAverageRating = averageRating,
                    CoverLetter = app.CoverLetter,
                    Salary = app.Salary,
                    Status = app.Status,
                    DateApplied = app.DateApplied,
                    TimeAgo = timeAgo,
                    StudentSkills = studentSkills,
                    BusinessNotes = app.BusinessNotes,
                    ResumeAttachment = app.ResumeAttachment,
                    PortfolioLink = app.PortfolioLink,
                    LastStatusUpdate = app.LastStatusUpdate
                };
                
                result.Add(viewModel);
            }
            
            return result;
        }

        public async Task<IEnumerable<StudentApplication>> GetApplicationsByStudentIdAsync(int studentId)
        {
            return await _context.StudentApplications
                .Include(a => a.User)
                .Include(a => a.Project)
                    .ThenInclude(p => p.Business)
                .Where(a => a.UserID == studentId)
                .OrderByDescending(a => a.DateApplied)
                .ToListAsync();
        }

        public async Task<StudentApplication> CreateApplicationAsync(StudentApplication application)
        {
            try
            {
                // Đảm bảo các giá trị mặc định được thiết lập
                application.DateApplied = DateTime.Now;
                application.Status = "Pending";
                application.LastStatusUpdate = DateTime.Now;
                
                // ResumeAttachment có thể null, không cần kiểm tra

                // Debug info
                Console.WriteLine($"Thêm đơn ứng tuyển: ProjectID={application.ProjectID}, UserID={application.UserID}, ResumeAttachment={application.ResumeAttachment ?? "null"}");
                
                // Thêm entity trực tiếp thay vì sử dụng SQL raw
                _context.StudentApplications.Add(application);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Đã thêm đơn ứng tuyển với ID: {application.ApplicationID}");
                
                // Lấy đơn ứng tuyển vừa tạo để trả về
                var createdApplication = await _context.StudentApplications
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ApplicationID == application.ApplicationID);
                
                return createdApplication;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết
                Console.WriteLine($"Lỗi khi tạo đơn ứng tuyển: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }
        
        public async Task<StudentApplication> ApplyToProjectAsync(int projectId, int studentId, string coverLetter, decimal salary)
        {
            try
            {
                // Kiểm tra xem dự án có tồn tại không
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null) 
                {
                    Console.WriteLine($"Không tìm thấy dự án với ID: {projectId}");
                    return null;
                }
                
                // Kiểm tra xem sinh viên đã ứng tuyển vào dự án này chưa
                bool alreadyApplied = await HasStudentAppliedAsync(projectId, studentId);
                if (alreadyApplied)
                {
                    Console.WriteLine($"Sinh viên ID: {studentId} đã ứng tuyển vào dự án ID: {projectId}");
                    return null;
                }
                
                // Tạo đơn ứng tuyển mới
                var application = new StudentApplication
                {
                    ProjectID = projectId,
                    UserID = studentId,
                    CoverLetter = coverLetter,
                    Salary = salary,
                    DateApplied = DateTime.Now,
                    Status = "Pending",
                    LastStatusUpdate = DateTime.Now,
                    BusinessConfirmedCompletion = false,
                    StudentConfirmedCompletion = false
                };
                
                // Debug info
                Console.WriteLine($"Thêm đơn ứng tuyển mới: ProjectID={projectId}, UserID={studentId}");
                
                // Thêm entity trực tiếp thay vì sử dụng SQL raw
                _context.StudentApplications.Add(application);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Đã thêm đơn ứng tuyển với ID: {application.ApplicationID}");
                
                return application;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết
                Console.WriteLine($"Lỗi khi ứng tuyển: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đơn ứng tuyển
        /// </summary>
        public async Task<StudentApplication> UpdateApplicationStatusAsync(int applicationId, string status)
        {
            // Sử dụng truy vấn trực tiếp để lấy đối tượng có thể cập nhật
            var application = await _context.StudentApplications
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
                return null;

            try
            {
                // Cập nhật trạng thái
                application.Status = status;
                application.LastStatusUpdate = DateTime.Now;
                
                // Thực hiện lưu trực tiếp thay đổi vào database
                _context.StudentApplications.Update(application);
                await _context.SaveChangesAsync();
                
                // Tải lại đối tượng với dữ liệu mới nhất
                return await _context.StudentApplications
                    .AsNoTracking() // Đảm bảo lấy dữ liệu mới từ DB
                    .Include(a => a.User)
                    .Include(a => a.Project)
                    .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết
                Console.WriteLine($"Lỗi khi cập nhật trạng thái: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        public async Task<bool> DeleteApplicationAsync(int id)
        {
            try
            {
                var application = await _context.StudentApplications.FindAsync(id);
                if (application == null)
                    return false;

                _context.StudentApplications.Remove(application);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa đơn ứng tuyển: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> HasStudentAppliedAsync(int projectId, int studentId)
        {
            return await _context.StudentApplications
                .AnyAsync(a => a.ProjectID == projectId && a.UserID == studentId);
        }
        
        /// <summary>
        /// Cập nhật thông tin phỏng vấn cho đơn ứng tuyển
        /// </summary>
        public async Task<StudentApplication> ScheduleInterviewAsync(int applicationId, DateTime interviewTime)
        {
            var application = await _context.StudentApplications
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
                return null;

            try
            {
                application.InterviewSchedule = interviewTime;
                application.Status = "Interview";
                application.LastStatusUpdate = DateTime.Now;
                
                _context.StudentApplications.Update(application);
                await _context.SaveChangesAsync();
                
                return await _context.StudentApplications
                    .AsNoTracking()
                    .Include(a => a.User)
                    .Include(a => a.Project)
                    .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lên lịch phỏng vấn: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Cập nhật đánh giá và ghi chú của doanh nghiệp
        /// </summary>
        public async Task<bool> UpdateBusinessFeedbackAsync(int applicationId, string notes)
        {
            var application = await _context.StudentApplications.FindAsync(applicationId);
            if (application == null)
                return false;
                
            application.BusinessNotes = notes;
            application.LastStatusUpdate = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        /// <summary>
        /// Cập nhật kết quả phỏng vấn
        /// </summary>
        public async Task<StudentApplication> UpdateInterviewResultAsync(int applicationId, string result)
        {
            var application = await _context.StudentApplications
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
                return null;

            try
            {
                application.InterviewResult = result;
                application.LastStatusUpdate = DateTime.Now;
                
                _context.StudentApplications.Update(application);
                await _context.SaveChangesAsync();
                
                return await _context.StudentApplications
                    .AsNoTracking()
                    .Include(a => a.User)
                    .Include(a => a.Project)
                    .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật kết quả phỏng vấn: {ex.Message}");
                return null;
            }
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;
            
            if (span.TotalDays > 30)
                return $"{Math.Floor(span.TotalDays / 30)} tháng trước";
            if (span.TotalDays > 1)
                return $"{Math.Floor(span.TotalDays)} ngày trước";
            if (span.TotalHours > 1)
                return $"{Math.Floor(span.TotalHours)} giờ trước";
            if (span.TotalMinutes > 1)
                return $"{Math.Floor(span.TotalMinutes)} phút trước";
            
            return "Vừa xong";
        }

        // Lấy chi tiết đơn ứng tuyển
        public async Task<ApplicationDetailViewModel> GetApplicationDetailAsync(int applicationId)
        {
            var application = await _context.StudentApplications
                .Include(a => a.User)
                .Include(a => a.Project)
                    .ThenInclude(p => p.Business)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);
                
            if (application == null)
                return null;
                
            // Lấy danh sách kỹ năng của sinh viên
            var studentSkills = await _context.StudentSkills
                .Include(ss => ss.Skill)
                .Include(ss => ss.ProficiencyLevel)
                .Where(ss => ss.UserID == application.UserID && ss.IsActive)
                .ToListAsync();
                
            var skillViewModels = studentSkills.Select(ss => new SkillViewModel
            {
                SkillID = ss.SkillID,
                SkillName = ss.Skill.SkillName,
                ProficiencyLevelName = ss.ProficiencyLevel.LevelName
            }).ToList();
            
            // Tính thời gian đã trôi qua từ khi ứng tuyển
            var timeAgo = GetTimeAgo(application.DateApplied);
            
            return new ApplicationDetailViewModel
            {
                ApplicationID = application.ApplicationID,
                ProjectID = application.ProjectID,
                ProjectTitle = application.Project.Title,
                
                UserID = application.UserID,
                UserName = application.User.UserName,
                UserFullName = application.User.FullName,
                UserEmail = application.User.Email,
                UserPhone = application.User.PhoneNumber,
                UserAvatar = application.User.Avatar ?? "/img/default-avatar.png",
                UserAverageRating = application.User.AverageRating,
                
                CoverLetter = application.CoverLetter,
                Salary = application.Salary,
                Status = application.Status,
                TimeAgo = timeAgo,
                StudentSkills = skillViewModels,
                
                BusinessNotes = application.BusinessNotes,
                ResumeAttachment = application.ResumeAttachment,
                PortfolioLink = application.PortfolioLink,
                
                DateApplied = application.DateApplied,
                LastStatusUpdate = application.LastStatusUpdate,
                
                Project = application.Project
            };
        }
    }
} 