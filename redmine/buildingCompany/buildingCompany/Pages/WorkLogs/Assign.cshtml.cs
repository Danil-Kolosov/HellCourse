using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildingCompany.Models;
using buildingCompany.Data;

namespace buildingCompany.Pages.WorkLogs
{
    public class AssignModel : PageModel
    {
        private readonly AppDbContext _context;

        public AssignModel(AppDbContext context)
        {
            _context = context;
        }

        public List<BuildingSite> BuildingSites { get; set; } = new();
        public List<WorkType> WorkTypes { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();

        // Загружаем данные для выпадающих списков
        public async Task OnGetAsync()
        {
            BuildingSites = await _context.BuildingSites.ToListAsync();
            WorkTypes = await _context.WorkTypes.ToListAsync();
            Employees = await _context.Employees.ToListAsync();
        }

        // Сохраняем назначение работы с транзакцией
        public async Task<IActionResult> OnPostAsync(
            int BuildingSiteId,
            int WorkTypeId,
            int EmployeeId,
            int HoursWorked)
        {
            // НАЧИНАЕМ ТРАНЗАКЦИЮ
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Создаем запись в журнале работ
                var workLog = new WorkLog
                {
                    BuildingSiteId = BuildingSiteId,
                    WorkTypeId = WorkTypeId,
                    EmployeeId = EmployeeId,
                    HoursWorked = HoursWorked,
                    Status = "Назначено"
                };

                _context.WorkLogs.Add(workLog);
                await _context.SaveChangesAsync();

                // Можно добавить еще какие-то действия в той же транзакции
                // Например, обновить статистику сотрудника

                // ПОДТВЕРЖДАЕМ ТРАНЗАКЦИЮ
                await transaction.CommitAsync();

                return RedirectToPage("Index");
            }
            catch
            {
                // ОТМЕНЯЕМ ТРАНЗАКЦИЮ В СЛУЧАЕ ОШИБКИ
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}