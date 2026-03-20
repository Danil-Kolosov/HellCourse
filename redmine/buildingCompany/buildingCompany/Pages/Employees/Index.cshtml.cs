using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildingCompany.Models;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace buildingCompany.Pages.Employees;

/// <summary>
/// Страница управления сотрудниками строительной компании.
/// </summary>
/// <remarks>
/// <para>Страница поддерживает следующие операции:</para>
/// <list type="bullet">
/// <item><description>Просмотр списка всех сотрудников</description></item>
/// <item><description>Добавление нового сотрудника</description></item>
/// <item><description>Редактирование данных сотрудника</description></item>
/// <item><description>Удаление сотрудника с проверкой связей</description></item>
/// </list>
/// <para>Реализована оптимистическая блокировка (optimistic concurrency) через поле RowVersion.</para>
/// </remarks>
public class IndexModel : PageModel
{
    private readonly string _connectionString;

    /// <summary>
    /// Инициализирует новый экземпляр страницы сотрудников.
    /// </summary>
    /// <param name="configuration">Конфигурация приложения для получения строки подключения к БД.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если configuration равен null.</exception>
    public IndexModel(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    /// <summary>
    /// Список всех сотрудников для отображения на странице.
    /// </summary>
    public List<Employee> Employees { get; set; } = new();

    /// <summary>
    /// Идентификатор редактируемого сотрудника.
    /// </summary>
    [BindProperty]
    public int EmployeeId { get; set; }

    /// <summary>
    /// Полное имя сотрудника (Фамилия Имя Отчество).
    /// </summary>
    /// <remarks>Поле обязательно для заполнения.</remarks>
    [BindProperty]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Должность сотрудника.
    /// </summary>
    /// <example>Маляр, Плотник, Разнорабочий</example>
    [BindProperty]
    public string Position { get; set; } = string.Empty;


    /// <summary>
    /// Версия записи для оптимистической блокировки.
    /// Используется для предотвращения конфликтов при одновременном редактировании.
    /// </summary>
    [BindProperty]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Сообщение об ошибке при конфликте версий.
    /// </summary>
    public string? ConcurrencyError { get; set; }

    /// <summary>
    /// Флаг отображения формы добавления/редактирования.
    /// </summary>
    public bool ShowForm { get; set; }

    /// <summary>
    /// Флаг, указывающий, находится ли форма в режиме редактирования.
    /// </summary>
    public bool IsEditMode { get; set; }

    /// <summary>
    /// Обработчик GET-запроса для страницы сотрудников.
    /// </summary>
    /// <param name="create">Если true, открывает форму для создания нового сотрудника.</param>
    /// <param name="editId">Если указан, открывает форму для редактирования сотрудника с данным ID.</param>    
    /// <remarks>
    /// Метод загружает список всех сотрудников и, при необходимости,
    /// открывает форму для создания или редактирования.
    /// </remarks>
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
            var emp = await GetEmployeeByIdAsync(editId.Value);
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

        Employees = await GetAllEmployeesAsync();
    }

    private async Task<List<Employee>> GetAllEmployeesAsync()
    {
        var employees = new List<Employee>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT Id, FullName, Position, RowVersion FROM Employees ORDER BY Id";

        using var command = new SqliteCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            employees.Add(new Employee
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Position = reader.GetString(2),
                RowVersion = reader.IsDBNull(3) ? Array.Empty<byte>() : (byte[])reader[3]
            });
        }

        return employees;
    }

    private async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT Id, FullName, Position, RowVersion FROM Employees WHERE Id = $id";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("$id", id);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Employee
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Position = reader.GetString(2),
                RowVersion = reader.IsDBNull(3) ? Array.Empty<byte>() : (byte[])reader[3]
            };
        }

        return null;
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        ModelState.Remove("RowVersion");
        ModelState.Remove("EmployeeId");

        if (!ModelState.IsValid)
        {
            ShowForm = true;
            IsEditMode = false;
            Employees = await GetAllEmployeesAsync();
            return Page();
        }

        try
        {
            // Генерируем новую версию для RowVersion
            var newRowVersion = Guid.NewGuid().ToByteArray();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO Employees (FullName, Position, RowVersion) 
                VALUES ($fullName, $position, $rowVersion);
                SELECT last_insert_rowid();";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("$fullName", FullName.Trim());
            command.Parameters.AddWithValue("$position", Position.Trim());
            command.Parameters.AddWithValue("$rowVersion", newRowVersion);

            var newId = await command.ExecuteScalarAsync();

            TempData["Success"] = $"Сотрудник {FullName.Trim()} успешно добавлен";

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
            ShowForm = true;
            IsEditMode = false;
            Employees = await GetAllEmployeesAsync();
            return Page();
        }
    }

    /// <summary>
    /// Обработчик POST-запроса для обновления существующего сотрудника.
    /// </summary>
    /// <returns>Перенаправление на страницу списка сотрудников при успехе, иначе возврат к странице с ошибкой.</returns>
    /// <remarks>
    /// <para>Реализует оптимистическую блокировку (optimistic concurrency):</para>
    /// <list type="bullet">
    /// <item><description>Проверяет версию записи перед обновлением</description></item>
    /// <item><description>При обнаружении конфликта отображает актуальные данные</description></item>
    /// <item><description>Генерирует новую версию при успешном обновлении</description></item>
    /// </list>
    /// </remarks>
    public async Task<IActionResult> OnPostUpdateAsync()
    {
        ModelState.Remove("RowVersion");

        if (!ModelState.IsValid)
        {
            ShowForm = true;
            IsEditMode = true;
            Employees = await GetAllEmployeesAsync();
            return Page();
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Проверяем существование записи и получаем текущую версию
            var checkSql = "SELECT FullName, Position, RowVersion FROM Employees WHERE Id = $id";
            Employee? currentEmployee = null;

            using (var checkCommand = new SqliteCommand(checkSql, connection))
            {
                checkCommand.Parameters.AddWithValue("$id", EmployeeId);
                using var reader = await checkCommand.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    currentEmployee = new Employee
                    {
                        Id = EmployeeId,
                        FullName = reader.GetString(0),
                        Position = reader.GetString(1),
                        RowVersion = reader.IsDBNull(2) ? Array.Empty<byte>() : (byte[])reader[2]
                    };
                }
            }

            if (currentEmployee == null)
            {
                TempData["Error"] = "Сотрудник не найден";
                return RedirectToPage("Index");
            }

            // Проверяем версию для обнаружения конфликтов
            if (RowVersion != null && currentEmployee.RowVersion != null &&
                !currentEmployee.RowVersion.SequenceEqual(RowVersion))
            {
                // Конфликт версий
                FullName = currentEmployee.FullName;
                Position = currentEmployee.Position;
                RowVersion = currentEmployee.RowVersion;

                ConcurrencyError = "Данные были изменены другим пользователем. Показаны актуальные данные.";
                ShowForm = true;
                IsEditMode = true;
                Employees = await GetAllEmployeesAsync();
                return Page();
            }

            // Генерируем новую версию и обновляем запись
            var newRowVersion = Guid.NewGuid().ToByteArray();

            var updateSql = @"
                UPDATE Employees 
                SET FullName = $fullName, 
                    Position = $position, 
                    RowVersion = $rowVersion 
                WHERE Id = $id 
                AND (RowVersion = $originalRowVersion)";

            using var updateCommand = new SqliteCommand(updateSql, connection);
            updateCommand.Parameters.AddWithValue("$fullName", FullName.Trim());
            updateCommand.Parameters.AddWithValue("$position", Position.Trim());
            updateCommand.Parameters.AddWithValue("$rowVersion", newRowVersion);
            updateCommand.Parameters.AddWithValue("$id", EmployeeId);
            updateCommand.Parameters.AddWithValue("$originalRowVersion", RowVersion ?? Array.Empty<byte>());

            var rowsAffected = await updateCommand.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                TempData["Success"] = $"Сотрудник {FullName.Trim()} обновлен";
                return RedirectToPage("Index");
            }
            else
            {
                // Конфликт при обновлении - получаем актуальные данные
                var latestEmployee = await GetEmployeeByIdAsync(EmployeeId);
                if (latestEmployee != null)
                {
                    FullName = latestEmployee.FullName;
                    Position = latestEmployee.Position;
                    RowVersion = latestEmployee.RowVersion;

                    ConcurrencyError = "Конфликт при сохранении. Показаны актуальные данные.";
                    ShowForm = true;
                    IsEditMode = true;
                    Employees = await GetAllEmployeesAsync();
                    return Page();
                }

                TempData["Error"] = "Сотрудник был удален другим пользователем";
                return RedirectToPage("Index");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
            ShowForm = true;
            IsEditMode = true;
            Employees = await GetAllEmployeesAsync();
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
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Проверяем наличие связанных записей в WorkLogs
            var checkLogsSql = "SELECT COUNT(*) FROM WorkLogs WHERE EmployeeId = $id";
            int workLogsCount = 0;

            using (var checkCommand = new SqliteCommand(checkLogsSql, connection))
            {
                checkCommand.Parameters.AddWithValue("$id", deleteId);
                workLogsCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
            }

            if (workLogsCount > 0)
            {
                TempData["Error"] =
                    $"Невозможно удалить сотрудника. У него есть {workLogsCount} " +
                    $"записей в журнале работ. Пожалуйста, сначала удалите эти записи.";

                return RedirectToPage("Index");
            }

            // Получаем имя сотрудника для сообщения
            var getNameSql = "SELECT FullName FROM Employees WHERE Id = $id";
            string? employeeName = null;

            using (var getNameCommand = new SqliteCommand(getNameSql, connection))
            {
                getNameCommand.Parameters.AddWithValue("$id", deleteId);
                employeeName = await getNameCommand.ExecuteScalarAsync() as string;
            }

            // Параметризированный DELETE запрос
            var deleteSql = "DELETE FROM Employees WHERE Id = $id";
            using var deleteCommand = new SqliteCommand(deleteSql, connection);
            deleteCommand.Parameters.AddWithValue("$id", deleteId);

            var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();

            if (rowsAffected > 0 && !string.IsNullOrEmpty(employeeName))
            {
                TempData["Success"] = $"Сотрудник {employeeName} удален";
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