using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models
{
    public class Conversation
    {
        [Key]
        public int ConversationID { get; set; }

        // Gắn vào ProjectID bắt buộc vì chỉ còn loại chat theo project
        [Required]
        public int ProjectID { get; set; }

        // Hai participants: A là người khởi tạo, B là đối tác (Student - Business)
        [Required]
        public int ParticipantAID { get; set; }

        [Required]
        public int ParticipantBID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation tới tin nhắn
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
