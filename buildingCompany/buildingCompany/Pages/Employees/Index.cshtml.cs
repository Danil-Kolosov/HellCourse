using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildingCompany.Models;
using buildingCompany.Data;

namespace buildingCompany.Pages.Employees;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Employee> Employees { get; set; } = new();

    [BindProperty]
    public int EmployeeId { get; set; }

    [BindProperty]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    public string Position { get; set; } = string.Empty;

    [BindProperty]
    public int DeleteId { get; set; }

    public bool ShowForm { get; set; }

    public async Task OnGetAsync(bool? create, int? editId, int? deleteId)
    {
        try
        {
            Console.WriteLine($"OnGetAsync: create={create}, editId={editId}, deleteId={deleteId}");

            // Удаление (если передан deleteId)
            if (deleteId.HasValue)
            {
                var emp = await _context.Employees.FindAsync(deleteId.Value);
                if (emp != null)
                {
                    _context.Employees.Remove(emp);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Сотрудник {emp.FullName} удален";
                }
            }

            // Режим создания
            if (create == true)
            {
                ShowForm = true;
                EmployeeId = 0;
                FullName = string.Empty;
                Position = string.Empty;
            }
            // Режим редактирования
            else if (editId.HasValue)
            {
                var emp = await _context.Employees.FindAsync(editId.Value);
                if (emp != null)
                {
                    ShowForm = true;
                    EmployeeId = emp.Id;
                    FullName = emp.FullName;
                    Position = emp.Position;
                }
            }

            // Загрузка списка
            Employees = await _context.Employees.ToListAsync();
            Console.WriteLine($"Загружено сотрудников: {Employees.Count}");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка: {ex.Message}";
            Console.WriteLine($"Ошибка в OnGetAsync: {ex}");
        }
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        try
        {
            Console.WriteLine("OnPostCreateAsync вызван");
            Console.WriteLine($"FullName: '{FullName}', Position: '{Position}'");

            // Проверка ModelState
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState невалидна");
                ShowForm = true;
                Employees = await _context.Employees.ToListAsync();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Position))
            {
                Console.WriteLine("Поля пустые");
                ModelState.AddModelError("", "Все поля обязательны для заполнения");
                ShowForm = true;
                Employees = await _context.Employees.ToListAsync();
                return Page();
            }

            var employee = new Employee
            {
                FullName = FullName.Trim(),
                Position = Position.Trim()
            };

            Console.WriteLine($"Добавление сотрудника: {employee.FullName}, {employee.Position}");

            _context.Employees.Add(employee);
            var result = await _context.SaveChangesAsync();

            Console.WriteLine($"SaveChangesAsync результат: {result}");
            Console.WriteLine($"ID нового сотрудника: {employee.Id}");

            if (result > 0)
            {
                TempData["Success"] = $"Сотрудник {employee.FullName} успешно добавлен (ID: {employee.Id})";
            }
            else
            {
                TempData["Error"] = "Не удалось добавить сотрудника";
            }

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в OnPostCreateAsync: {ex}");
            ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
            ShowForm = true;
            Employees = await _context.Employees.ToListAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        try
        {
            Console.WriteLine("OnPostUpdateAsync вызван");
            Console.WriteLine($"EmployeeId: {EmployeeId}, FullName: '{FullName}', Position: '{Position}'");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState невалидна");
                ShowForm = true;
                Employees = await _context.Employees.ToListAsync();
                return Page();
            }

            if (EmployeeId <= 0)
            {
                ModelState.AddModelError("", "Некорректный ID сотрудника");
                ShowForm = true;
                Employees = await _context.Employees.ToListAsync();
                return Page();
            }

            var employee = await _context.Employees.FindAsync(EmployeeId);
            if (employee == null)
            {
                Console.WriteLine($"Сотрудник с ID {EmployeeId} не найден");
                ModelState.AddModelError("", "Сотрудник не найден");
                ShowForm = true;
                Employees = await _context.Employees.ToListAsync();
                return Page();
            }

            employee.FullName = FullName.Trim();
            employee.Position = Position.Trim();

            var result = await _context.SaveChangesAsync();

            Console.WriteLine($"SaveChangesAsync результат: {result}");

            if (result > 0)
            {
                TempData["Success"] = $"Сотрудник {employee.FullName} обновлен";
            }
            else
            {
                TempData["Error"] = "Не удалось обновить сотрудника";
            }

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в OnPostUpdateAsync: {ex}");
            ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
            ShowForm = true;
            Employees = await _context.Employees.ToListAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        try
        {
            Console.WriteLine($"OnPostDeleteAsync вызван. DeleteId: {DeleteId}");

            if (DeleteId <= 0)
            {
                TempData["Error"] = "Некорректный ID сотрудника";
                return RedirectToPage("Index");
            }

            var employee = await _context.Employees.FindAsync(DeleteId);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Сотрудник {employee.FullName} удален";
                Console.WriteLine($"Сотрудник {employee.FullName} удален");
            }
            else
            {
                TempData["Error"] = "Сотрудник не найден";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в OnPostDeleteAsync: {ex}");
            TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
        }

        return RedirectToPage("Index");
    }
}
public class Testing
{
    public static int Sum(int a, int b)
    {
        return a + b;
    }
}