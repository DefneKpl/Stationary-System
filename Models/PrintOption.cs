namespace SmartStationerySystem.Models
{
    public class PrintOption
    {
        public int Id { get; set; }

        public string ColorType { get; set; } = string.Empty;

        public string PaperSize { get; set; } = string.Empty;

        public bool IsDoubleSided { get; set; }

        public decimal PricePerPage { get; set; }

        public List<Order> Orders { get; set; } = new();
    }
}