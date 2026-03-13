using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace buildingCompany.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;

        // Поле для оптимистической блокировки
    
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Навигационное свойство
        public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
    }
}