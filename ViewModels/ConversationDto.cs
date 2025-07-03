using System;

namespace StudentFreelance.ViewModels
{
    public class ConversationDto
    {
        public int ConversationID { get; set; }

        // Thông tin user bên kia
        public int OtherUserID { get; set; }
        public string? OtherUserName { get; set; }
        public string? OtherUserAvatar { get; set; }

        // Thông tin dự án gắn với cuộc trò chuyện
        public int ProjectID { get; set; }
        public string? ProjectTitle { get; set; }

        // Tin nhắn gần nhất
        public string? LastMessage { get; set; }
        public DateTime LastMessageAt { get; set; }

        // Số lượng tin chưa đọc
        public int UnreadCount { get; set; }
    }
}
