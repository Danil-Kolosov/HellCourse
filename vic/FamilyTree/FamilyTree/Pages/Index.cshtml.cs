using FamilyTree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FamilyTree.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PersonRepository _personRepo;
        private readonly FamilyConnectionRepository _connectionRepo;

        public IndexModel()
        {
            _personRepo = new PersonRepository();
            _connectionRepo = new FamilyConnectionRepository(_personRepo);
            DataSaver.LoadAllData(_personRepo, _connectionRepo);
        }

        [BindProperty]
        public Person NewPerson { get; set; } = new Person();

        [BindProperty]
        public FamilyConnection NewConnection { get; set; } = new FamilyConnection();

        [BindProperty]
        public SearchCriteria SearchCriteria { get; set; } = new SearchCriteria();

        public List<Person> AllPersons => _personRepo.GetAllPersons();
        public List<FamilyConnection> AllConnections => _connectionRepo.GetAllConnections();
        public List<Person> SearchResults { get; set; } = new List<Person>();
        public bool Searched { get; set; }

        public string Message { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPostAddPerson()
        {
            try
            {
                _personRepo.Add(NewPerson);
                DataSaver.SaveAllData(_personRepo, _connectionRepo);
                Message = "Персона успешно добавлена!";
                NewPerson = new Person();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            return Page();
        }

        public IActionResult OnPostAddConnection()
        {
            try
            {
                _connectionRepo.Add(NewConnection);
                DataSaver.SaveAllData(_personRepo, _connectionRepo);
                Message = "Родственная связь успешно добавлена!";
                NewConnection = new FamilyConnection();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            return Page();
        }

        public IActionResult OnPostDeleteConnection(int personId1, int personId2, int connectionTypeId)
        {
            try
            {
                var connections = _connectionRepo.FindConnectionsByPerson(personId1);
                var connectionToDelete = connections.FirstOrDefault(c =>
                    c.InvolvesPerson(personId2) && c.ConnectionTypeId == connectionTypeId);

                if (connectionToDelete != null)
                {
                    var allConnections = _connectionRepo.GetAllConnections();
                    var index = allConnections.IndexOf(connectionToDelete);
                    _connectionRepo.Remove(index);
                    DataSaver.SaveAllData(_personRepo, _connectionRepo);
                    Message = "Связь успешно удалена!";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            return Page();
        }

        public IActionResult OnPostSearchPersons()
        {
            try
            {
                SearchResults = _personRepo.FindPerson(
                    SearchCriteria.Surname,
                    SearchCriteria.Name,
                    SearchCriteria.LastName
                );
                Searched = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка поиска: {ex.Message}";
            }
            return Page();
        }

        public IActionResult OnPostDeletePerson(int personId)
        {
            try
            {
                var person = _personRepo.GetAllPersons().FirstOrDefault(p => p.PersonId == personId);
                if (person != null)
                {
                    var index = _personRepo.GetAllPersons().IndexOf(person);
                    _personRepo.Remove(index);
                    DataSaver.SaveAllData(_personRepo, _connectionRepo);
                    Message = "Персона успешно удалена!";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            return Page();
        }

        public IActionResult OnPostSearchPersonsAjax([FromBody] SearchCriteria criteria)
        {
            var results = _personRepo.FindPerson(criteria.Surname, criteria.Name, criteria.LastName);
            return new JsonResult(results.Select(p => new {
                p.PersonId,
                p.Surname,
                p.Name,
                p.LastName,
                p.GenderId,
                GenderName = p.GenderName,
                BirthDate = p.BirthDate.ToString("yyyy-MM-dd"),
                p.DeathDate
            }));
        }

        public Person GetPersonById(int personId)
        {
            return _personRepo.GetPerson(personId);
        }
        public IActionResult OnPostGetAllPersons()
        {
            var persons = _personRepo.GetAllPersons();
            return new JsonResult(persons.Select(p => new {
                p.PersonId,
                p.Surname,
                p.Name,
                p.LastName,
                p.GenderId,
                GenderName = p.GenderName,
                BirthDate = p.BirthDate.ToString("yyyy-MM-dd"),
                DeathDate = p.DeathDate?.ToString("yyyy-MM-dd"),
                p.Biography
            }));
        }

        public IActionResult OnPostUpdatePerson([FromBody] Person updatedPerson)
        {
            try
            {
                var allPersons = _personRepo.GetAllPersons();
                var index = allPersons.FindIndex(p => p.PersonId == updatedPerson.PersonId);
                if (index >= 0)
                {
                    _personRepo.Update(index,
                        updatedPerson.PersonId,
                        updatedPerson.Surname,
                        updatedPerson.Name,
                        updatedPerson.LastName,
                        updatedPerson.GenderId,
                        updatedPerson.BirthDate,
                        updatedPerson.DeathDate,
                        updatedPerson.Biography
                    );
                    DataSaver.SaveAllData(_personRepo, _connectionRepo);
                    return new JsonResult(new { success = true });
                }
                return new JsonResult(new { success = false, error = "Персона не найдена" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }
    }

    public class SearchCriteria
    {
        public string Surname { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
    }
}