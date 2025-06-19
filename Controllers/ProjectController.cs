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
using StudentFreelance.Interfaces;
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

        public ProjectController(
            IProjectService projectService, 
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _projectService = projectService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
            var viewModel = new ProjectViewModel
            {
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                ProjectStatuses = await _context.ProjectStatuses.Where(s => s.IsActive).ToListAsync(),
                ProjectTypes = await _context.ProjectTypes.Where(t => t.IsActive).ToListAsync(),
                Skills = await _context.Skills.Where(s => s.IsActive).ToListAsync(),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                Deadline = DateTime.Today.AddDays(14),
                BusinessID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                StatusID = 1 // Mặc định là "Đang tuyển"
            };

            return View(viewModel);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ProjectViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
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

                    var createdProject = await _projectService.CreateProjectAsync(project);

                    // Add skills
                    if (viewModel.SelectedSkills != null && viewModel.SelectedSkills.Any())
                    {
                        foreach (var skillId in viewModel.SelectedSkills)
                        {
                            var projectSkill = new ProjectSkillRequired
                            {
                                ProjectID = createdProject.ProjectID,
                                SkillID = skillId
                            };
                            _context.ProjectSkillsRequired.Add(projectSkill);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Upload attachments if any
                    if (viewModel.Attachments != null && viewModel.Attachments.Any(f => f != null && f.Length > 0))
                    {
                        await SaveAttachments(viewModel.Attachments, createdProject.ProjectID);
                    }

                    return RedirectToAction(nameof(Details), new { id = createdProject.ProjectID });
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while creating the project. Please try again.");
                }
            }

            // If we got this far, something failed; redisplay form
            viewModel.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            viewModel.ProjectStatuses = await _context.ProjectStatuses.Where(s => s.IsActive).ToListAsync();
            viewModel.ProjectTypes = await _context.ProjectTypes.Where(t => t.IsActive).ToListAsync();
            viewModel.Skills = await _context.Skills.Where(s => s.IsActive).ToListAsync();
            
            return View(viewModel);
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
                Console.WriteLine($"Không tìm thấy dự án với ID {id} cho người dùng {currentUserId}, isAdmin: {isAdmin}");
                return NotFound();
            }

            // Check if current user is the owner of the project
            if (project.BusinessID != currentUserId && !isAdmin)
            {
                // Log cho việc debugging
                Console.WriteLine($"Không có quyền chỉnh sửa. BusinessID: {project.BusinessID}, CurrentUserId: {currentUserId}, isAdmin: {isAdmin}");
                return Forbid();
            }

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
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                ProjectStatuses = await _context.ProjectStatuses.Where(s => s.IsActive).ToListAsync(),
                ProjectTypes = await _context.ProjectTypes.Where(t => t.IsActive).ToListAsync(),
                Skills = await _context.Skills.Where(s => s.IsActive).ToListAsync(),
                IsActive = project.IsActive
            };

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
                Console.WriteLine($"Không tìm thấy dự án với ID {id} cho người dùng {currentUserId}, isAdmin: {isAdmin}");
                return NotFound();
            }

            // Check if current user is the owner of the project
            if (project.BusinessID != currentUserId && !isAdmin)
            {
                // Log cho việc debugging
                Console.WriteLine($"Không có quyền kích hoạt. BusinessID: {project.BusinessID}, CurrentUserId: {currentUserId}, isAdmin: {isAdmin}");
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
                Console.WriteLine($"Không tìm thấy dự án với ID {id}");
                return NotFound();
            }

            // Check if current user is the owner of the project
            if (project.BusinessID != currentUserId && !isAdmin)
            {
                // Log cho việc debugging
                Console.WriteLine($"Không có quyền kích hoạt. BusinessID: {project.BusinessID}, CurrentUserId: {currentUserId}, isAdmin: {isAdmin}");
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
                        // Log cho việc debugging
                        Console.WriteLine($"Không tìm thấy dự án với ID {id}");
                        return NotFound();
                    }

                    // Check if current user is the owner of the project
                    if (project.BusinessID != currentUserId && !isAdmin)
                    {
                        // Log cho việc debugging
                        Console.WriteLine($"Không có quyền chỉnh sửa. BusinessID: {project.BusinessID}, CurrentUserId: {currentUserId}, isAdmin: {isAdmin}");
                        return Forbid();
                    }

                    // Update project properties
                    project.CategoryID = viewModel.CategoryID;
                    project.Title = viewModel.Title;
                    project.Description = viewModel.Description;
                    project.Budget = viewModel.Budget;
                    project.Deadline = viewModel.Deadline;
                    project.StatusID = viewModel.StatusID;
                    project.IsHighlighted = viewModel.IsHighlighted;
                    project.TypeID = viewModel.TypeID;
                    project.AddressID = viewModel.AddressID;
                    project.IsRemoteWork = viewModel.IsRemoteWork;
                    project.StartDate = viewModel.StartDate;
                    project.EndDate = viewModel.EndDate;

                    await _projectService.UpdateProjectAsync(project);

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
                                var projectSkill = new ProjectSkillRequired
                                {
                                    ProjectID = id,
                                    SkillID = skillId
                                };
                                _context.ProjectSkillsRequired.Add(projectSkill);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    // Upload new attachments if any
                    if (viewModel.Attachments != null && viewModel.Attachments.Any(f => f != null && f.Length > 0))
                    {
                        await SaveAttachments(viewModel.Attachments, id);
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
                    // Log exception for debugging
                    Console.WriteLine($"Error updating project: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the project. Please try again.");
                }
            }

            // If we got this far, something failed; redisplay form
            viewModel.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            viewModel.ProjectStatuses = await _context.ProjectStatuses.Where(s => s.IsActive).ToListAsync();
            viewModel.ProjectTypes = await _context.ProjectTypes.Where(t => t.IsActive).ToListAsync();
            viewModel.Skills = await _context.Skills.Where(s => s.IsActive).ToListAsync();
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

            // Check if current user is the owner of the project
            if (project.BusinessID != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) && !User.IsInRole("Admin"))
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

            // Check if current user is the owner of the project
            if (project.BusinessID != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) && !User.IsInRole("Admin"))
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

            // Check if current user is the owner of the project
            if (project.BusinessID != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) && !User.IsInRole("Admin"))
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
    }
} 