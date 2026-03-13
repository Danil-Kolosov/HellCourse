//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.EntityFrameworkCore;
//using buildingCompany.Models;
//using buildingCompany.Data;

//namespace buildingCompany.Pages.BuildingSites
//{
//    public class IndexModel : PageModel
//    {
//        private readonly AppDbContext _context;

//        public IndexModel(AppDbContext context)
//        {
//            _context = context;
//        }

//        public List<BuildingSite> BuildingSites { get; set; } = new();

//        [BindProperty]
//        public BuildingSite EditingSite { get; set; } = new();

//        public bool ShowForm { get; set; }

//        public async Task OnGetAsync(bool? create, int? editId, int? deleteId)
//        {
//            if (create == true)
//            {
//                ShowForm = true;
//                EditingSite = new BuildingSite();
//            }

//            if (editId.HasValue)
//            {
//                var site = await _context.BuildingSites.FindAsync(editId);
//                if (site != null)
//                {
//                    EditingSite = site;
//                    ShowForm = true;
//                }
//            }

//            if (deleteId.HasValue)
//            {
//                var site = await _context.BuildingSites.FindAsync(deleteId);
//                if (site != null)
//                {
//                    _context.BuildingSites.Remove(site);
//                    await _context.SaveChangesAsync();
//                }
//            }

//            BuildingSites = await _context.BuildingSites.ToListAsync();
//        }

//        public async Task<IActionResult> OnPostCreateAsync()
//        {
//            if (!ModelState.IsValid)
//            {
//                ShowForm = true;
//                BuildingSites = await _context.BuildingSites.ToListAsync();
//                return Page();
//            }

//            _context.BuildingSites.Add(EditingSite);
//            await _context.SaveChangesAsync();
//            return RedirectToPage("Index");
//        }

//        public async Task<IActionResult> OnPostUpdateAsync()
//        {
//            if (!ModelState.IsValid)
//            {
//                ShowForm = true;
//                BuildingSites = await _context.BuildingSites.ToListAsync();
//                return Page();
//            }

//            _context.Attach(EditingSite).State = EntityState.Modified;
//            await _context.SaveChangesAsync();
//            return RedirectToPage("Index");
//        }
//    }
//}