using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildingCompany.Data;
using buildingCompany.Models;

namespace buildingCompany.Pages
{
    /// <summary>
    /// Главная страница приложения.
    /// Отображает приветствие и основную информацию о компании.
    /// 
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        // Статистика
        public int TotalBuildingSites { get; set; }
        public int TotalWorkTypes { get; set; }
        public int TotalEmployees { get; set; }
        public int ActiveWorkLogs { get; set; }

        // Данные
        public List<WorkLog> RecentWorkLogs { get; set; } = new();
        public List<BuildingSite> BuildingSites { get; set; } = new();

        // Поиск
        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Считаем статистику
            TotalBuildingSites = await _context.BuildingSites.CountAsync();
            TotalWorkTypes = await _context.WorkTypes.CountAsync();
            TotalEmployees = await _context.Employees.CountAsync();
            ActiveWorkLogs = await _context.WorkLogs
                .CountAsync(w => w.Status == "В процессе");

            // Последние 5 работ
            RecentWorkLogs = await _context.WorkLogs
                .Include(w => w.BuildingSite)
                .Include(w => w.WorkType)
                .Include(w => w.Employee)
                .OrderByDescending(w => w.Id)
                .Take(5)
                .ToListAsync();

            // Поиск объектов
            var query = _context.BuildingSites.AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(s =>
                    s.Name.Contains(SearchString) ||
                    s.Address.Contains(SearchString));
            }

            BuildingSites = await query
                .Take(5)
                .ToListAsync();
        }
    }
}