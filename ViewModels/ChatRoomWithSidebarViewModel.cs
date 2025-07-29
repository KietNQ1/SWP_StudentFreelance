using System.Collections.Generic;
using StudentFreelance.Models;

namespace StudentFreelance.ViewModels
{
    public class ChatRoomWithSidebarViewModel
    {
        // Thông tin cuộc trò chuyện hiện tại
        public int ConversationID { get; set; }
        public List<MessageDto> Messages { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserAvatar { get; set; }
        
        // Thông tin cho sidebar
        public List<ConversationDto> Conversations { get; set; }
        public List<Project> Projects { get; set; }
        public int? SelectedProjectID { get; set; }
    }
} 