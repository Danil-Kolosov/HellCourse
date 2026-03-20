using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildingCompany.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace buildingCompany.Pages.BuildingSites
{
    public class IndexModel : PageModel
    {
        private readonly string _connectionString;

        public IndexModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<BuildingSite> BuildingSites { get; set; } = new();

        [BindProperty]
        public int SiteId { get; set; }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Address { get; set; } = string.Empty;

        [BindProperty]
        public int DeleteId { get; set; }

        public bool ShowForm { get; set; }

        public async Task OnGetAsync(bool? create, int? editId, int? deleteId)
        {
            try
            {
                Console.WriteLine($"BuildingSites OnGetAsync: create={create}, editId={editId}, deleteId={deleteId}");

                // Удаление (если передан deleteId)
                if (deleteId.HasValue)
                {
                    await DeleteBuildingSiteAsync(deleteId.Value);
                }

                // Режим создания
                if (create == true)
                {
                    ShowForm = true;
                    SiteId = 0;
                    Name = string.Empty;
                    Address = string.Empty;
                }
                // Режим редактирования
                else if (editId.HasValue)
                {
                    var site = await GetBuildingSiteByIdAsync(editId.Value);
                    if (site != null)
                    {
                        ShowForm = true;
                        SiteId = site.Id;
                        Name = site.Name;
                        Address = site.Address;
                    }
                }

                // Загрузка списка
                BuildingSites = await GetAllBuildingSitesAsync();
                Console.WriteLine($"Загружено объектов: {BuildingSites.Count}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка: {ex.Message}";
                Console.WriteLine($"Ошибка в OnGetAsync: {ex}");
            }
        }

        private async Task<List<BuildingSite>> GetAllBuildingSitesAsync()
        {
            var buildingSites = new List<BuildingSite>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT Id, Name, Address FROM BuildingSites ORDER BY Id";

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

        private async Task<BuildingSite?> GetBuildingSiteByIdAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT Id, Name, Address FROM BuildingSites WHERE Id = $id";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("$id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new BuildingSite
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Address = reader.GetString(2)
                };
            }

            return null;
        }

        private async Task DeleteBuildingSiteAsync(int id)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // Проверяем наличие связанных записей в WorkLogs
                var checkLogsSql = "SELECT COUNT(*) FROM WorkLogs WHERE BuildingSiteId = $id";
                int workLogsCount = 0;

                using (var checkCommand = new SqliteCommand(checkLogsSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("$id", id);
                    workLogsCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                }

                if (workLogsCount > 0)
                {
                    TempData["Error"] =
                        $"Невозможно удалить объект. У него есть {workLogsCount} " +
                        $"записей в журнале работ. Пожалуйста, сначала удалите эти записи.";
                    return;
                }

                // Получаем имя объекта для сообщения
                var getNameSql = "SELECT Name FROM BuildingSites WHERE Id = $id";
                string? siteName = null;

                using (var getNameCommand = new SqliteCommand(getNameSql, connection))
                {
                    getNameCommand.Parameters.AddWithValue("$id", id);
                    siteName = await getNameCommand.ExecuteScalarAsync() as string;
                }

                // Удаляем объект
                var deleteSql = "DELETE FROM BuildingSites WHERE Id = $id";
                using var deleteCommand = new SqliteCommand(deleteSql, connection);
                deleteCommand.Parameters.AddWithValue("$id", id);

                var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();

                if (rowsAffected > 0 && !string.IsNullOrEmpty(siteName))
                {
                    TempData["Success"] = $"Объект {siteName} удален";
                    Console.WriteLine($"Объект {siteName} удален");
                }
                else
                {
                    TempData["Error"] = "Объект не найден";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex}");
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                Console.WriteLine("BuildingSites OnPostCreateAsync вызван");
                Console.WriteLine($"Name: '{Name}', Address: '{Address}'");

                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Address))
                {
                    Console.WriteLine("Поля пустые");
                    ModelState.AddModelError("", "Все поля обязательны для заполнения");
                    ShowForm = true;
                    BuildingSites = await GetAllBuildingSitesAsync();
                    return Page();
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO BuildingSites (Name, Address) 
                    VALUES ($name, $address);
                    SELECT last_insert_rowid();";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("$name", Name.Trim());
                command.Parameters.AddWithValue("$address", Address.Trim());

                var newId = await command.ExecuteScalarAsync();

                Console.WriteLine($"ID нового объекта: {newId}");

                TempData["Success"] = $"Объект {Name.Trim()} успешно добавлен";

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnPostCreateAsync: {ex}");
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                ShowForm = true;
                BuildingSites = await GetAllBuildingSitesAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            try
            {
                Console.WriteLine("BuildingSites OnPostUpdateAsync вызван");
                Console.WriteLine($"SiteId: {SiteId}, Name: '{Name}', Address: '{Address}'");

                if (SiteId <= 0)
                {
                    ModelState.AddModelError("", "Некорректный ID объекта");
                    ShowForm = true;
                    BuildingSites = await GetAllBuildingSitesAsync();
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Address))
                {
                    ModelState.AddModelError("", "Все поля обязательны для заполнения");
                    ShowForm = true;
                    BuildingSites = await GetAllBuildingSitesAsync();
                    return Page();
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // Проверяем существование объекта
                var checkSql = "SELECT Id FROM BuildingSites WHERE Id = $id";
                using (var checkCommand = new SqliteCommand(checkSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("$id", SiteId);
                    var exists = await checkCommand.ExecuteScalarAsync();

                    if (exists == null)
                    {
                        Console.WriteLine($"Объект с ID {SiteId} не найден");
                        ModelState.AddModelError("", "Объект не найден");
                        ShowForm = true;
                        BuildingSites = await GetAllBuildingSitesAsync();
                        return Page();
                    }
                }

                // Обновляем объект
                var updateSql = "UPDATE BuildingSites SET Name = $name, Address = $address WHERE Id = $id";
                using var updateCommand = new SqliteCommand(updateSql, connection);
                updateCommand.Parameters.AddWithValue("$name", Name.Trim());
                updateCommand.Parameters.AddWithValue("$address", Address.Trim());
                updateCommand.Parameters.AddWithValue("$id", SiteId);

                var rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                Console.WriteLine($"Rows affected: {rowsAffected}");

                if (rowsAffected > 0)
                {
                    TempData["Success"] = $"Объект {Name.Trim()} обновлен";
                }
                else
                {
                    TempData["Error"] = "Не удалось обновить объект";
                }

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnPostUpdateAsync: {ex}");
                ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
                ShowForm = true;
                BuildingSites = await GetAllBuildingSitesAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            try
            {
                Console.WriteLine($"BuildingSites OnPostDeleteAsync вызван. DeleteId: {DeleteId}");

                if (DeleteId <= 0)
                {
                    TempData["Error"] = "Некорректный ID объекта";
                    return RedirectToPage("Index");
                }

                await DeleteBuildingSiteAsync(DeleteId);

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnPostDeleteAsync: {ex}");
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
                return RedirectToPage("Index");
            }
        }
    }
}