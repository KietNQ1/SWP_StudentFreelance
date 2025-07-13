namespace StudentFreelance.ViewModels
{
    public class RatingViewModel
    {
        public int ReviewerID { get; set; }
        public string ReviewerName { get; set; } // Tên người đánh giá
        public string ReviewerAvatarPath { get; set; } // Avatar
        public Decimal Score { get; set; } // Số sao
        public string Comment { get; set; } // Nội dung đánh giá
        public DateTime DateRated { get; set; } // Ngày đánh giá
    }

}
