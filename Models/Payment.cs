namespace SmartStationerySystem.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;
        // Nakit / Kart / Online

        public string PaymentStatus { get; set; } = string.Empty;
        // Beklemede / Ödendi / İptal

        public DateTime PaymentDate { get; set; }
    }
}