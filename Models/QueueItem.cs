namespace SmartStationerySystem.Models
{
    public class QueueItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        public int QueueNumber { get; set; }

        public DateTime AddedTime { get; set; }

        public bool IsCompleted { get; set; }
    }
}