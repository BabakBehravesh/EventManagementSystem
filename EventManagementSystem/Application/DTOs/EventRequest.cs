namespace EventManagementSystem.Application.DTOs;

    public record EventRequest
    (
        string Name,
        string Description,
        string Location,
        DateTime StartTime,
        DateTime EndTime
    )
    {
        public string Name { get; set; } = Name;
        public string Description { get; set; } = Description;
        public string Location { get; set; } = Location;
        public DateTime StartTime { get; set; } = StartTime;
        public DateTime EndTime { get; set; } = EndTime;
    }

