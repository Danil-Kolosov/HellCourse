using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildingCompany.Models;
using buildingCompany.Data;

namespace buildingCompany.Pages.WorkLogs
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<WorkLog> WorkLogs { get; set; } = new();
        public List<BuildingSite> BuildingSites { get; set; } = new();
        public List<WorkType> WorkTypes { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();

        [BindProperty]
        public int DeleteId { get; set; }

        [BindProperty]
        public int EditId { get; set; }

        [BindProperty]
        public int BuildingSiteId { get; set; }

        [BindProperty]
        public int WorkTypeId { get; set; }

        [BindProperty]
        public int EmployeeId { get; set; }

        [BindProperty]
        public int HoursWorked { get; set; }

        [BindProperty]
        public string Status { get; set; } = string.Empty;

        public WorkLog? EditWorkLog { get; set; }
        public bool ShowEditForm { get; set; }

        public async Task OnGetAsync(int? deleteId, int? editId)
        {
            try
            {
                Console.WriteLine($"WorkLogs OnGetAsync: deleteId={deleteId}, editId={editId}");

                // Удаление записи
                if (deleteId.HasValue)
                {
                    var workLog = await _context.WorkLogs.FindAsync(deleteId.Value);
                    if (workLog != null)
                    {
                        _context.WorkLogs.Remove(workLog);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Запись удалена";
                    }
                }

                // Режим редактирования
                if (editId.HasValue)
                {
                    EditWorkLog = await _context.WorkLogs
                        .Include(w => w.BuildingSite)
                        .Include(w => w.WorkType)
                        .Include(w => w.Employee)
                        .FirstOrDefaultAsync(w => w.Id == editId.Value);

                    if (EditWorkLog != null)
                    {
                        ShowEditForm = true;
                        EditId = EditWorkLog.Id;

                        // Загружаем данные для выпадающих списков
                        BuildingSites = await _context.BuildingSites.ToListAsync();
                        WorkTypes = await _context.WorkTypes.ToListAsync();
                        Employees = await _context.Employees.ToListAsync();
                    }
                }

                // Загрузка списка всех записей
                WorkLogs = await _context.WorkLogs
                    .Include(w => w.BuildingSite)
                    .Include(w => w.WorkType)
                    .Include(w => w.Employee)
                    .OrderByDescending(w => w.Id)
                    .ToListAsync();

                Console.WriteLine($"Загружено записей: {WorkLogs.Count}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка: {ex.Message}";
                Console.WriteLine($"Ошибка в OnGetAsync: {ex}");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            try
            {
                Console.WriteLine($"WorkLogs OnPostDeleteAsync вызван. DeleteId: {DeleteId}");

                if (DeleteId <= 0)
                {
                    TempData["Error"] = "Некорректный ID";
                    return RedirectToPage("Index");
                }

                var workLog = await _context.WorkLogs.FindAsync(DeleteId);
                if (workLog != null)
                {
                    _context.WorkLogs.Remove(workLog);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Запись удалена";
                    Console.WriteLine($"Запись {DeleteId} удалена");
                }
                else
                {
                    TempData["Error"] = "Запись не найдена";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnPostDeleteAsync: {ex}");
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
            }

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            try
            {
                Console.WriteLine("WorkLogs OnPostUpdateAsync вызван");
                Console.WriteLine($"EditId: {EditId}, BuildingSiteId: {BuildingSiteId}, WorkTypeId: {WorkTypeId}, EmployeeId: {EmployeeId}, HoursWorked: {HoursWorked}, Status: {Status}");

                if (EditId <= 0)
                {
                    TempData["Error"] = "Некорректный ID";
                    return RedirectToPage("Index");
                }

                var workLog = await _context.WorkLogs.FindAsync(EditId);
                if (workLog == null)
                {
                    TempData["Error"] = "Запись не найдена";
                    return RedirectToPage("Index");
                }

                // Проверяем существование связанных записей
                var buildingSite = await _context.BuildingSites.FindAsync(BuildingSiteId);
                var workType = await _context.WorkTypes.FindAsync(WorkTypeId);
                var employee = await _context.Employees.FindAsync(EmployeeId);

                if (buildingSite == null || workType == null || employee == null)
                {
                    TempData["Error"] = "Одна из связанных записей не найдена";
                    return RedirectToPage("Index");
                }

                // Обновляем поля
                workLog.BuildingSiteId = BuildingSiteId;
                workLog.WorkTypeId = WorkTypeId;
                workLog.EmployeeId = EmployeeId;
                workLog.HoursWorked = HoursWorked;
                workLog.Status = Status;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Запись успешно обновлена";
                Console.WriteLine($"Запись {EditId} обновлена");

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnPostUpdateAsync: {ex}");
                TempData["Error"] = $"Ошибка при обновлении: {ex.Message}";
                return RedirectToPage("Index", new { editId = EditId });
            }
        }
    }
}



/*
using buildingCompany.Data;
using buildingCompany.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class AssignModel : PageModel
{
    private readonly AppDbContext _context;

    public AssignModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public int WorkLogId { get; set; }

    [BindProperty]
    public int SelectedEmployeeId { get; set; }

    public string WorkDescription { get; set; }
    public List<Employee> AvailableEmployees { get; set; }

    public async Task<IActionResult> OnGetAsync(int workLogId)
    {
        WorkLogId = workLogId;

        // 1. ПЕССИМИСТИЧНАЯ БЛОКИРОВКА (RAW SQL - FOR UPDATE)
        // Мы блокируем запись WorkLog, чтобы никто другой не мог её изменить,
        // пока мы выбираем сотрудника.
        // В EF Core нет прямого .ForUpdate(), но можно выполнить сырой SQL.
        // Важно: Эта транзакция должна быть открыта долго (плохая практика в вебе,
        // но для лабы сойдет). В реальности так не делают.
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Блокируем строку WorkLog
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM WorkLogs WHERE Id = @id FOR UPDATE;";
            cmd.Parameters.Add(new SqlParameter("@id", workLogId));
            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                // Получаем описание работы
                var buildingSiteId = reader.GetInt32(1);
                var workTypeId = reader.GetInt32(2);

                // Здесь нам нужно получить данные через другой контекст или до-выборку.
                // Упростим: просто сохраним транзакцию в TempData или свойство.
                // Для простоты я просто загружу данные позже через основной контекст,
                // но это нарушит блокировку. Поэтому в реальном проекте нужно
                // проектировать иначе.
                WorkDescription = $"Запись #{workLogId} заблокирована для редактирования.";
            }
            reader.Close();

            // Сохраняем транзакцию, чтобы использовать её позже в OnPost
            // (Сложно, для лабы достаточно просто показать концепцию).
            // Второй вариант (проще) - использовать оптимистичную для редактирования,
            // а пессимистичную показать на отдельном примере, например, при генерации отчета.

            // Загружаем доступных сотрудников (без блокировки)
            AvailableEmployees = await _context.Employees.ToListAsync();
        }
        finally
        {
            // В реальном коде тут бы не закрывали, а хранили транзакцию.
            await transaction.CommitAsync(); // Или Rollback если ошибка
            await connection.CloseAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // 2. ОПТИМИСТИЧНАЯ БЛОКИРОВКА
        // Мы предполагаем, что данные не изменятся между чтением и записью.
        // Для этого используем RowVersion (Concurrency Token).

        // Добавим поле Timestamp в модель WorkLog:
        // [Timestamp] public byte[] RowVersion { get; set; }

        try
        {
            var workLog = await _context.WorkLogs
                .FirstOrDefaultAsync(w => w.Id == WorkLogId);

            if (workLog == null)
            {
                return NotFound();
            }

            workLog.EmployeeId = SelectedEmployeeId;
            workLog.Status = "В процессе";

            // Сохраняем изменения. Если RowVersion в БД отличается от того,
            // что был загружен в workLog, EF кинет DbUpdateConcurrencyException.
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
        catch (DbUpdateConcurrencyException)
        {
            // Ловим исключение. Значит, кто-то другой изменил эту запись.
            ModelState.AddModelError(string.Empty, "Запись была изменена другим пользователем. Попробуйте снова.");
            // Перезагружаем данные для формы
            AvailableEmployees = await _context.Employees.ToListAsync();
            return Page();
        }
    }
}*/