using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildingCompany.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace buildingCompany.Pages.WorkLogs
{
    public class IndexModel : PageModel
    {
        private readonly string _connectionString;

        public IndexModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
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
                    await DeleteWorkLogAsync(deleteId.Value);
                }

                // Режим редактирования
                if (editId.HasValue)
                {
                    EditWorkLog = await GetWorkLogByIdAsync(editId.Value);

                    if (EditWorkLog != null)
                    {
                        ShowEditForm = true;
                        EditId = EditWorkLog.Id;
                        BuildingSiteId = EditWorkLog.BuildingSiteId;
                        WorkTypeId = EditWorkLog.WorkTypeId;
                        EmployeeId = EditWorkLog.EmployeeId;
                        HoursWorked = EditWorkLog.HoursWorked;
                        Status = EditWorkLog.Status;

                        // Загружаем данные для выпадающих списков
                        await LoadDropdownDataAsync();
                    }
                }

                // Загрузка списка всех записей
                WorkLogs = await GetAllWorkLogsAsync();
                Console.WriteLine($"Загружено записей: {WorkLogs.Count}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка: {ex.Message}";
                Console.WriteLine($"Ошибка в OnGetAsync: {ex}");
            }
        }

        private async Task<List<WorkLog>> GetAllWorkLogsAsync()
        {
            var workLogs = new List<WorkLog>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // JOIN запрос для получения всех связанных данных
            var sql = @"
                SELECT 
                    wl.Id, wl.BuildingSiteId, wl.WorkTypeId, wl.EmployeeId, 
                    wl.HoursWorked, wl.Status,
                    bs.Id, bs.Name, bs.Address,
                    wt.Id, wt.Title, wt.PricePerHour,
                    e.Id, e.FullName, e.Position, e.RowVersion
                FROM WorkLogs wl
                INNER JOIN BuildingSites bs ON wl.BuildingSiteId = bs.Id
                INNER JOIN WorkTypes wt ON wl.WorkTypeId = wt.Id
                INNER JOIN Employees e ON wl.EmployeeId = e.Id
                ORDER BY wl.Id DESC";

            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var workLog = new WorkLog
                {
                    Id = reader.GetInt32(0),
                    BuildingSiteId = reader.GetInt32(1),
                    WorkTypeId = reader.GetInt32(2),
                    EmployeeId = reader.GetInt32(3),
                    HoursWorked = reader.GetInt32(4),
                    Status = reader.GetString(5),

                    BuildingSite = new BuildingSite
                    {
                        Id = reader.GetInt32(6),
                        Name = reader.GetString(7),
                        Address = reader.GetString(8)
                    },

                    WorkType = new WorkType
                    {
                        Id = reader.GetInt32(9),
                        Title = reader.GetString(10),
                        PricePerHour = reader.GetDecimal(11)
                    },

                    Employee = new Employee
                    {
                        Id = reader.GetInt32(12),
                        FullName = reader.GetString(13),
                        Position = reader.GetString(14),
                        RowVersion = reader.IsDBNull(15) ? Array.Empty<byte>() : (byte[])reader[15]
                    }
                };

                workLogs.Add(workLog);
            }

            return workLogs;
        }

        private async Task<WorkLog?> GetWorkLogByIdAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    wl.Id, wl.BuildingSiteId, wl.WorkTypeId, wl.EmployeeId, 
                    wl.HoursWorked, wl.Status,
                    bs.Id, bs.Name, bs.Address,
                    wt.Id, wt.Title, wt.PricePerHour,
                    e.Id, e.FullName, e.Position, e.RowVersion
                FROM WorkLogs wl
                INNER JOIN BuildingSites bs ON wl.BuildingSiteId = bs.Id
                INNER JOIN WorkTypes wt ON wl.WorkTypeId = wt.Id
                INNER JOIN Employees e ON wl.EmployeeId = e.Id
                WHERE wl.Id = $id";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("$id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new WorkLog
                {
                    Id = reader.GetInt32(0),
                    BuildingSiteId = reader.GetInt32(1),
                    WorkTypeId = reader.GetInt32(2),
                    EmployeeId = reader.GetInt32(3),
                    HoursWorked = reader.GetInt32(4),
                    Status = reader.GetString(5),

                    BuildingSite = new BuildingSite
                    {
                        Id = reader.GetInt32(6),
                        Name = reader.GetString(7),
                        Address = reader.GetString(8)
                    },

                    WorkType = new WorkType
                    {
                        Id = reader.GetInt32(9),
                        Title = reader.GetString(10),
                        PricePerHour = reader.GetDecimal(11)
                    },

                    Employee = new Employee
                    {
                        Id = reader.GetInt32(12),
                        FullName = reader.GetString(13),
                        Position = reader.GetString(14),
                        RowVersion = reader.IsDBNull(15) ? Array.Empty<byte>() : (byte[])reader[15]
                    }
                };
            }

            return null;
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

        private async Task DeleteWorkLogAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var deleteSql = "DELETE FROM WorkLogs WHERE Id = $id";
            using var deleteCommand = new SqliteCommand(deleteSql, connection);
            deleteCommand.Parameters.AddWithValue("$id", id);

            var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                TempData["Success"] = "Запись удалена";
                Console.WriteLine($"Запись {id} удалена");
            }
            else
            {
                TempData["Error"] = "Запись не найдена";
            }
        }

        private async Task LoadDropdownDataAsync()
        {
            BuildingSites = await GetAllBuildingSitesAsync();
            WorkTypes = await GetAllWorkTypesAsync();
            Employees = await GetAllEmployeesAsync();
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

                await DeleteWorkLogAsync(DeleteId);
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
            SqliteConnection? connection = null;
            SqliteTransaction? transaction = null;

            try
            {
                connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // BEGIN IMMEDIATE блокирует БД для записи, 
                // но чтение разрешено другим подключениям
                transaction = (SqliteTransaction?)await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                // Проверяем существование записи и сразу получаем данные
                var selectSql = "SELECT Id, BuildingSiteId, WorkTypeId, EmployeeId, HoursWorked, Status FROM WorkLogs WHERE Id = $id";

                using var selectCommand = new SqliteCommand(selectSql, connection, transaction);
                selectCommand.Parameters.AddWithValue("$id", EditId);

                using var reader = await selectCommand.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Запись не найдена";
                    return RedirectToPage("Index");
                }

                // Обновляем запись
                var updateSql = @"
            UPDATE WorkLogs 
            SET BuildingSiteId = $buildingSiteId,
                WorkTypeId = $workTypeId,
                EmployeeId = $employeeId,
                HoursWorked = $hoursWorked,
                Status = $status
            WHERE Id = $id";
                // Задержка для демонстрации блокировки
                await Task.Delay(10000);
                using var updateCommand = new SqliteCommand(updateSql, connection, transaction);
                updateCommand.Parameters.AddWithValue("$buildingSiteId", BuildingSiteId);
                updateCommand.Parameters.AddWithValue("$workTypeId", WorkTypeId);
                updateCommand.Parameters.AddWithValue("$employeeId", EmployeeId);
                updateCommand.Parameters.AddWithValue("$hoursWorked", HoursWorked);
                updateCommand.Parameters.AddWithValue("$status", Status);
                updateCommand.Parameters.AddWithValue("$id", EditId);

                var rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                // Подтверждаем транзакцию - БЛОКИРОВКА СНИМАЕТСЯ!
                await transaction.CommitAsync();
                transaction = null;

                if (rowsAffected > 0)
                {
                    TempData["Success"] = "Запись успешно обновлена";
                    Console.WriteLine($"Запись {EditId} обновлена с пессимистичной блокировкой");
                }
                else
                {
                    TempData["Error"] = "Запись не найдена";
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 5) // SQLITE_BUSY
            {
                // База заблокирована другим процессом
                if (transaction != null) await transaction.RollbackAsync();
                TempData["Error"] = "База данных занята. Попробуйте позже.";
                Console.WriteLine($"SQLite busy: {ex.Message}");
            }
            catch (Exception ex)
            {
                if (transaction != null) await transaction.RollbackAsync();
                TempData["Error"] = $"Ошибка при обновлении: {ex.Message}";
                Console.WriteLine($"Ошибка в OnPostUpdateWithPessimisticLockAsync: {ex}");
            }
            finally
            {
                // ВАЖНО: Всегда закрываем соединение
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                if (connection != null)
                {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }
            }

            return RedirectToPage("Index");
        }
        }
}