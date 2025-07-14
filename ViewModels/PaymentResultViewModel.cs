namespace StudentFreelance.ViewModels
{
    public class PaymentResultViewModel
    {
        public bool IsSuccess { get; set; }  // true nếu thành công
        public string OrderCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
