namespace SmartStationerySystem.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int PrintOptionId { get; set; }
        public PrintOption? PrintOption { get; set; }

        public int PageCount { get; set; }

        public int CopyCount { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public UploadedFile? UploadedFile { get; set; }
    }
}