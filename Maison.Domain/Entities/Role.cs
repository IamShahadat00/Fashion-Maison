using System.Collections.Generic;

namespace Maison.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsExecutive { get; set; }

        // Navigation Property
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}