using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildingCompany.Models;
using buildingCompany.Data;

namespace buildingCompany.Pages.BuildingSites
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
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
                    var site = await _context.BuildingSites.FindAsync(deleteId.Value);
                    if (site != null)
                    {
                        _context.BuildingSites.Remove(site);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = $"Объект {site.Name} удален";
                    }
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
                    var site = await _context.BuildingSites.FindAsync(editId.Value);
                    if (site != null)
                    {
                        ShowForm = true;
                        SiteId = site.Id;
                        Name = site.Name;
                        Address = site.Address;
                    }
                }

                // Загрузка списка
                BuildingSites = await _context.BuildingSites.ToListAsync();
                Console.WriteLine($"Загружено объектов: {BuildingSites.Count}");
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
                Console.WriteLine("BuildingSites OnPostCreateAsync вызван");
                Console.WriteLine($"Name: '{Name}', Address: '{Address}'");

                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Address))
                {
                    Console.WriteLine("Поля пустые");
                    ModelState.AddModelError("", "Все поля обязательны для заполнения");
                    ShowForm = true;
                    BuildingSites = await _context.BuildingSites.ToListAsync();
                    return Page();
                }

                var site = new BuildingSite
                {
                    Name = Name.Trim(),
                    Address = Address.Trim()
                };

                Console.WriteLine($"Добавление объекта: {site.Name}, {site.Address}");

                _context.BuildingSites.Add(site);
                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"SaveChangesAsync результат: {result}");
                Console.WriteLine($"ID нового объекта: {site.Id}");

                if (result > 0)
                {
                    TempData["Success"] = $"Объект {site.Name} успешно добавлен";
                }
                else
                {
                    TempData["Error"] = "Не удалось добавить объект";
                }

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnPostCreateAsync: {ex}");
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                ShowForm = true;
                BuildingSites = await _context.BuildingSites.ToListAsync();
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
                    BuildingSites = await _context.BuildingSites.ToListAsync();
                    return Page();
                }

                var site = await _context.BuildingSites.FindAsync(SiteId);
                if (site == null)
                {
                    Console.WriteLine($"Объект  не найден");
                    ModelState.AddModelError("", "Объект не найден");
                    ShowForm = true;
                    BuildingSites = await _context.BuildingSites.ToListAsync();
                    return Page();
                }

                site.Name = Name.Trim();
                site.Address = Address.Trim();

                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"SaveChangesAsync результат: {result}");

                if (result > 0)
                {
                    TempData["Success"] = $"Объект {site.Name} обновлен";
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
                BuildingSites = await _context.BuildingSites.ToListAsync();
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

                var site = await _context.BuildingSites.FindAsync(DeleteId);
                if (site != null)
                {
                    _context.BuildingSites.Remove(site);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Объект {site.Name} удален";
                    Console.WriteLine($"Объект {site.Name} удален");
                }
                else
                {
                    TempData["Error"] = "Объект не найден";
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
}