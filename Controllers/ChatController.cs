using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.ViewModels;

namespace StudentFreelance.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userMgr;

        public ChatController(ApplicationDbContext db, UserManager<ApplicationUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
        }
     

        // POST: /Chat/StartProjectChat
        [HttpPost]
        public async Task<IActionResult> StartProjectChat(int projectId)
        {
            var userId = int.Parse(_userMgr.GetUserId(User));
            if (!User.IsInRole("Student")) return Forbid();

            var proj = await _db.Projects.FindAsync(projectId);
            if (proj == null) return NotFound();

            var bizId = proj.BusinessID;

            // Tìm hoặc tạo cuộc hội thoại
            var conv = await _db.Conversations.FirstOrDefaultAsync(c =>
                c.ProjectID == projectId &&
                ((c.ParticipantAID == userId && c.ParticipantBID == bizId) ||
                 (c.ParticipantBID == userId && c.ParticipantAID == bizId))
            );

            if (conv == null)
            {
                conv = new Conversation
                {
                    ProjectID = projectId,
                    ParticipantAID = userId,
                    ParticipantBID = bizId
                };
                _db.Conversations.Add(conv);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Chat", new { conversationId = conv.ConversationID });

        }

       

        // GET
        [HttpGet("Chat/Room/User/{userId}")]
        public async Task<IActionResult> StartUserChat(int userId)
        {
            var currentUserId = int.Parse(_userMgr.GetUserId(User));
            
            
            if (currentUserId == userId)
                return RedirectToAction("Index");

            
            var otherUser = await _userMgr.FindByIdAsync(userId.ToString());
            if (otherUser == null)
                return NotFound();

            
            var conv = await _db.Conversations.FirstOrDefaultAsync(c =>
                (c.ParticipantAID == currentUserId && c.ParticipantBID == userId) ||
                (c.ParticipantBID == currentUserId && c.ParticipantAID == userId)
            );

            
            if (conv == null)
            {
               
                int? projectId = null;
                
                if (User.IsInRole("Student"))
                {
                   
                    projectId = await _db.Projects
                        .Where(p => p.BusinessID == userId)
                        .Join(_db.StudentApplications.Where(a => a.UserID == currentUserId),
                            p => p.ProjectID,
                            a => a.ProjectID,
                            (p, a) => p.ProjectID)
                        .FirstOrDefaultAsync();
                }
                else if (User.IsInRole("Business"))
                {
                    
                    projectId = await _db.Projects
                        .Where(p => p.BusinessID == currentUserId)
                        .Join(_db.StudentApplications.Where(a => a.UserID == userId),
                            p => p.ProjectID,
                            a => a.ProjectID,
                            (p, a) => p.ProjectID)
                        .FirstOrDefaultAsync();
                }

                conv = new Conversation
                {
                    ProjectID = projectId ?? 0, 
                    ParticipantAID = currentUserId,
                    ParticipantBID = userId,
                    CreatedAt = DateTime.UtcNow
                };
                
                _db.Conversations.Add(conv);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Chat), new { id = conv.ConversationID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int conversationID, string content)
        {
            var userId = int.Parse(_userMgr.GetUserId(User));

            var conv = await _db.Conversations.FindAsync(conversationID);
            if (conv == null) return NotFound();

            if (conv.ParticipantAID != userId && conv.ParticipantBID != userId)
                return Forbid();

            // Xác định kiểu nội dung
            string messageType;
            if (content.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                content.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                content.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                content.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            {
                messageType = "image";
            }
            else if (content.EndsWith(".doc", StringComparison.OrdinalIgnoreCase) ||
                     content.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ||
                     content.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                messageType = "file";
            }
            else
            {
                messageType = "text";
            }

            var msg = new Message
            {
                ConversationID = conversationID,
                SenderID = userId,
                ReceiverID = (conv.ParticipantAID == userId ? conv.ParticipantBID : conv.ParticipantAID),
                ProjectID = conv.ProjectID,
                Content = content,
                MessageType = messageType, 
                SentAt = DateTime.UtcNow,
                IsRead = false,
                IsActive = true
            };


            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Chat), new { id = conversationID });
        }
        public async Task<IActionResult> Chat(int? conversationId = null, int? projectId = null)
        {
            var userId = int.Parse(_userMgr.GetUserId(User));

            // Lấy danh sách conversation
            var convs = await _db.Conversations
                .Include(c => c.Messages)
                .Where(c =>
                    (c.ParticipantAID == userId || c.ParticipantBID == userId) &&
                    (!projectId.HasValue || c.ProjectID == projectId)
                )
                .AsNoTracking()
                .ToListAsync();

            var items = convs.Select(c =>
            {
                var otherId = c.ParticipantAID == userId ? c.ParticipantBID : c.ParticipantAID;
                var other = _userMgr.Users
                    .Where(u => u.Id == otherId)
                    .Select(u => new { u.UserName, u.Avatar })
                    .FirstOrDefault();

                var last = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                var unread = c.Messages.Count(m => !m.IsRead && m.SenderID != userId);

                var title = _db.Projects
                    .Where(p => p.ProjectID == c.ProjectID)
                    .Select(p => p.Title)
                    .FirstOrDefault();

                return new ConversationDto
                {
                    ConversationID = c.ConversationID,
                    OtherUserID = otherId,
                    OtherUserName = other?.UserName,
                    OtherUserAvatar = other?.Avatar,
                    ProjectID = c.ProjectID,
                    ProjectTitle = title,
                    LastMessage = last?.Content,
                    LastMessageAt = last?.SentAt ?? c.CreatedAt,
                    UnreadCount = unread
                };
            }).ToList();

            // Lấy danh sách project liên quan
            var bizProjects = await _db.Projects
                .Where(p => p.BusinessID == userId && p.IsActive)
                .ToListAsync();

            var appliedProjectIds = await _db.StudentApplications
                .Where(a => a.UserID == userId)
                .Select(a => a.ProjectID)
                .Distinct()
                .ToListAsync();

            var studentProjects = await _db.Projects
                .Where(p => appliedProjectIds.Contains(p.ProjectID) && p.IsActive)
                .ToListAsync();

            var projects = bizProjects
                .Concat(studentProjects)
                .GroupBy(p => p.ProjectID)
                .Select(g => g.First())
                .ToList();

            // Nếu có conversationId → load nội dung chat
            ChatRoomViewModel chatRoom = null;

            if (conversationId.HasValue)
            {
                var conv = await _db.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.ConversationID == conversationId.Value);

                if (conv != null && (conv.ParticipantAID == userId || conv.ParticipantBID == userId))
                {
                    var otherId = conv.ParticipantAID == userId ? conv.ParticipantBID : conv.ParticipantAID;
                    var other = await _userMgr.Users
                        .Where(u => u.Id == otherId)
                        .Select(u => new { u.UserName })
                        .FirstOrDefaultAsync();

                    var senderIds = conv.Messages.Select(m => m.SenderID).Distinct().ToList();
                    var users = await _userMgr.Users
                        .Where(u => senderIds.Contains(u.Id))
                        .Select(u => new { u.Id, u.UserName })
                        .ToListAsync();

                    var userDict = users.ToDictionary(u => u.Id, u => u.UserName);

                    var messageDtos = conv.Messages
                        .OrderBy(m => m.SentAt)
                        .Select(m => new MessageDto
                        {
                            SenderID = m.SenderID,
                            SenderName = userDict.TryGetValue(m.SenderID, out var name) ? name : "Invalid",
                            Content = m.Content,
                            MessageType = m.MessageType,
                            SentAt = m.SentAt,
                            IsMine = m.SenderID == userId
                        }).ToList();

                    chatRoom = new ChatRoomViewModel
                    {
                        ConversationID = conv.ConversationID,
                        Messages = messageDtos,
                        OtherUserName = other?.UserName ?? "Invalid"
                    };
                }
            }

            var vm = new ChatPageViewModel
            {
                Conversations = items,
                Projects = projects,
                SelectedProjectID = projectId,
                ChatRoom = chatRoom
            };

            return View(vm);
        }

    }
}
