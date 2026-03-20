using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildingCompany.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace buildingCompany.Pages.WorkLogs
{
    public class AssignModel : PageModel
    {
        private readonly string _connectionString;

        public AssignModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<BuildingSite> BuildingSites { get; set; } = new();
        public List<WorkType> WorkTypes { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();

        // Загружаем данные для выпадающих списков
        public async Task OnGetAsync()
        {
            BuildingSites = await GetAllBuildingSitesAsync();
            WorkTypes = await GetAllWorkTypesAsync();
            Employees = await GetAllEmployeesAsync();
        }

        private async Task<List<BuildingSite>> GetAllBuildingSitesAsync()
        {
            var buildingSites = new List<BuildingSite>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT Id, Name, Address FROM BuildingSites ORDER BY Name";

            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                buildingSites.Add(new BuildingSite
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Address = reader.GetString(2)
                });
            }

            return buildingSites;
        }

        private async Task<List<WorkType>> GetAllWorkTypesAsync()
        {
            var workTypes = new List<WorkType>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT Id, Title, PricePerHour FROM WorkTypes ORDER BY Title";

            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                workTypes.Add(new WorkType
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    PricePerHour = reader.GetDecimal(2)
                });
            }

            return workTypes;
        }

        private async Task<List<Employee>> GetAllEmployeesAsync()
        {
            var employees = new List<Employee>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT Id, FullName, Position, RowVersion FROM Employees ORDER BY FullName";

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

        // Сохраняем назначение работы
        public async Task<IActionResult> OnPostAsync(
            int BuildingSiteId,
            int WorkTypeId,
            int EmployeeId,
            int HoursWorked)
        {
            try
            {
                // Проверка входных данных
                if (BuildingSiteId <= 0 || WorkTypeId <= 0 || EmployeeId <= 0 || HoursWorked <= 0)
                {
                    ModelState.AddModelError("", "Все поля обязательны для заполнения и должны иметь корректные значения");
                    await LoadDropdownDataAsync();
                    return Page();
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // Проверяем существование связанных записей (опционально, для надежности)
                var checkBuildingSiteSql = "SELECT Id FROM BuildingSites WHERE Id = $id";
                using (var checkCommand = new SqliteCommand(checkBuildingSiteSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("$id", BuildingSiteId);
                    var exists = await checkCommand.ExecuteScalarAsync();
                    if (exists == null)
                    {
                        ModelState.AddModelError("", "Выбранный строительный объект не существует");
                        await LoadDropdownDataAsync();
                        return Page();
                    }
                }

                var checkWorkTypeSql = "SELECT Id FROM WorkTypes WHERE Id = $id";
                using (var checkCommand = new SqliteCommand(checkWorkTypeSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("$id", WorkTypeId);
                    var exists = await checkCommand.ExecuteScalarAsync();
                    if (exists == null)
                    {
                        ModelState.AddModelError("", "Выбранный вид работы не существует");
                        await LoadDropdownDataAsync();
                        return Page();
                    }
                }

                var checkEmployeeSql = "SELECT Id FROM Employees WHERE Id = $id";
                using (var checkCommand = new SqliteCommand(checkEmployeeSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("$id", EmployeeId);
                    var exists = await checkCommand.ExecuteScalarAsync();
                    if (exists == null)
                    {
                        ModelState.AddModelError("", "Выбранный сотрудник не существует");
                        await LoadDropdownDataAsync();
                        return Page();
                    }
                }

                // Создаем запись в журнале работ
                var insertSql = @"
                    INSERT INTO WorkLogs (BuildingSiteId, WorkTypeId, EmployeeId, HoursWorked, Status) 
                    VALUES ($buildingSiteId, $workTypeId, $employeeId, $hoursWorked, $status);
                    SELECT last_insert_rowid();";

                using var insertCommand = new SqliteCommand(insertSql, connection);
                insertCommand.Parameters.AddWithValue("$buildingSiteId", BuildingSiteId);
                insertCommand.Parameters.AddWithValue("$workTypeId", WorkTypeId);
                insertCommand.Parameters.AddWithValue("$employeeId", EmployeeId);
                insertCommand.Parameters.AddWithValue("$hoursWorked", HoursWorked);
                insertCommand.Parameters.AddWithValue("$status", "Назначено");

                var newId = await insertCommand.ExecuteScalarAsync();

                TempData["Success"] = "Работа успешно назначена";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");

                // Перезагружаем данные для выпадающих списков
                await LoadDropdownDataAsync();

                return Page();
            }
        }

        private async Task LoadDropdownDataAsync()
        {
            BuildingSites = await GetAllBuildingSitesAsync();
            WorkTypes = await GetAllWorkTypesAsync();
            Employees = await GetAllEmployeesAsync();
        }
    }
}