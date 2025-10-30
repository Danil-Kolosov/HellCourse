namespace FamilyTree
{
    public class Person
    {
        public int PersonId { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public int GenderId { get; set; }
        public string GenderName => Gender.GetGenderName(GenderId);
        public DateOnly BirthDate { get; set; }
        public DateOnly? DeathDate { get; set; }
        public string Biography { get; set; }

        public Person() { }

        public Person(int personId, string surname, string name, string lastName,
                      int genderId, DateOnly birthDate, DateOnly? deathDate, string biography)
        {
            PersonId = personId;
            Surname = surname;
            Name = name;
            LastName = lastName;
            GenderId = genderId;
            BirthDate = birthDate;
            DeathDate = deathDate;
            Biography = biography;

            // Валидация при создании объекта
            Validator.ValidatePerson(this);
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

        public int? CalculateAge()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var endDate = DeathDate ?? today;

            if (BirthDate > endDate) return null;

            var age = endDate.Year - BirthDate.Year;
            if (BirthDate > endDate.AddYears(-age)) age--;
            return age;
        }
    }
}
