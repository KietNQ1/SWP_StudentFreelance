using System.Collections.Generic;
using StudentFreelance.Models;

namespace StudentFreelance.ViewModels
{
    public class ChatPageViewModel
    {
        public List<ConversationDto> Conversations { get; set; }
        public List<Project> Projects { get; set; }
        public int? SelectedProjectID { get; set; }
        public ChatRoomViewModel? ChatRoom { get; set; }
    }
}
