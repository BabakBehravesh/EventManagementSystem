using Microsoft.AspNetCore.Identity;
using System.Collections.ObjectModel;

namespace EventManagementSystem.Domain.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public required string CreatedBy { get; set; } 
        public ApplicationUser? Creator { get; set; } 
        public Collection<Registration> Registrations { get; set; } = [];
    }
}
