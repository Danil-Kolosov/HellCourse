using System.Collections.Generic;

namespace buildingCompany.Models
{
    public class WorkType
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }

        // Навигационное свойство
        public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
    }
}