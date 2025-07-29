using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;
using StudentFreelance.ViewModels;

namespace StudentFreelance.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;

        public ProjectController(
            IProjectService projectService,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            INotificationService notificationService,
            IEmailSender emailSender
        )
        {
            _projectService = projectService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notificationService = notificationService;
            _emailSender = emailSender;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var userId = User.Identity != null && User.Identity.IsAuthenticated ?
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) :
                (int?)null;

            bool includeInactive = false;

            // Kiểm tra quyền để xem dự án không hoạt động
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Moderator"))
                {
                    // Admin và moderator có thể xem tất cả dự án
                    includeInactive = true;
                    userId = null; // Đặt userId = null để logic trong service hiểu đây là admin/mod
                }
                else
                {
                    // Người dùng bình thường chỉ xem được dự án không hoạt động của họ
                    includeInactive = true;
                }
            }

            var projects = await _projectService.GetAllProjectsAsync(includeInactive, userId);

            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return NotFound();

            bool includeInactive = false;
            int? userId = null;

            // Kiểm tra quyền để xem dự án không hoạt động
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Moderator"))
                {
                    includeInactive = true;
                }
                else
                {
                    // Nếu là người dùng bình thường, chỉ xem được dự án không hoạt động của họ
                    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedUserId))
                    {
                        userId = parsedUserId;
                        includeInactive = true;
                    }
                }
            }

            var project = await _projectService.GetProjectByIdAsync(id, includeInactive, userId);

            if (project == null)
                return NotFound();

            // Check if there's an error message that needs to be displayed
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
                // Clear TempData to prevent it from persisting
                TempData.Remove("ErrorMessage");
            }

            // Hiển thị thông báo nếu dự án không hoạt động
            if (!project.IsActive)
            {
                ViewBag.InactiveMessage = "Dự án này đã bị ẩn và chỉ hiển thị cho bạn vì bạn là người tạo dự án hoặc có quyền quản trị.";
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách dữ liệu từ cơ sở dữ liệu
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            var projectStatuses = await _context.ProjectStatuses.Where(s => s.IsActive).ToListAsync();
            var projectTypes = await _context.ProjectTypes.Where(t => t.IsActive).ToListAsync();

            // Kiểm tra xem có dữ liệu không
            if (!categories.Any() || !projectStatuses.Any() || !projectTypes.Any())
            {
                TempData["ErrorMessage"] = "Không thể tạo dự án do thiếu dữ liệu cơ bản. Vui lòng liên hệ quản trị viên.";
                return RedirectToAction("Index");
            }

            // Lấy giá trị mặc định từ cơ sở dữ liệu
            var defaultCategoryId = categories.FirstOrDefault()?.CategoryID ?? 0;
            var defaultStatusId = projectStatuses.FirstOrDefault(s => s.StatusName == "Đang tuyển")?.StatusID ??
                                 projectStatuses.FirstOrDefault()?.StatusID ?? 0;
            var defaultTypeId = projectTypes.FirstOrDefault()?.TypeID ?? 0;

            // Lấy thông tin người dùng hiện tại (doanh nghiệp)
            var businessId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var business = await _context.Users.FirstOrDefaultAsync(u => u.Id == businessId);

            // Lấy địa chỉ của doanh nghiệp
            var businessAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressID == business.AddressID && a.IsActive);
            ViewBag.BusinessAddressId = business?.AddressID;
            ViewBag.BusinessAddresses = businessAddress != null ? new List<Address> { businessAddress } : new List<Address>();

            // Tạo viewModel với giá trị mặc định
            var viewModel = new ProjectViewModel
            {
                Categories = categories,
                ProjectStatuses = projectStatuses,
                ProjectTypes = projectTypes,
                Skills = await _context.Skills.Where(s => s.IsActive).ToListAsync(),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                Deadline = DateTime.Today.AddDays(14),
                BusinessID = businessId,
                CategoryID = defaultCategoryId, // Đặt giá trị mặc định
                StatusID = defaultStatusId, // Đặt giá trị mặc định
                TypeID = defaultTypeId, // Đặt giá trị mặc định
                // Mặc định không sử dụng địa chỉ doanh nghiệp
                AddressID = null
            };

            // Lấy danh sách ImportanceLevel để hiển thị trong dropdown
            ViewBag.ImportanceLevels = await _context.ImportanceLevels.Where(il => il.IsActive).ToListAsync();

            return View(viewModel);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ProjectViewModel viewModel)
        {
            // Kiểm tra giá trị CategoryID
            if (viewModel.CategoryID <= 0)
            {
                ModelState.AddModelError("CategoryID", "Vui lòng chọn danh mục cho dự án.");
                await PopulateFormDataAsync(viewModel);
                return View(viewModel);
            }

            // Xử lý địa chỉ dựa trên checkbox IsRemoteWork
            if (!viewModel.IsRemoteWork)
            {
                // Nếu không phải làm việc từ xa, sử dụng địa chỉ của doanh nghiệp
                var business = await _context.Users.FirstOrDefaultAsync(u => u.Id == viewModel.BusinessID);
                viewModel.AddressID = business?.AddressID;
            }
            else
            {
                // Nếu là làm việc từ xa, địa chỉ sẽ là null
                viewModel.AddressID = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate that the selected CategoryID exists in the database
                    var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryID == viewModel.CategoryID && c.IsActive);
                    if (!categoryExists)
                    {
                        ModelState.AddModelError("CategoryID", "Danh mục đã chọn không tồn tại hoặc không hoạt động.");

                        // Re-populate form data and return to view
                        await PopulateFormDataAsync(viewModel);
                        return View(viewModel);
                    }

                    // Validate that the selected StatusID exists in the database
                    var statusExists = await _context.ProjectStatuses.AnyAsync(s => s.StatusID == viewModel.StatusID && s.IsActive);
                    if (!statusExists)
                    {
                        ModelState.AddModelError("StatusID", "Trạng thái đã chọn không tồn tại hoặc không hoạt động.");

                        // Re-populate form data and return to view
                        await PopulateFormDataAsync(viewModel);
                        return View(viewModel);
                    }

                    // Validate that the selected TypeID exists in the database
                    var typeExists = await _context.ProjectTypes.AnyAsync(t => t.TypeID == viewModel.TypeID && t.IsActive);
                    if (!typeExists)
                    {
                        ModelState.AddModelError("TypeID", "Loại dự án đã chọn không tồn tại hoặc không hoạt động.");

                        // Re-populate form data and return to view
                        await PopulateFormDataAsync(viewModel);
                        return View(viewModel);
                    }

                    // Create the project
                    var project = new Project
                    {
                        BusinessID = viewModel.BusinessID,
                        CategoryID = viewModel.CategoryID,
                        Title = viewModel.Title,
                        Description = viewModel.Description,
                        Budget = viewModel.Budget,
                        Deadline = viewModel.Deadline,
                        StatusID = viewModel.StatusID,
                        IsHighlighted = viewModel.IsHighlighted,
                        TypeID = viewModel.TypeID,
                        AddressID = viewModel.AddressID,
                        IsRemoteWork = viewModel.IsRemoteWork,
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate
                    };

                    Project createdProject;

                    // Check if user has enough money and create project with transaction
                    var result = await _projectService.CreateProjectWithTransactionAsync(project);

                    if (!result.Success)
                    {
                        ModelState.AddModelError("", result.ErrorMessage);
                        await PopulateFormDataAsync(viewModel);
                        return View(viewModel);
                    }

                    createdProject = result.Project;

                    // Add skills
                    if (viewModel.SelectedSkills != null && viewModel.SelectedSkills.Any())
                    {
                        foreach (var skillId in viewModel.SelectedSkills)
                        {
                            // Lấy ImportanceLevelID từ SelectedSkillImportanceLevels nếu có, nếu không thì dùng giá trị mặc định 1
                            int importanceLevelId = 1; // Giá trị mặc định
                            if (viewModel.SelectedSkillImportanceLevels != null && viewModel.SelectedSkillImportanceLevels.ContainsKey(skillId))
                            {
                                importanceLevelId = viewModel.SelectedSkillImportanceLevels[skillId];
                            }

                            var projectSkill = new ProjectSkillRequired
                            {
                                ProjectID = createdProject.ProjectID,
                                SkillID = skillId,
                                ImportanceLevelID = importanceLevelId
                            };
                            _context.ProjectSkillsRequired.Add(projectSkill);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Upload attachments if any (catch riêng lỗi file)
                    if (viewModel.Attachments != null && viewModel.Attachments.Any(f => f != null && f.Length > 0))
                    {
                        try
                        {
                            await SaveAttachments(viewModel.Attachments, createdProject.ProjectID);
                        }
                        catch (Exception ex)
                        {
                            TempData["AttachmentError"] = "Project đã tạo thành công, nhưng có lỗi khi upload file đính kèm.";
                        }
                    }

                    return RedirectToAction(nameof(Details), new { id = createdProject.ProjectID });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã có lỗi khi tạo dự án. Vui lòng thử lại.");
                }
            }
            else
            {
                // Log model validation errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        // Không cần log ra lỗi validation
                    }
                }
            }

            // If we got this far, something failed; redisplay form
            await PopulateFormDataAsync(viewModel);
            return View(viewModel);
        }

        // Helper method to populate form data
        private async Task PopulateFormDataAsync(ProjectViewModel viewModel)
        {
            viewModel.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            viewModel.ProjectStatuses = await _context.ProjectStatuses.Where(s => s.IsActive).ToListAsync();
            viewModel.ProjectTypes = await _context.ProjectTypes.Where(t => t.IsActive).ToListAsync();
            viewModel.Skills = await _context.Skills.Where(s => s.IsActive).ToListAsync();
            ViewBag.ImportanceLevels = await _context.ImportanceLevels.Where(il => il.IsActive).ToListAsync();

            // Get business address ID
            var business = await _context.Users.FirstOrDefaultAsync(u => u.Id == viewModel.BusinessID);
            ViewBag.BusinessAddressId = business?.AddressID;
        }

        // GET: Projects/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return NotFound();

            // Lấy thông tin người dùng hiện tại
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin");

            // Sử dụng context trực tiếp thay vì service để đảm bảo lấy được cả dự án inactive
            var project = await _context.Projects
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
                .FirstOrDefaultAsync(p => p.ProjectID == id);

            if (project == null)
            {
                // Log cho việc debugging
                return NotFound();
            }

            // Check if current user is the owner of the project
            if (project.BusinessID != currentUserId && !isAdmin)
            {
                // Log cho việc debugging
                return Forbid();
            }

            // Get all active addresses of the business
            var business = await _context.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == project.BusinessID);
            ViewBag.BusinessAddresses = business?.Address != null && business.Address.IsActive
                ? new List<Address> { business.Address }
                : new List<Address>();

            var viewModel = new ProjectViewModel
            {
                ProjectID = project.ProjectID,
                BusinessID = project.BusinessID,
                CategoryID = project.CategoryID,
                Title = project.Title,
                Description = project.Description,
                Budget = project.Budget,
                Deadline = project.Deadline,
                StatusID = project.StatusID,
                IsHighlighted = project.IsHighlighted,
                TypeID = project.TypeID,
                AddressID = project.AddressID,
                IsRemoteWork = project.IsRemoteWork,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                IsEdit = true,
                ExistingAttachments = project.ProjectAttachments,
                ExistingSkills = project.ProjectSkillsRequired,
                SelectedSkills = project.ProjectSkillsRequired.Select(ps => ps.SkillID).ToList(),
                // Khởi tạo SelectedSkillImportanceLevels từ các kỹ năng hiện có
                SelectedSkillImportanceLevels = project.ProjectSkillsRequired.ToDictionary(ps => ps.SkillID, ps => ps.ImportanceLevelID),
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                ProjectStatuses = await _context.ProjectStatuses.Where(s => s.IsActive).ToListAsync(),
                ProjectTypes = await _context.ProjectTypes.Where(t => t.IsActive).ToListAsync(),
                Skills = await _context.Skills.Where(s => s.IsActive).ToListAsync(),
                IsActive = project.IsActive
            };

            // Lấy danh sách ImportanceLevel để hiển thị trong dropdown
            ViewBag.ImportanceLevels = await _context.ImportanceLevels.Where(il => il.IsActive).ToListAsync();

            return View(viewModel);
        }

        // GET: Projects/Activate/5
        [Authorize]
        public async Task<IActionResult> Activate(int id)
        {
            if (id <= 0)
                return NotFound();

            // Lấy thông tin người dùng hiện tại
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin");

            // Sử dụng context trực tiếp thay vì service để đảm bảo lấy được cả dự án inactive
            var project = await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.ProjectID == id);

            if (project == null)
            {
                // Log cho việc debugging
                return NotFound();
            }

            // Check if current user is the owner of the project
            if (project.BusinessID != currentUserId && !isAdmin)
            {
                // Log cho việc debugging
                return Forbid();
            }

            return View(project);
        }

        // POST: Projects/Activate/5
        [HttpPost, ActionName("Activate")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            // Lấy thông tin người dùng hiện tại
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin");

            // Lấy thông tin dự án bằng context để đảm bảo có thể truy xuất cả dự án inactive
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                // Log cho việc debugging
                return NotFound();
            }

            // Check if current user is the owner of the project
            if (project.BusinessID != currentUserId && !isAdmin)
            {
                // Log cho việc debugging
                return Forbid();
            }

            await _projectService.ActivateProjectAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, ProjectViewModel viewModel)
        {
            if (id != viewModel.ProjectID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin người dùng hiện tại
                    var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    bool isAdmin = User.IsInRole("Admin");

                    // Lấy thông tin dự án bằng context để đảm bảo có thể truy xuất cả dự án inactive
                    var project = await _context.Projects.FindAsync(id);

                    if (project == null)
                    {
                        return NotFound();
                    }

                    // Check if current user is the owner of the project
                    if (project.BusinessID != currentUserId && !isAdmin)
                    {
                        return Forbid();
                    }

                    // Store the original budget to compare
                    decimal originalBudget = project.Budget;

                    // Create new project object with updated values
                    var updatedProject = new Project
                    {
                        ProjectID = id,
                        BusinessID = project.BusinessID,
                        CategoryID = viewModel.CategoryID,
                        Title = viewModel.Title,
                        Description = viewModel.Description,
                        Budget = viewModel.Budget,
                        Deadline = viewModel.Deadline,
                        StatusID = viewModel.StatusID,
                        IsHighlighted = viewModel.IsHighlighted,
                        TypeID = viewModel.TypeID,
                        AddressID = viewModel.AddressID,
                        IsRemoteWork = viewModel.IsRemoteWork,
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate,
                        CreatedAt = project.CreatedAt,
                        IsActive = project.IsActive
                    };

                    // Update project with transaction if budget changed
                    var result = await _projectService.UpdateProjectWithTransactionAsync(updatedProject, originalBudget);

                    if (!result.Success)
                    {
                        ModelState.AddModelError("", result.ErrorMessage);

                        // Re-populate form data
                        viewModel.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                        viewModel.ProjectStatuses = await _context.ProjectStatuses.Where(s => s.IsActive).ToListAsync();
                        viewModel.ProjectTypes = await _context.ProjectTypes.Where(t => t.IsActive).ToListAsync();
                        viewModel.Skills = await _context.Skills.Where(s => s.IsActive).ToListAsync();
                        ViewBag.ImportanceLevels = await _context.ImportanceLevels.Where(il => il.IsActive).ToListAsync();
                        viewModel.ExistingAttachments = await _context.ProjectAttachments
                            .Where(pa => pa.ProjectID == id)
                            .ToListAsync();
                        viewModel.ExistingSkills = await _context.ProjectSkillsRequired
                            .Include(ps => ps.Skill)
                            .Where(ps => ps.ProjectID == id)
                            .ToListAsync();

                        return View(viewModel);
                    }

                    // Get the updated project
                    project = result.Project;

                    // Update skills
                    var existingSkills = await _context.ProjectSkillsRequired
                        .Where(ps => ps.ProjectID == id)
                        .ToListAsync();

                    // Remove skills that are no longer selected
                    if (viewModel.SelectedSkills != null)
                    {
                        var skillsToRemove = existingSkills
                            .Where(ps => !viewModel.SelectedSkills.Contains(ps.SkillID))
                            .ToList();

                        foreach (var skill in skillsToRemove)
                        {
                            _context.ProjectSkillsRequired.Remove(skill);
                        }

                        // Add new skills
                        foreach (var skillId in viewModel.SelectedSkills)
                        {
                            if (!existingSkills.Any(ps => ps.SkillID == skillId))
                            {
                                // Lấy ImportanceLevelID từ SelectedSkillImportanceLevels nếu có, nếu không thì dùng giá trị mặc định 1
                                int importanceLevelId = 1; // Giá trị mặc định
                                if (viewModel.SelectedSkillImportanceLevels != null && viewModel.SelectedSkillImportanceLevels.ContainsKey(skillId))
                                {
                                    importanceLevelId = viewModel.SelectedSkillImportanceLevels[skillId];
                                }

                                var projectSkill = new ProjectSkillRequired
                                {
                                    ProjectID = id,
                                    SkillID = skillId,
                                    ImportanceLevelID = importanceLevelId
                                };
                                _context.ProjectSkillsRequired.Add(projectSkill);
                            }
                            // Cập nhật ImportanceLevelID cho các kỹ năng đã tồn tại nếu có thay đổi
                            else if (viewModel.SelectedSkillImportanceLevels != null && viewModel.SelectedSkillImportanceLevels.ContainsKey(skillId))
                            {
                                var existingSkill = existingSkills.FirstOrDefault(ps => ps.SkillID == skillId);
                                if (existingSkill != null && existingSkill.ImportanceLevelID != viewModel.SelectedSkillImportanceLevels[skillId])
                                {
                                    existingSkill.ImportanceLevelID = viewModel.SelectedSkillImportanceLevels[skillId];
                                    _context.ProjectSkillsRequired.Update(existingSkill);
                                }
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    // Upload new attachments if any (catch riêng lỗi file)
                    if (viewModel.Attachments != null && viewModel.Attachments.Any(f => f != null && f.Length > 0))
                    {
                        try
                        {
                            await SaveAttachments(viewModel.Attachments, id);
                        }
                        catch (Exception ex)
                        {
                            TempData["AttachmentError"] = "Project đã cập nhật thành công, nhưng có lỗi khi upload file đính kèm.";
                        }
                    }

                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjectExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã có lỗi khi cập nhật dự án. Vui lòng thử lại.");
                }
            }

            // If we got this far, something failed; redisplay form
            viewModel.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            viewModel.ProjectStatuses = await _context.ProjectStatuses.Where(s => s.IsActive).ToListAsync();
            viewModel.ProjectTypes = await _context.ProjectTypes.Where(t => t.IsActive).ToListAsync();
            viewModel.Skills = await _context.Skills.Where(s => s.IsActive).ToListAsync();
            ViewBag.ImportanceLevels = await _context.ImportanceLevels.Where(il => il.IsActive).ToListAsync();
            viewModel.ExistingAttachments = await _context.ProjectAttachments
                .Where(pa => pa.ProjectID == id)
                .ToListAsync();
            viewModel.ExistingSkills = await _context.ProjectSkillsRequired
                .Include(ps => ps.Skill)
                .Where(ps => ps.ProjectID == id)
                .ToListAsync();

            return View(viewModel);
        }

        // GET: Projects/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return NotFound();

            // Sử dụng context trực tiếp để lấy dự án
            var project = await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.ProjectID == id);

            if (project == null)
                return NotFound();

            // Check if current user is the owner of the project or has admin/moderator role
            if (project.BusinessID != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) 
                && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound();

            // Check if current user is the owner of the project or has admin/moderator role
            if (project.BusinessID != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) 
                && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            await _projectService.DeleteProjectAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: Projects/DeleteAttachment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteAttachment(int attachmentId, int projectId)
        {
            var attachment = await _context.ProjectAttachments.FindAsync(attachmentId);

            if (attachment == null)
                return NotFound();

            var project = await _context.Projects.FindAsync(projectId);

            if (project == null)
                return NotFound();

            // Check if current user is the owner of the project or has admin/moderator role
            if (project.BusinessID != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) 
                && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            // Delete the file from the server
            if (!string.IsNullOrEmpty(attachment.FilePath))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, attachment.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.ProjectAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = projectId });
        }

        // GET: Projects/TestAccess/5
        [Authorize]
        public async Task<IActionResult> TestAccess(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin");

            // Test với ProjectService
            var projectFromService = await _projectService.GetProjectByIdAsync(id, true, currentUserId);

            // Test với context trực tiếp
            var projectFromContext = await _context.Projects.FindAsync(id);

            var result = new
            {
                ProjectID = id,
                CurrentUserID = currentUserId,
                IsAdmin = isAdmin,
                ProjectFromServiceFound = projectFromService != null,
                ProjectFromContextFound = projectFromContext != null,
                ProjectFromService = projectFromService != null ? new
                {
                    IsActive = projectFromService.IsActive,
                    BusinessID = projectFromService.BusinessID,
                    Title = projectFromService.Title
                } : null,
                ProjectFromContext = projectFromContext != null ? new
                {
                    IsActive = projectFromContext.IsActive,
                    BusinessID = projectFromContext.BusinessID,
                    Title = projectFromContext.Title
                } : null
            };

            return Json(result);
        }

        // POST: Projects/ConfirmCompletion
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ConfirmCompletion(int projectId, int applicationId)
        {
            if (projectId <= 0 || applicationId <= 0)
                return NotFound();

            // Get current user ID
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get project and application
            var project = await _context.Projects.FindAsync(projectId);
            var application = await _context.StudentApplications.FindAsync(applicationId);

            if (project == null || application == null)
                return NotFound();

            if (application.ProjectID != projectId)
                return NotFound();

            // Check if user is authorized (either business owner, the student, admin or moderator)
            bool isBusinessConfirmation = false;
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (project.BusinessID == currentUserId || isAdmin)
            {
                isBusinessConfirmation = true;
            }
            else if (application.UserID == currentUserId)
            {
                isBusinessConfirmation = false;
            }
            else
            {
                // User is neither the business owner, admin, moderator nor the student
                return Forbid();
            }

            try
            {
                // Call the service to confirm completion
                var result = await _projectService.ConfirmProjectCompletionAsync(projectId, applicationId, isBusinessConfirmation);

                if (result)
                {
                    // Gửi thông báo theo người xác nhận
                    var business = await _context.Users.FindAsync(project.BusinessID);
                    var student = await _context.Users.FindAsync(application.UserID);

                    if (isBusinessConfirmation)
                    {
                        TempData["SuccessMessage"] = "Bạn đã xác nhận hoàn thành dự án. Đang chờ xác nhận từ sinh viên.";

                        if (business != null && student != null)
                        {
                            await _notificationService.SendNotificationToUserAsync(
                                student.Id,
                                "Doanh nghiệp xác nhận hoàn thành dự án",
                                $"Doanh nghiệp đã xác nhận hoàn thành dự án '{project.Title}'.",
                                1,
                                project.ProjectID,
                                business.Id,
                                true // gửi email
                            );
                        }
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Bạn đã xác nhận hoàn thành dự án. Đang chờ xác nhận từ doanh nghiệp.";

                        if (business != null && student != null)
                        {
                            await _notificationService.SendNotificationToUserAsync(
                                business.Id,
                                "Sinh viên xác nhận hoàn thành dự án",
                                $"Sinh viên {student.FullName} đã xác nhận hoàn thành dự án '{project.Title}'.",
                                1,
                                project.ProjectID,
                                student.Id,
                                true // gửi email
                            );
                        }
                    }

                    if (application.BusinessConfirmedCompletion && application.StudentConfirmedCompletion)
                    {
                        TempData["SuccessMessage"] = "Dự án đã được xác nhận hoàn thành bởi cả hai bên.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xác nhận hoàn thành dự án.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }


        // POST: Projects/CancelConfirmation
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CancelConfirmation(int projectId, int applicationId)
        {
            if (projectId <= 0 || applicationId <= 0)
                return NotFound();

            // Get current user ID
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get project and application
            var project = await _context.Projects.FindAsync(projectId);
            var application = await _context.StudentApplications.FindAsync(applicationId);

            if (project == null || application == null)
                return NotFound();

            if (application.ProjectID != projectId)
                return NotFound();

            // Check if user is authorized (either business owner, the student, admin or moderator)
            bool isBusinessCancellation = false;
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (project.BusinessID == currentUserId || isAdmin)
            {
                isBusinessCancellation = true;

                // Check if business had confirmed
                if (!application.BusinessConfirmedCompletion)
                {
                    TempData["ErrorMessage"] = "Bạn chưa xác nhận hoàn thành dự án.";
                    return RedirectToAction(nameof(Details), new { id = projectId });
                }

                // Cancel business confirmation
                application.BusinessConfirmedCompletion = false;
            }
            else if (application.UserID == currentUserId)
            {
                isBusinessCancellation = false;

                // Check if student had confirmed
                if (!application.StudentConfirmedCompletion)
                {
                    TempData["ErrorMessage"] = "Bạn chưa xác nhận hoàn thành dự án.";
                    return RedirectToAction(nameof(Details), new { id = projectId });
                }

                // Cancel student confirmation
                application.StudentConfirmedCompletion = false;
            }
            else
            {
                // User is neither the business owner, admin, moderator nor the student
                return Forbid();
            }

            // Nếu cả hai bên đều chưa xác nhận và trạng thái là PendingReview, chuyển lại thành InProgress
            // Không thay đổi trạng thái nếu đã là Completed
            if (!application.BusinessConfirmedCompletion && !application.StudentConfirmedCompletion &&
                application.Status == "PendingReview")
            {
                application.Status = "InProgress";
            }

            try
            {
                await _context.SaveChangesAsync();

                if (isBusinessCancellation)
                {
                    TempData["SuccessMessage"] = "Bạn đã huỷ xác nhận hoàn thành dự án.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Bạn đã huỷ xác nhận hoàn thành dự án.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        // POST: Projects/TransferPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Business,Admin")]
        public async Task<IActionResult> TransferPayment(int projectId, int applicationId)
        {
            // Chuyển hướng người dùng đến trang ProjectSubmission/ProjectSubmissions để thanh toán
            TempData["InfoMessage"] = "Vui lòng sử dụng trang kết quả dự án để thanh toán cho sinh viên.";
            return RedirectToAction("ProjectSubmissions", "ProjectSubmission", new { applicationId });
        }

        // POST: Projects/CompleteProject
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> CompleteProject(int id)
        {
            if (id <= 0)
                return NotFound();

            // Get current user ID
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get project
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            // Check if user is authorized (business owner or admin/moderator)
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");
            if (project.BusinessID != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            try
            {
                // Complete the project
                var result = await _projectService.CompleteProjectByBusinessAsync(id, isAdmin ? project.BusinessID : currentUserId);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Dự án đã được đánh dấu là hoàn thành thành công.";

                    // Gửi thông báo cho tất cả sinh viên đã tham gia dự án
                    var completedApplications = await _context.StudentApplications
                        .Where(a => a.ProjectID == id && a.Status == "Completed")
                        .Include(a => a.User)
                        .ToListAsync();

                    foreach (var application in completedApplications)
                    {
                        await _notificationService.SendNotificationToUserAsync(
                            application.UserID,
                            "Dự án đã hoàn thành",
                            $"Dự án '{project.Title}' đã được đánh dấu là hoàn thành bởi doanh nghiệp.",
                            1, // Loại thông báo: Dự án
                            project.ProjectID,
                            project.BusinessID,
                            true // Gửi email
                        );
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Projects/CancelProject
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> CancelProject(int id)
        {
            if (id <= 0)
                return NotFound();

            // Get current user ID
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get project
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            // Check if user is authorized (business owner or admin/moderator)
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");
            if (project.BusinessID != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            try
            {
                // Cancel the project
                var result = await _projectService.CancelProjectByBusinessAsync(id, isAdmin ? project.BusinessID : currentUserId);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Dự án đã được hủy thành công.";

                    // Gửi thông báo cho tất cả sinh viên đã tham gia dự án
                    var applications = await _context.StudentApplications
                        .Where(a => a.ProjectID == id)
                        .Include(a => a.User)
                        .ToListAsync();

                    foreach (var application in applications)
                    {
                        await _notificationService.SendNotificationToUserAsync(
                            application.UserID,
                            "Dự án đã bị hủy",
                            $"Dự án '{project.Title}' đã bị hủy bởi doanh nghiệp.",
                            1, // Loại thông báo: Dự án
                            project.ProjectID,
                            project.BusinessID,
                            true // Gửi email
                        );
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Projects/Debug
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Debug()
        {
            // Check if the required tables and values exist
            var result = new Dictionary<string, object>();

            // Check Categories
            result["Categories"] = await _context.Categories.Where(c => c.IsActive).Select(c => new { c.CategoryID, c.CategoryName }).ToListAsync();

            // Check ProjectStatuses
            result["ProjectStatuses"] = await _context.ProjectStatuses.Where(s => s.IsActive).Select(s => new { s.StatusID, s.StatusName }).ToListAsync();

            // Check ProjectTypes
            result["ProjectTypes"] = await _context.ProjectTypes.Where(t => t.IsActive).Select(t => new { t.TypeID, t.TypeName }).ToListAsync();

            // Check ImportanceLevels
            result["ImportanceLevels"] = await _context.ImportanceLevels.Where(i => i.IsActive).Select(i => new { i.LevelID, i.LevelName }).ToListAsync();

            // Check Skills
            result["Skills"] = await _context.Skills.Where(s => s.IsActive).Select(s => new { s.SkillID, s.SkillName }).ToListAsync();

            // Check database connection
            result["DbConnectionStatus"] = "Connected";

            // Check if a test project can be created
            try
            {
                var testProject = new Project
                {
                    BusinessID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    CategoryID = await _context.Categories.Where(c => c.IsActive).Select(c => c.CategoryID).FirstOrDefaultAsync(),
                    Title = "Test Project - " + Guid.NewGuid().ToString(),
                    Description = "This is a test project",
                    Budget = 1000,
                    Deadline = DateTime.Today.AddDays(30),
                    StatusID = await _context.ProjectStatuses.Where(s => s.IsActive).Select(s => s.StatusID).FirstOrDefaultAsync(),
                    IsHighlighted = false,
                    TypeID = await _context.ProjectTypes.Where(t => t.IsActive).Select(t => t.TypeID).FirstOrDefaultAsync(),
                    IsRemoteWork = true,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(30),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Don't actually save, just validate
                _context.Projects.Add(testProject);
                // Can't reload an entity that hasn't been saved yet
                // await _context.Entry(testProject).ReloadAsync();
                _context.Projects.Remove(testProject);

                result["TestProjectValidation"] = "Success";
            }
            catch (Exception ex)
            {
                result["TestProjectValidation"] = "Failed";
                result["TestProjectError"] = ex.Message;
                if (ex.InnerException != null)
                {
                    result["TestProjectInnerError"] = ex.InnerException.Message;
                }
            }

            return Json(result);
        }

        // GET: Projects/ListCategories
        [Authorize]
        public async Task<IActionResult> ListCategories()
        {
            var allCategories = await _context.Categories.ToListAsync();
            var activeCategories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            return Json(new
            {
                AllCategories = allCategories.Select(c => new { c.CategoryID, c.CategoryName, c.IsActive }),
                ActiveCategories = activeCategories.Select(c => new { c.CategoryID, c.CategoryName })
            });
        }

        // GET: Projects/CheckCategory/{id}
        [Authorize]
        public async Task<IActionResult> CheckCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return Json(new { exists = false, isActive = false, message = "Danh mục không tồn tại." });
            }

            return Json(new
            {
                exists = true,
                isActive = category.IsActive,
                message = category.IsActive ? "Danh mục hợp lệ." : "Danh mục không hoạt động."
            });
        }

        // GET: Projects/SeedCategories
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedCategories()
        {
            // Check if categories already exist
            if (await _context.Categories.AnyAsync())
            {
                return Json(new { success = false, message = "Categories already exist." });
            }

            // Add default categories
            var categories = new List<Category>
            {
                new Category { CategoryName = "Web Development", CategoryType = "Project", IsActive = true },
                new Category { CategoryName = "Mobile Development", CategoryType = "Project", IsActive = true },
                new Category { CategoryName = "Design", CategoryType = "Project", IsActive = true },
                new Category { CategoryName = "Writing", CategoryType = "Project", IsActive = true },
                new Category { CategoryName = "Marketing", CategoryType = "Project", IsActive = true },
                new Category { CategoryName = "Data Analysis", CategoryType = "Project", IsActive = true }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Categories seeded successfully." });
        }

        // GET: Projects/SeedProjectStatuses
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedProjectStatuses()
        {
            // Check if project statuses already exist
            if (await _context.ProjectStatuses.AnyAsync())
            {
                return Json(new { success = false, message = "Project statuses already exist." });
            }

            // Add default project statuses
            var statuses = new List<ProjectStatus>
            {
                new ProjectStatus { StatusID = 1, StatusName = "Đang tuyển", IsActive = true },
                new ProjectStatus { StatusID = 2, StatusName = "Đang thực hiện", IsActive = true },
                new ProjectStatus { StatusID = 3, StatusName = "Hoàn thành", IsActive = true },
                new ProjectStatus { StatusID = 4, StatusName = "Đã hủy", IsActive = true }
            };

            await _context.ProjectStatuses.AddRangeAsync(statuses);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Project statuses seeded successfully." });
        }

        // GET: Projects/SeedProjectTypes
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedProjectTypes()
        {
            // Check if project types already exist
            if (await _context.ProjectTypes.AnyAsync())
            {
                return Json(new { success = false, message = "Project types already exist." });
            }

            // Add default project types
            var types = new List<ProjectType>
            {
                new ProjectType { TypeID = 1, TypeName = "Toàn thời gian", IsActive = true },
                new ProjectType { TypeID = 2, TypeName = "Bán thời gian", IsActive = true },
                new ProjectType { TypeID = 3, TypeName = "Theo dự án", IsActive = true }
            };

            await _context.ProjectTypes.AddRangeAsync(types);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Project types seeded successfully." });
        }

        // GET: Projects/SeedImportanceLevels
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedImportanceLevels()
        {
            // Check if importance levels already exist
            if (await _context.ImportanceLevels.AnyAsync())
            {
                return Json(new { success = false, message = "Importance levels already exist." });
            }

            // Add default importance levels
            var levels = new List<ImportanceLevel>
            {
                new ImportanceLevel { LevelID = 1, LevelName = "Thấp", IsActive = true },
                new ImportanceLevel { LevelID = 2, LevelName = "Trung bình", IsActive = true },
                new ImportanceLevel { LevelID = 3, LevelName = "Cao", IsActive = true },
                new ImportanceLevel { LevelID = 4, LevelName = "Rất cao", IsActive = true }
            };

            await _context.ImportanceLevels.AddRangeAsync(levels);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Importance levels seeded successfully." });
        }

        // GET: Projects/FlagProject/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> FlagProject(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id, true);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> FlagProject(int id, string reason)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Check if already flagged
            if (project.IsFlagged)
            {
                TempData["ErrorMessage"] = "Project is already flagged.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Update project flag status
            project.IsFlagged = true;
            project.FlagReason = reason;
            project.FlaggedAt = DateTime.UtcNow;
            project.FlaggedByID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Log the action
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var action = new ProjectFlagAction
            {
                ProjectID = project.ProjectID,
                ActionByID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                ActionType = "Flag",
                Reason = reason,
                ActionDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.ProjectFlagActions.Add(action);
            await _context.SaveChangesAsync();

            // Notify the project owner
            var notification = new Notification
            {
                Title = "Project Flagged",
                Content = $"Your project '{project.Title}' has been flagged for review due to: {reason}",
                TypeID = _context.NotificationTypes.First(t => t.TypeName == "Hệ thống").TypeID,
                NotificationDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _context.UserNotifications.Add(new UserNotification
            {
                UserID = project.BusinessID,
                NotificationID = notification.NotificationID,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Project successfully flagged.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> UnflagProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Check if not flagged
            if (!project.IsFlagged)
            {
                TempData["ErrorMessage"] = "Project is not flagged.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Update project flag status
            project.IsFlagged = false;
            project.FlagReason = null;
            project.FlaggedAt = null;
            project.FlaggedByID = null;

            // Log the action
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var action = new ProjectFlagAction
            {
                ProjectID = project.ProjectID,
                ActionByID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                ActionType = "Unflag",
                Reason = "Flag removed",
                ActionDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.ProjectFlagActions.Add(action);
            await _context.SaveChangesAsync();

            // Notify the project owner
            var notification = new Notification
            {
                Title = "Project Flag Removed",
                Content = $"The flag on your project '{project.Title}' has been removed.",
                TypeID = _context.NotificationTypes.First(t => t.TypeName == "Hệ thống").TypeID,
                NotificationDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _context.UserNotifications.Add(new UserNotification
            {
                UserID = project.BusinessID,
                NotificationID = notification.NotificationID,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Project flag successfully removed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Helper methods
        private async Task<bool> ProjectExists(int id)
        {
            return await _context.Projects.AnyAsync(p => p.ProjectID == id);
        }

        private async Task SaveAttachments(List<IFormFile> files, int projectId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "projects", projectId.ToString());

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Generate a unique filename
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Create the attachment record
                    var attachment = new ProjectAttachment
                    {
                        ProjectID = projectId,
                        FileName = file.FileName,
                        FilePath = "/uploads/projects/" + projectId + "/" + fileName,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        UploadedBy = userId,
                        UploadedAt = DateTime.UtcNow
                    };

                    _context.ProjectAttachments.Add(attachment);
                }
            }

            await _context.SaveChangesAsync();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> InviteStudents(int projectId)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.ProjectSkillsRequired)
                    .ThenInclude(ps => ps.Skill)
                    .Include(p => p.Business)
                    .FirstOrDefaultAsync(p => p.ProjectID == projectId);

                if (project == null)
                {
                    return NotFound();
                }

                // Kiểm tra quyền - chỉ chủ dự án mới được mời sinh viên
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (project.BusinessID != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Lấy danh sách kỹ năng yêu cầu
                var requiredSkillIds = project.ProjectSkillsRequired.Select(ps => ps.SkillID).ToList();

                if (!requiredSkillIds.Any())
                {
                    // Nếu dự án không có kỹ năng yêu cầu, trả về danh sách rỗng
                    ViewBag.Project = project;
                    return View(new List<InviteStudentViewModel>());
                }

                // Tìm sinh viên có kỹ năng phù hợp - sử dụng cách tiếp cận đơn giản hơn
                var studentSkills = await _context.StudentSkills
                    .Include(ss => ss.User)
                    .Include(ss => ss.Skill)
                    .Where(ss => requiredSkillIds.Contains(ss.SkillID) && ss.IsActive)
                    .ToListAsync();

                // Nhóm theo user và xử lý trong memory
                var studentGroups = studentSkills
                    .GroupBy(ss => ss.UserID)
                    .Where(g => g.First().User.IsActive) // Chỉ lấy user đang hoạt động
                    .Select(g => new
                    {
                        User = g.First().User,
                        MatchedSkills = g.Select(ss => ss.Skill).ToList(),
                        SkillMatchCount = g.Count(),
                        IsVip = g.First().User.VipStatus,
                        VipExpiryDate = g.First().User.VipExpiryDate
                    })
                    .OrderByDescending(s => s.SkillMatchCount)
                    .ThenByDescending(s => s.IsVip)
                    .ThenBy(s => s.VipExpiryDate)
                    .ToList();

                var sortedStudents = studentGroups.Select(s => new InviteStudentViewModel
                {
                    UserID = s.User.Id,
                    FullName = s.User.FullName ?? "Không có tên",
                    Email = s.User.Email ?? "",
                    Avatar = s.User.Avatar,
                    University = s.User.University ?? "",
                    Major = s.User.Major ?? "",
                    IsVip = s.IsVip,
                    VipExpiryDate = s.VipExpiryDate,
                    MatchedSkills = s.MatchedSkills,
                    SkillMatchCount = s.SkillMatchCount,
                    TotalRequiredSkills = requiredSkillIds.Count,
                    MatchPercentage = requiredSkillIds.Count > 0 ? (double)s.SkillMatchCount / requiredSkillIds.Count * 100 : 0
                }).ToList();

                ViewBag.Project = project;
                return View(sortedStudents);
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                Console.WriteLine($"Error in InviteStudents: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi tải danh sách sinh viên: {ex.Message}";
                return RedirectToAction("Details", new { id = projectId });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendInvitation(int projectId, int studentId)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Business)
                    .FirstOrDefaultAsync(p => p.ProjectID == projectId);

                var student = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == studentId);

                if (project == null || student == null)
                {
                    return NotFound();
                }

                // Kiểm tra quyền
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (project.BusinessID != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Tạo URL dự án
                var projectUrl = $"{Request.Scheme}://{Request.Host}/Project/Details/{projectId}";

                // Gửi thông báo cho sinh viên
                await _notificationService.SendNotificationToUserAsync(
                    studentId,
                    "Thư mời tham gia dự án",
                    $"Doanh nghiệp {project.Business.FullName} đã gửi thư mời tham gia dự án \"{project.Title}\" cho bạn. <a href='{projectUrl}' style='color: #89AC46; text-decoration: underline; font-weight: bold;'>Xem chi tiết dự án</a>",
                    2, // NotificationType cho Project
                    projectId,
                    currentUserId,
                    true // Gửi email
                );

                // Gửi email chi tiết
                var subject = $"Thư mời tham gia dự án: {project.Title}";
                var body = $@"
                    <h3>Xin chào {student.FullName},</h3>
                    <p>Doanh nghiệp <strong>{project.Business.FullName}</strong> đã gửi thư mời tham gia dự án cho bạn.</p>
                    <h4>Thông tin dự án:</h4>
                    <ul>
                        <li><strong>Tên dự án:</strong> {project.Title}</li>
                        <li><strong>Mô tả:</strong> {project.Description}</li>
                        <li><strong>Ngân sách:</strong> {project.Budget:C}</li>
                        <li><strong>Thời hạn:</strong> {project.Deadline:dd/MM/yyyy}</li>
                    </ul>
                    <p>Nếu bạn quan tâm, hãy truy cập link bên dưới để xem chi tiết và ứng tuyển:</p>
                    <p><a href='{projectUrl}' style='display: inline-block; padding: 10px 20px; background-color: #89AC46; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>Xem chi tiết dự án</a></p>
                    <p>Hoặc copy link này: <a href='{projectUrl}'>{projectUrl}</a></p>
                    <p>Trân trọng,<br>StudentFreelance Team</p>";

                await _emailSender.SendEmailAsync(student.Email, subject, body);

                TempData["SuccessMessage"] = $"Đã gửi thư mời thành công cho {student.FullName}";
                return RedirectToAction("InviteStudents", new { projectId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi thư mời";
                return RedirectToAction("InviteStudents", new { projectId });
            }
        }

        [Authorize(Roles = "Business")]
        public async Task<IActionResult> MyProjects()
        {
            var currentUserId = int.Parse(System.Security.Claims.ClaimTypes.NameIdentifier);
            var projects = await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Where(p => p.BusinessID == currentUserId)
                .OrderByDescending(p => p.ProjectID)
                .ToListAsync();
            return View("MyProjects", projects);
        }
    }
}