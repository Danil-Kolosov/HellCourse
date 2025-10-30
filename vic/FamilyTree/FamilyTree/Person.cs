using System.Xml.Linq;

namespace FamilyTree
{
    public class Person : Data
    {
        private string name;
        public int personId;
        public string surname;
        public string lastName;
        public int genderId;
        public string genderName;
        public DateOnly birthDate;
        public DateOnly? deathDate;
        public string biography;
        public int PersonId
        {
            get => personId;
            set
            {
                personId = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }
        public string Surname
        {
            get => surname;
            set
            {
                surname = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }
        public string Name
        {
            get => name;
            set
            {
                name = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }
        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }
        public int GenderId
        {
            get => genderId;
            set
            {
                genderId = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }
        public string GenderName => Gender.GetGenderName(GenderId);
        public DateOnly BirthDate
        {
            get => birthDate;
            set
            {
                birthDate = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }
        public DateOnly? DeathDate
        {
            get => deathDate;
            set
            {
                deathDate = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }
        public string Biography
        {
            get => biography;
            set
            {
                biography = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }

        public Person()
        {
            PersonId = 0; // По умолчанию 0 - будет установлен автоинкрементом
        }

        public Person(string surname, string name, string lastName,
                      int genderId, DateOnly birthDate, DateOnly? deathDate, string biography)
        {
            PersonId = 0; // Будет установлен автоинкрементом
            Surname = surname;
            Name = name;
            LastName = lastName;
            GenderId = genderId;
            BirthDate = birthDate;
            DeathDate = deathDate;
            Biography = biography;

            // Валидация при создании объекта
            Validator.ValidatePerson(this);
            UpdateModifiedDate(); // Автоматическое обновление!
        }

        public List<string> GetFullInfo()
        {
            return new List<string>
        {
            $"ID: {PersonId}",
            $"ФИО: {Surname} {Name} {LastName}",
            $"Пол: {GenderName}",
            $"Дата рождения: {BirthDate:dd.MM.yyyy}",
            $"Дата смерти: {(DeathDate.HasValue ? DeathDate.Value.ToString("dd.MM.yyyy") : "н/д")}",
            $"Биография: {Biography}"
        };
        }

        public List<string> GetShortInfo()
        {
            return new List<string>
        {
            $"{Surname} {Name} {LastName}",
            $"{BirthDate:dd.MM.yyyy} - {(DeathDate.HasValue ? DeathDate.Value.ToString("dd.MM.yyyy") : "н/д")}"
        };
        }

        //public int? CalculateAge()
        //{
        //    var today = DateOnly.FromDateTime(DateTime.Today);
        //    var endDate = DeathDate ?? today;

        //    if (BirthDate > endDate) return null;

        //    var age = endDate.Year - BirthDate.Year;
        //    if (BirthDate > endDate.AddYears(-age)) age--;
        //    return age;
        //}
    }
}
