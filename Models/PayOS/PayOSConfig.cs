namespace StudentFreelance.Models.PayOS
{
    public class PayOSConfig
    {
        public string ClientId { get; set; } = null;
        public string ApiKey { get; set; } = null;
        public string ChecksumKey { get; set; } = null;
        public string ReturnUrl { get; set; } = null;
        public string CancelUrl { get; set; } = null;
    }
}
