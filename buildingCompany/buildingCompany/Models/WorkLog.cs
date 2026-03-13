using System.ComponentModel.DataAnnotations;

namespace buildingCompany.Models
{
    public class WorkLog
    {
        public int Id { get; set; }

        // Внешние ключи
        public int BuildingSiteId { get; set; }
        public int WorkTypeId { get; set; }
        public int EmployeeId { get; set; }

        public int HoursWorked { get; set; }
        public string Status { get; set; } = "Запланировано";

        // Для оптимистичной блокировки (добавим позже)
        // [Timestamp]
        // public byte[] RowVersion { get; set; }

        // Навигационные свойства
        public BuildingSite BuildingSite { get; set; } = null!;
        public WorkType WorkType { get; set; } = null!;
        public Employee Employee { get; set; } = null!;
    }
}