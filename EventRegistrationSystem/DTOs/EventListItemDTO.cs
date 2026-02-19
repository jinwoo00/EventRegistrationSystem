namespace EventRegistrationSystem.DTOs
{
    public class EventListItemDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime StartDate { get; set; }
        public string? Location { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}