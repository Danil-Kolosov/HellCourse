using System.Collections.Generic;

namespace buildingCompany.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;

        // Навигационное свойство
        public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
    }
}