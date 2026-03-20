using System.Collections.Generic;

namespace buildingCompany.Models
{
    /// <summary>
    /// Класс, представляющий строительный объект.
    /// </summary>
    public class BuildingSite
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Навигационное свойство
        public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
    }
}