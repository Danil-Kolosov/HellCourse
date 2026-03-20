using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildingCompany.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace buildingCompany.Pages.WorkTypes
{
    public class IndexModel : PageModel
    {
        private readonly string _connectionString;

        public IndexModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<WorkType> Items { get; set; } = new();

        [BindProperty]
        public int ItemId { get; set; }

        [BindProperty]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        public decimal PricePerHour { get; set; }

        [BindProperty]
        public int DeleteId { get; set; }

        public bool ShowForm { get; set; }

        public async Task OnGetAsync(bool? create, int? editId, int? deleteId)
        {
            try
            {
                Console.WriteLine($"WorkTypes OnGetAsync: create={create}, editId={editId}, deleteId={deleteId}");

                // Удаление (если передан deleteId)
                if (deleteId.HasValue)
                {
                    await DeleteWorkTypeAsync(deleteId.Value);
                }

                // Режим создания
                if (create == true)
                {
                    ShowForm = true;
                    ItemId = 0;
                    Title = string.Empty;
                    PricePerHour = 0;
                }
                // Режим редактирования
                else if (editId.HasValue)
                {
                    var item = await GetWorkTypeByIdAsync(editId.Value);
                    if (item != null)
                    {
                        ShowForm = true;
                        ItemId = item.Id;
                        Title = item.Title;
                        PricePerHour = item.PricePerHour;
                    }
                }

                // Загрузка списка
                Items = await GetAllWorkTypesAsync();
                Console.WriteLine($"Загружено видов работ: {Items.Count}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка: {ex.Message}";
                Console.WriteLine($"Ошибка в OnGetAsync: {ex}");
            }
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

        private async Task<WorkType?> GetWorkTypeByIdAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT Id, Title, PricePerHour FROM WorkTypes WHERE Id = $id";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("$id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new WorkType
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    PricePerHour = reader.GetDecimal(2)
                };
            }

            return null;
        }

        private async Task DeleteWorkTypeAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Проверяем наличие связанных записей в WorkLogs
            var checkLogsSql = "SELECT COUNT(*) FROM WorkLogs WHERE WorkTypeId = $id";
            int workLogsCount = 0;

            using (var checkCommand = new SqliteCommand(checkLogsSql, connection))
            {
                checkCommand.Parameters.AddWithValue("$id", id);
                workLogsCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
            }

            if (workLogsCount > 0)
            {
                TempData["Error"] =
                    $"Невозможно удалить вид работ. Он используется в {workLogsCount} " +
                    $"записях журнала работ. Пожалуйста, сначала удалите эти записи.";
                return;
            }

            // Получаем название для сообщения
            var getNameSql = "SELECT Title FROM WorkTypes WHERE Id = $id";
            string? title = null;

            using (var getNameCommand = new SqliteCommand(getNameSql, connection))
            {
                getNameCommand.Parameters.AddWithValue("$id", id);
                title = await getNameCommand.ExecuteScalarAsync() as string;
            }

            // Удаляем запись
            var deleteSql = "DELETE FROM WorkTypes WHERE Id = $id";
            using var deleteCommand = new SqliteCommand(deleteSql, connection);
            deleteCommand.Parameters.AddWithValue("$id", id);

            var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();

            if (rowsAffected > 0 && !string.IsNullOrEmpty(title))
            {
                TempData["Success"] = $"Вид работ {title} удален";
                Console.WriteLine($"Вид работ {title} удален");
            }
            else
            {
                TempData["Error"] = "Запись не найдена";
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                Console.WriteLine("WorkTypes OnPostCreateAsync вызван");
                Console.WriteLine($"Title: '{Title}', PricePerHour: {PricePerHour}");

                if (string.IsNullOrWhiteSpace(Title) || PricePerHour <= 0)
                {
                    Console.WriteLine("Поля некорректны");
                    ModelState.AddModelError("", "Все поля должны быть заполнены корректно");
                    ShowForm = true;
                    Items = await GetAllWorkTypesAsync();
                    return Page();
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO WorkTypes (Title, PricePerHour) 
                    VALUES ($title, $pricePerHour);
                    SELECT last_insert_rowid();";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("$title", Title.Trim());
                command.Parameters.AddWithValue("$pricePerHour", PricePerHour);

                var newId = await command.ExecuteScalarAsync();

                Console.WriteLine($"ID нового вида работ: {newId}");

                TempData["Success"] = $"Вид работ {Title.Trim()} успешно добавлен";

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnPostCreateAsync: {ex}");
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                ShowForm = true;
                Items = await GetAllWorkTypesAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            try
            {
                Console.WriteLine("WorkTypes OnPostUpdateAsync вызван");
                Console.WriteLine($"ItemId: {ItemId}, Title: '{Title}', PricePerHour: {PricePerHour}");

                if (ItemId <= 0)
                {
                    ModelState.AddModelError("", "Некорректный ID");
                    ShowForm = true;
                    Items = await GetAllWorkTypesAsync();
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Title) || PricePerHour <= 0)
                {
                    ModelState.AddModelError("", "Все поля должны быть заполнены корректно");
                    ShowForm = true;
                    Items = await GetAllWorkTypesAsync();
                    return Page();
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // Проверяем существование записи
                var checkSql = "SELECT Id FROM WorkTypes WHERE Id = $id";
                using (var checkCommand = new SqliteCommand(checkSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("$id", ItemId);
                    var exists = await checkCommand.ExecuteScalarAsync();

                    if (exists == null)
                    {
                        Console.WriteLine($"Вид работ с ID {ItemId} не найден");
                        ModelState.AddModelError("", "Запись не найдена");
                        ShowForm = true;
                        Items = await GetAllWorkTypesAsync();
                        return Page();
                    }
                }

                // Обновляем запись
                var updateSql = "UPDATE WorkTypes SET Title = $title, PricePerHour = $pricePerHour WHERE Id = $id";
                using var updateCommand = new SqliteCommand(updateSql, connection);
                updateCommand.Parameters.AddWithValue("$title", Title.Trim());
                updateCommand.Parameters.AddWithValue("$pricePerHour", PricePerHour);
                updateCommand.Parameters.AddWithValue("$id", ItemId);

                var rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                Console.WriteLine($"SaveChangesAsync результат: {rowsAffected}");

                if (rowsAffected > 0)
                {
                    TempData["Success"] = $"Вид работ {Title.Trim()} обновлен";
                }
                else
                {
                    TempData["Error"] = "Не удалось обновить вид работ";
                }

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnPostUpdateAsync: {ex}");
                ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
                ShowForm = true;
                Items = await GetAllWorkTypesAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            try
            {
                Console.WriteLine($"WorkTypes OnPostDeleteAsync вызван. DeleteId: {DeleteId}");

                if (DeleteId <= 0)
                {
                    TempData["Error"] = "Некорректный ID";
                    return RedirectToPage("Index");
                }

                await DeleteWorkTypeAsync(DeleteId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnPostDeleteAsync: {ex}");
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
            }

            return RedirectToPage("Index");
        }
    }
}