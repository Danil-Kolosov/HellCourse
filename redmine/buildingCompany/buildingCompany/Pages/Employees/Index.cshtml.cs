using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildingCompany.Models;
using buildingCompany.Data;
using System.Text;

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
    public byte[]? RowVersion { get; set; }

    public string? ConcurrencyError { get; set; }

    public bool ShowForm { get; set; }
    public bool IsEditMode { get; set; }

    public async Task OnGetAsync(bool? create, int? editId)
    {
        ConcurrencyError = null;

        if (create == true)
        {
            ShowForm = true;
            IsEditMode = false;
            FullName = string.Empty;
            Position = string.Empty;
            RowVersion = null;
        }
        else if (editId.HasValue)
        {
            var emp = await _context.Employees.FindAsync(editId.Value);
            if (emp != null)
            {
                ShowForm = true;
                IsEditMode = true;
                EmployeeId = emp.Id;
                FullName = emp.FullName;
                Position = emp.Position;
                RowVersion = emp.RowVersion;
            }
        }

        Employees = await _context.Employees.ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        ModelState.Remove("RowVersion");
        ModelState.Remove("EmployeeId");

        if (!ModelState.IsValid)
        {
            ShowForm = true;
            IsEditMode = false;
            Employees = await _context.Employees.ToListAsync();
            return Page();
        }

        try
        {
            var employee = new Employee
            {
                FullName = FullName.Trim(),
                Position = Position.Trim(),
                // Устанавливаем начальную версию
                RowVersion = Guid.NewGuid().ToByteArray()
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Сотрудник {employee.FullName} успешно добавлен";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
            ShowForm = true;
            IsEditMode = false;
            Employees = await _context.Employees.ToListAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        ModelState.Remove("RowVersion");

        if (!ModelState.IsValid)
        {
            ShowForm = true;
            IsEditMode = true;
            Employees = await _context.Employees.ToListAsync();
            return Page();
        }

        try
        {
            var employee = await _context.Employees.FindAsync(EmployeeId);
            if (employee == null)
            {
                TempData["Error"] = "Сотрудник не найден";
                return RedirectToPage("Index");
            }

            // Проверяем версию
            if (RowVersion != null && !employee.RowVersion.SequenceEqual(RowVersion))
            {
                // Конфликт версий - перезагружаем данные
                var latestEmployee = await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == EmployeeId);

                if (latestEmployee != null)
                {
                    FullName = latestEmployee.FullName;
                    Position = latestEmployee.Position;
                    RowVersion = latestEmployee.RowVersion;

                    ConcurrencyError = "Данные были изменены другим пользователем. Показаны актуальные данные.";
                    ShowForm = true;
                    IsEditMode = true;
                    Employees = await _context.Employees.ToListAsync();
                    return Page();
                }
            }

            // Обновляем поля
            employee.FullName = FullName.Trim();
            employee.Position = Position.Trim();

            // Обновляем версию при каждом изменении
            employee.RowVersion = Guid.NewGuid().ToByteArray();

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Сотрудник {employee.FullName} обновлен";
            return RedirectToPage("Index");
        }
        catch (DbUpdateConcurrencyException)
        {
            var latestEmployee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == EmployeeId);

            if (latestEmployee != null)
            {
                FullName = latestEmployee.FullName;
                Position = latestEmployee.Position;
                RowVersion = latestEmployee.RowVersion;

                ConcurrencyError = "Конфликт при сохранении. Показаны актуальные данные.";
                ShowForm = true;
                IsEditMode = true;
                Employees = await _context.Employees.ToListAsync();
                return Page();
            }

            TempData["Error"] = "Сотрудник был удален другим пользователем";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
            ShowForm = true;
            IsEditMode = true;
            Employees = await _context.Employees.ToListAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int deleteId)
    {
        try
        {
            var employee = await _context.Employees.FindAsync(deleteId);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Сотрудник {employee.FullName} удален";
            }
            else
            {
                TempData["Error"] = "Сотрудник не найден";
            }
        }
        catch (Exception ex)
        {
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