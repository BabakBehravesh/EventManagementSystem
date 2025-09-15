namespace EventManagementSystem.Application.DTOs
{
    public class EventResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public EventResponse(int id, string name, string description, string location,
                            DateTime startTime, DateTime endTime)
        {
            Id = id;
            Name = name;
            Description = description;
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
