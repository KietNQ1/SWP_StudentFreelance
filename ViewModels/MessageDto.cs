using System;

namespace StudentFreelance.ViewModels
{
    public class MessageDto
    {
        public int SenderID { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsMine { get; set; }
    }
}
