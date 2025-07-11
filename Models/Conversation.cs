using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class Conversation
    {
        [Key]
        public int ConversationID { get; set; }

        // Gắn vào ProjectID bắt buộc vì chỉ còn loại chat theo project
        [Required]
        [ForeignKey(nameof(Project))]
        public int ProjectID { get; set; }

        // Hai participants: A là người khởi tạo, B là đối tác (Student - Business)
        [Required]
        public int ParticipantAID { get; set; }

        [Required]
        public int ParticipantBID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        // Navigation tới tin nhắn
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        
        // Navigation tới Project
        public virtual Project Project { get; set; }
    }
}
