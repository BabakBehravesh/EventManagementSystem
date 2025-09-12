namespace EventManagementSystem.Application.DTOs
{
    public record EventResponse
    (
        int Id,
        string Name,
        string Description,
        string Location,
        DateTime StartDate,
        DateTime EndDate
    )
    {
        public int Id { get; set; } = Id;
        public string Name { get; set; } = Name;
        public string Description { get; set; } = Description;
        public string Location { get; set; }= Location;
        public DateTime StartDate { get; set; }= StartDate;
        public DateTime EndDate { get; set; }= EndDate;

    }
}
