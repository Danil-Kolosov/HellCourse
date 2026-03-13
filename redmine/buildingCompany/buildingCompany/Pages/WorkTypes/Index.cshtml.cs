using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildingCompany.Models;
using buildingCompany.Data;

namespace buildingCompany.Pages.WorkTypes
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
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
                    var item = await _context.WorkTypes.FindAsync(deleteId.Value);
                    if (item != null)
                    {
                        _context.WorkTypes.Remove(item);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = $"Вид работ {item.Title} удален";
                    }
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
                    var item = await _context.WorkTypes.FindAsync(editId.Value);
                    if (item != null)
                    {
                        ShowForm = true;
                        ItemId = item.Id;
                        Title = item.Title;
                        PricePerHour = item.PricePerHour;
                    }
                }

                // Загрузка списка
                Items = await _context.WorkTypes.ToListAsync();
                Console.WriteLine($"Загружено видов работ: {Items.Count}");
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
                Console.WriteLine("WorkTypes OnPostCreateAsync вызван");
                Console.WriteLine($"Title: '{Title}', PricePerHour: {PricePerHour}");

                if (string.IsNullOrWhiteSpace(Title) || PricePerHour <= 0)
                {
                    Console.WriteLine("Поля некорректны");
                    ModelState.AddModelError("", "Все поля должны быть заполнены корректно");
                    ShowForm = true;
                    Items = await _context.WorkTypes.ToListAsync();
                    return Page();
                }

                var item = new WorkType
                {
                    Title = Title.Trim(),
                    PricePerHour = PricePerHour
                };

                Console.WriteLine($"Добавление вида работ: {item.Title}, {item.PricePerHour}");

                _context.WorkTypes.Add(item);
                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"SaveChangesAsync результат: {result}");
                Console.WriteLine($"ID нового вида работ: {item.Id}");

                if (result > 0)
                {
                    TempData["Success"] = $"Вид работ {item.Title} успешно добавлен";
                }
                else
                {
                    TempData["Error"] = "Не удалось добавить вид работ";
                }

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnPostCreateAsync: {ex}");
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                ShowForm = true;
                Items = await _context.WorkTypes.ToListAsync();
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
                    Items = await _context.WorkTypes.ToListAsync();
                    return Page();
                }

                var item = await _context.WorkTypes.FindAsync(ItemId);
                if (item == null)
                {
                    Console.WriteLine($"Вид работ не найден");
                    ModelState.AddModelError("", "Запись не найдена");
                    ShowForm = true;
                    Items = await _context.WorkTypes.ToListAsync();
                    return Page();
                }

                item.Title = Title.Trim();
                item.PricePerHour = PricePerHour;

                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"SaveChangesAsync результат: {result}");

                if (result > 0)
                {
                    TempData["Success"] = $"Вид работ {item.Title} обновлен";
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
                Items = await _context.WorkTypes.ToListAsync();
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

                var item = await _context.WorkTypes.FindAsync(DeleteId);
                if (item != null)
                {
                    _context.WorkTypes.Remove(item);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Вид работ {item.Title} удален";
                    Console.WriteLine($"Вид работ {item.Title} удален");
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
    }
}