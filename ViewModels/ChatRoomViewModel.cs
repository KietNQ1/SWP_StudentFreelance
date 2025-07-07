namespace StudentFreelance.ViewModels
{
    public class ChatRoomViewModel
    {
        public int ConversationID { get; set; }
        public List<MessageDto> Messages { get; set; }
        public string OtherUserName { get; set; }
    }
}
