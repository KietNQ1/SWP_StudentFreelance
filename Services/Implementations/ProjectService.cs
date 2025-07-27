using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;

namespace StudentFreelance.Services.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITransactionService _transactionService;

        public ProjectService(ApplicationDbContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync(bool includeInactive = false, int? userId = null)
        {
            var query = _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Business)
                .Include(p => p.Address)
                .AsQueryable();

            if (includeInactive)
            {
                // Nếu là admin hoặc moderator, hiển thị tất cả dự án
                if (userId.HasValue)
                {
                    // Nếu là người dùng thường, hiển thị dự án active + dự án của họ
                    query = query.Where(p => p.IsActive || p.BusinessID == userId.Value);
                }
                // Nếu không có userId và includeInactive = true, đây là admin/mod, hiển thị tất cả
            }
            else
            {
                // Nếu không bao gồm inactive, chỉ hiển thị dự án đang hoạt động
                query = query.Where(p => p.IsActive);
            }
            
            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project> GetProjectByIdAsync(int id, bool includeInactive = false, int? userId = null)
        {
            var query = _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Business)
                .Include(p => p.Address)
                .Include(p => p.ProjectSkillsRequired)
                    .ThenInclude(psr => psr.Skill)
                .Include(p => p.ProjectAttachments)
                .Include(p => p.StudentApplications)
                    .ThenInclude(sa => sa.User)
                .Where(p => p.ProjectID == id);

            if (includeInactive)
            {
                if (userId.HasValue)
                {
                    // Nếu là chủ dự án, hiển thị dự án của họ kể cả inactive
                    query = query.Where(p => p.IsActive || p.BusinessID == userId.Value);
                }
                // Nếu là admin/moderator (không có userId), hiển thị tất cả
            }
            else
            {
                // Người dùng thông thường chỉ xem được dự án đang hoạt động
                query = query.Where(p => p.IsActive);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            try
            {
                // Set default values
                project.CreatedAt = DateTime.UtcNow;
                project.UpdatedAt = DateTime.UtcNow;
                project.IsActive = true;
                
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                
                return project;
            }
            catch (Exception ex)
            {
                throw; // Re-throw to be handled by the controller
            }
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            var existingProject = await _context.Projects.FindAsync(project.ProjectID);
            
            if (existingProject == null)
                return null;
                
            // Update only the properties that should be updated
            existingProject.Title = project.Title;
            existingProject.Description = project.Description;
            existingProject.Budget = project.Budget;
            existingProject.Deadline = project.Deadline;
            existingProject.StatusID = project.StatusID;
            existingProject.IsHighlighted = project.IsHighlighted;
            existingProject.TypeID = project.TypeID;
            existingProject.AddressID = project.AddressID;
            existingProject.IsRemoteWork = project.IsRemoteWork;
            existingProject.CategoryID = project.CategoryID;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return existingProject;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            
            if (project == null)
                return false;
                
            // Soft delete - mark as inactive
            project.IsActive = false;
            project.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            
            if (project == null)
                return false;
                
            // Activate project
            project.IsActive = true;
            project.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId)
        {
            return await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Where(p => p.BusinessID.ToString() == userId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(int statusId)
        {
            return await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Business)
                .Where(p => p.StatusID == statusId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByTypeAsync(int typeId)
        {
            return await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Business)
                .Where(p => p.TypeID == typeId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateProjectStatusAsync(int projectId, int statusId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            
            if (project == null)
                return false;
                
            project.StatusID = statusId;
            project.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmProjectCompletionAsync(int projectId, int applicationId, bool isBusinessConfirmation)
        {
            var project = await _context.Projects.FindAsync(projectId);
            var application = await _context.StudentApplications.FindAsync(applicationId);
            
            if (project == null || application == null)
                return false;
                
            if (application.ProjectID != projectId)
                return false;
            
            // Nếu application đang ở trạng thái InProgress, chuyển sang PendingReview
            // Nếu đang ở trạng thái Completed, giữ nguyên trạng thái
            if (application.Status == "InProgress")
            {
                application.Status = "PendingReview";
            }
            
            if (isBusinessConfirmation)
            {
                application.BusinessConfirmedCompletion = true;
            }
            else
            {
                application.StudentConfirmedCompletion = true;
            }
            
            await _context.SaveChangesAsync();
            
            // Nếu cả hai bên đã xác nhận hoàn thành, cập nhật trạng thái application thành Completed
            if (application.BusinessConfirmedCompletion && application.StudentConfirmedCompletion)
            {
                application.Status = "Completed";
                await _context.SaveChangesAsync();
            }
            
            return true;
        }
        
        public async Task<bool> CompleteProjectAndTransferFundsAsync(int projectId, int applicationId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
        {
            var project = await _context.Projects.FindAsync(projectId);
            var application = await _context.StudentApplications
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);
            
            if (project == null || application == null)
                return false;
                
            if (application.ProjectID != projectId)
                return false;
            
                // Update application status only, not project status
                // project.StatusID = 3; // Không tự động chuyển dự án sang trạng thái hoàn thành
            
            // Update application status
            application.Status = "Completed";
                
                // Get the payment amount from the application's salary
                decimal paymentAmount = application.Salary;
                
                // Check if project wallet has enough funds
                if (project.ProjectWallet < paymentAmount)
                    return false;
                
                // Deduct payment from project wallet
                project.ProjectWallet -= paymentAmount;
            
            // Create transaction to transfer funds from business to student
            var transaction = new Transaction
            {
                UserID = application.UserID,
                ProjectID = projectId,
                    Amount = paymentAmount,
                TypeID = 4, // Assuming 4 is "ProjectPayment"
                TransactionDate = DateTime.UtcNow,
                Description = $"Payment for completed project: {project.Title}",
                StatusID = 1, // Assuming 1 is "Completed"
                IsActive = true
            };
            
            await _context.Transactions.AddAsync(transaction);
            
            // Update student's wallet balance
            var student = application.User;
            if (student != null)
            {
                    student.WalletBalance += paymentAmount;
                _context.Users.Update(student);
            }
            
            await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            return true;
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Hoàn thành dự án bởi chủ dự án
        /// </summary>
        /// <param name="projectId">ID của dự án</param>
        /// <param name="businessId">ID của chủ dự án</param>
        /// <returns>Kết quả thực hiện và thông báo lỗi nếu có</returns>
        public async Task<(bool Success, string ErrorMessage)> CompleteProjectByBusinessAsync(int projectId, int businessId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy thông tin dự án
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return (false, "Không tìm thấy dự án");
                
                // Kiểm tra quyền
                if (project.BusinessID != businessId)
                    return (false, "Bạn không phải là chủ dự án này");
                
                // Kiểm tra trạng thái dự án
                if (project.StatusID == 3) // Đã hoàn thành
                    return (false, "Dự án đã được hoàn thành trước đó");
                
                if (project.StatusID == 4) // Đã hủy
                    return (false, "Dự án đã bị hủy");
                
                // Kiểm tra xem tất cả sinh viên đã hoàn thành công việc chưa
                var applications = await _context.StudentApplications
                    .Where(a => a.ProjectID == projectId && a.Status == "Accepted" || a.Status == "InProgress" || a.Status == "PendingReview")
                    .ToListAsync();
                
                if (applications.Any())
                    return (false, "Vẫn còn sinh viên đang làm việc trong dự án này. Tất cả sinh viên phải hoàn thành công việc trước khi dự án có thể được đánh dấu là hoàn thành.");
                
                // Cập nhật trạng thái dự án thành "Hoàn thành"
                project.StatusID = 3; // Completed
                project.UpdatedAt = DateTime.UtcNow;
                
                // Hoàn trả tiền còn lại trong ví dự án cho chủ dự án
                if (project.ProjectWallet > 0)
                {
                    var business = await _context.Users.FindAsync(businessId);
                    if (business != null)
                    {
                        decimal refundAmount = project.ProjectWallet;
                        
                        // Add refund to business wallet
                        business.WalletBalance += refundAmount;
                        
                        // Create refund transaction
                        var refundTypeId = 4; // Refund transaction type
                        var completedStatusId = _context.TransactionStatuses.FirstOrDefault(s => s.StatusName == "Completed")?.StatusID ?? 1;
                        
                        var refundTxn = new Transaction
                        {
                            UserID = project.BusinessID,
                            ProjectID = projectId,
                            Amount = refundAmount,
                            Description = $"Hoàn tiền còn dư sau khi hoàn thành dự án: {project.Title}",
                            TransactionDate = DateTime.Now,
                            TypeID = refundTypeId,
                            StatusID = completedStatusId,
                            IsActive = true
                        };
                        
                        _context.Transactions.Add(refundTxn);
                        
                        // Set project wallet to zero
                        project.ProjectWallet = 0;
                    }
                }
                
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return (false, $"Lỗi khi hoàn thành dự án: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Hủy dự án bởi chủ dự án
        /// </summary>
        /// <param name="projectId">ID của dự án</param>
        /// <param name="businessId">ID của chủ dự án</param>
        /// <returns>Kết quả thực hiện và thông báo lỗi nếu có</returns>
        public async Task<(bool Success, string ErrorMessage)> CancelProjectByBusinessAsync(int projectId, int businessId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy thông tin dự án
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return (false, "Không tìm thấy dự án");
                
                // Kiểm tra quyền
                if (project.BusinessID != businessId)
                    return (false, "Bạn không phải là chủ dự án này");
                
                // Kiểm tra trạng thái dự án
                if (project.StatusID == 3) // Đã hoàn thành
                    return (false, "Dự án đã hoàn thành, không thể hủy");
                
                if (project.StatusID == 4) // Đã hủy
                    return (false, "Dự án đã bị hủy trước đó");
                
                // Kiểm tra xem có sinh viên đang làm việc không
                var activeApplications = await _context.StudentApplications
                    .Where(a => a.ProjectID == projectId && (a.Status == "Accepted" || a.Status == "InProgress" || a.Status == "PendingReview"))
                    .ToListAsync();
                
                if (activeApplications.Any())
                    return (false, "Không thể hủy dự án vì vẫn còn sinh viên đang làm việc trong dự án này");
                
                // Cập nhật trạng thái dự án thành "Đã hủy"
                project.StatusID = 4; // Cancelled
                project.UpdatedAt = DateTime.UtcNow;
                
                // Hoàn trả toàn bộ tiền trong ví dự án cho chủ dự án
                if (project.ProjectWallet > 0)
                {
                    var business = await _context.Users.FindAsync(businessId);
                    if (business != null)
                    {
                        decimal refundAmount = project.ProjectWallet;
                        
                        // Add refund to business wallet
                        business.WalletBalance += refundAmount;
                        
                        // Create refund transaction
                        var refundTypeId = 4; // Refund transaction type
                        var completedStatusId = _context.TransactionStatuses.FirstOrDefault(s => s.StatusName == "Completed")?.StatusID ?? 1;
                        
                        var refundTxn = new Transaction
                        {
                            UserID = project.BusinessID,
                            ProjectID = projectId,
                            Amount = refundAmount,
                            Description = $"Hoàn tiền do hủy dự án: {project.Title}",
                            TransactionDate = DateTime.Now,
                            TypeID = refundTypeId,
                            StatusID = completedStatusId,
                            IsActive = true
                        };
                        
                        _context.Transactions.Add(refundTxn);
                        
                        // Set project wallet to zero
                        project.ProjectWallet = 0;
                    }
                }
                
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return (false, $"Lỗi khi hủy dự án: {ex.Message}");
            }
        }

        public async Task<(bool IsEnough, decimal MissingAmount)> CheckProjectBudgetForAcceptedStudentsAsync(int projectId, int newStudentApplicationId)
        {
            // Lấy thông tin dự án
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return (false, 0);

            // Lấy tất cả đơn ứng tuyển đã được chấp nhận (bao gồm cả đơn mới)
            var acceptedApplications = await _context.StudentApplications
                .Where(a => a.ProjectID == projectId && 
                           (a.Status == "Accepted" || a.ApplicationID == newStudentApplicationId))
                .ToListAsync();

            if (!acceptedApplications.Any())
                return (true, 0);

            // Tính tổng số tiền lương của tất cả ứng viên được chấp nhận
            decimal totalSalary = acceptedApplications.Sum(a => a.Salary);

            // So sánh với ngân sách dự án
            if (totalSalary <= project.Budget)
                return (true, 0);
            else
                return (false, totalSalary - project.Budget);
        }

        public async Task<bool> AddFundsToProjectFromWalletAsync(int projectId, int businessId, decimal amount)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy thông tin dự án
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null || project.BusinessID != businessId)
                    return false;

                // Lấy thông tin doanh nghiệp
                var business = await _context.Users.FindAsync(businessId);
                if (business == null || business.WalletBalance < amount)
                    return false;

                // Trừ tiền từ ví doanh nghiệp
                business.WalletBalance -= amount;

                // Tăng ngân sách dự án
                project.Budget += amount;
                
                // Tăng số dư ví dự án
                project.ProjectWallet += amount;

                // Tạo giao dịch
                // Sử dụng loại giao dịch "Thanh toán" (ID = 3)
                var paymentTypeId = 3; // ID của loại giao dịch "Thanh toán"
                var completedStatusId = _context.TransactionStatuses.FirstOrDefault(s => s.StatusName == "Completed")?.StatusID ?? 1;
                
                var txn = new Transaction
                {
                    UserID = businessId,
                    Amount = amount,
                    Description = $"Bổ sung ngân sách cho dự án: {project.Title}",
                    TransactionDate = DateTime.Now,
                    TypeID = paymentTypeId,
                    StatusID = completedStatusId,
                    ProjectID = projectId,
                    IsActive = true
                };

                _context.Transactions.Add(txn);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> CheckUserHasEnoughFundsAsync(int userId, decimal amount)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null && user.WalletBalance >= amount;
        }

        public async Task<(bool Success, Project Project, string ErrorMessage)> CreateProjectWithTransactionAsync(Project project)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if user has enough funds
                var user = await _context.Users.FindAsync(project.BusinessID);
                if (user == null)
                    return (false, null, "Không tìm thấy thông tin người dùng");
                    
                if (user.WalletBalance < project.Budget)
                    return (false, null, "Số dư ví không đủ để tạo dự án");
                    
                // Create project
                project.CreatedAt = DateTime.UtcNow;
                project.UpdatedAt = DateTime.UtcNow;
                project.IsActive = true;
                project.ProjectWallet = project.Budget; // Initialize project wallet with the same amount as budget
                
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                
                // Create transaction
                var paymentTypeId = 3; // ID của loại giao dịch "Thanh toán"
                var completedStatusId = _context.TransactionStatuses.FirstOrDefault(s => s.StatusName == "Completed")?.StatusID ?? 1;
                
                var txn = new Transaction
                {
                    UserID = project.BusinessID,
                    ProjectID = project.ProjectID,
                    Amount = project.Budget,
                    Description = $"Thanh toán cho dự án: {project.Title}",
                    TransactionDate = DateTime.Now,
                    TypeID = paymentTypeId,
                    StatusID = completedStatusId,
                    IsActive = true
                };

                // Subtract from user wallet
                user.WalletBalance -= project.Budget;
                _context.Users.Update(user);
                
                _context.Transactions.Add(txn);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                return (true, project, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, null, $"Lỗi khi tạo dự án: {ex.Message}");
            }
        }

        public async Task<(bool Success, Project Project, string ErrorMessage)> UpdateProjectWithTransactionAsync(Project project, decimal originalBudget)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Find the existing project
                var existingProject = await _context.Projects.FindAsync(project.ProjectID);
                if (existingProject == null)
                    return (false, null, "Không tìm thấy dự án");
                    
                // Calculate budget difference
                decimal budgetDifference = project.Budget - originalBudget;

                // Check if the new budget is sufficient for all accepted students
                var acceptedApplications = await _context.StudentApplications
                    .Where(a => a.ProjectID == project.ProjectID && a.Status == "Accepted")
                    .ToListAsync();

                decimal totalSalaryForAcceptedStudents = acceptedApplications.Sum(a => a.Salary);
                
                // If new budget is less than total salary needed, prevent the update
                if (project.Budget < totalSalaryForAcceptedStudents)
                    return (false, null, $"Ngân sách mới ({project.Budget:N0} VND) không đủ để trả lương cho các ứng viên đã được chấp nhận ({totalSalaryForAcceptedStudents:N0} VND)");
                
                // If budget is increased, check wallet balance
                if (budgetDifference > 0)
                {
                    var user = await _context.Users.FindAsync(project.BusinessID);
                    if (user == null)
                        return (false, null, "Không tìm thấy thông tin người dùng");
                        
                    if (user.WalletBalance < budgetDifference)
                        return (false, null, "Số dư ví không đủ để tăng ngân sách dự án");
                        
                    // Deduct from wallet
                    user.WalletBalance -= budgetDifference;
                    _context.Users.Update(user);
                    
                    // Create transaction for the additional budget
                    var paymentTypeId = 3; // ID của loại giao dịch "Thanh toán"
                    var completedStatusId = _context.TransactionStatuses.FirstOrDefault(s => s.StatusName == "Completed")?.StatusID ?? 1;
                    
                    var txn = new Transaction
                    {
                        UserID = project.BusinessID,
                        ProjectID = project.ProjectID,
                        Amount = budgetDifference,
                                            Description = $"Bổ sung ngân sách cho dự án: {project.Title}",
                    TransactionDate = DateTime.Now,
                    TypeID = paymentTypeId,
                        StatusID = completedStatusId,
                        IsActive = true
                    };
                    
                    _context.Transactions.Add(txn);
                    
                    // Update project wallet
                    existingProject.ProjectWallet += budgetDifference;
                }
                // If budget is decreased, refund the difference to the user
                else if (budgetDifference < 0)
                {
                    var user = await _context.Users.FindAsync(project.BusinessID);
                    if (user == null)
                        return (false, null, "Không tìm thấy thông tin người dùng");
                    
                    // Add refund to wallet
                    decimal refundAmount = Math.Abs(budgetDifference);
                    user.WalletBalance += refundAmount;
                    _context.Users.Update(user);
                    
                    // Create transaction for the refund
                    var refundTypeId = 4; // Refund transaction type
                    var completedStatusId = _context.TransactionStatuses.FirstOrDefault(s => s.StatusName == "Completed")?.StatusID ?? 1;
                    
                    var refundTxn = new Transaction
                    {
                        UserID = project.BusinessID,
                        ProjectID = project.ProjectID,
                        Amount = refundAmount,
                        Description = $"Hoàn tiền từ giảm ngân sách dự án: {project.Title}",
                        TransactionDate = DateTime.Now,
                        TypeID = refundTypeId,
                        StatusID = completedStatusId,
                        IsActive = true
                    };
                    
                    _context.Transactions.Add(refundTxn);
                    
                    // Update project wallet
                    existingProject.ProjectWallet -= refundAmount;
                }
                
                // Update project properties
                existingProject.Title = project.Title;
                existingProject.Description = project.Description;
                existingProject.Budget = project.Budget;
                existingProject.Deadline = project.Deadline;
                existingProject.StatusID = project.StatusID;
                existingProject.IsHighlighted = project.IsHighlighted;
                existingProject.TypeID = project.TypeID;
                existingProject.AddressID = project.AddressID;
                existingProject.IsRemoteWork = project.IsRemoteWork;
                existingProject.CategoryID = project.CategoryID;
                existingProject.StartDate = project.StartDate;
                existingProject.EndDate = project.EndDate;
                existingProject.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return (true, existingProject, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, null, $"Lỗi khi cập nhật dự án: {ex.Message}");
            }
        }
    }
} 