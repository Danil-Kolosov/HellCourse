using System;
using System.Reflection;
using System.Text.Json;

namespace FamilyTree
{
    public class PersonRepository
    {
        private List<Person> personList = new List<Person>();
        private int nextPersonId = 1; // Счетчик для автоинкремента
        public int Size => personList.Count;

        public void Add(Person person)
        {
            // Автоматически назначаем ID только если он не установлен
            if (person.PersonId == 0)
            {
                person.PersonId = nextPersonId++;
            }
            else
            {
                // Если ID уже установлен, обновляем nextPersonId
                if (person.PersonId >= nextPersonId)
                {
                    nextPersonId = person.PersonId + 1;
                }
            }
            // Валидация данных
            Validator.ValidatePerson(person);

            if (personList.Any(p => p.PersonId == person.PersonId))
                throw new ArgumentException($"Персона с ID {person.PersonId} уже существует");              

            personList.Add(person);
        }        

        public int GetSize() => personList.Count;

        public bool Update(int index, int personId, string surname, string name, string lastName,
                           string gender, DateOnly birthDate, DateOnly? deathDate, string biography)
        {
            int genderId = Gender.GetGenderNumber(gender);
            if (genderId == -1)
                return false;

            if (index >= 0 && index < personList.Count)
            {
                // Создаем временный объект для валидации
                var tempPerson = new Person(surname, name, lastName, genderId, birthDate, deathDate, biography);
                Validator.ValidatePerson(tempPerson);
                personList[index] = tempPerson;
                return true;
            }
            return false;
        }

        public bool Update(int index, int personId, string surname, string name, string lastName,
                           int gender, DateOnly birthDate, DateOnly? deathDate, string biography)
        {           
            if (index >= 0 && index < personList.Count)
            {
                // Создаем временный объект для валидации
                var tempPerson = new Person(surname, name, lastName, gender, birthDate, deathDate, biography);
                Validator.ValidatePerson(tempPerson);
                personList[index] = tempPerson;
                return true;
            }
            return false;
        }

        public Person GetPerson(int personId/*index*/)
        {
            return personList.FirstOrDefault(p => p.PersonId == personId);
            //if (index >= 0 && index < personList.Count)
            //    return personList[index];
            //return null;
        }

        public Person FindPerson(int index/*personId*/)
        {
            if (index >= 0 && index < personList.Count)
                return personList[index];
            return null;
            //return personList.FirstOrDefault(p => p.PersonId == personId);
        }

        public List<Person> FindPerson(string surname = null, string name = null, string lastName = null,
                                       string gender = null, DateOnly? birthDate = null,
                                       DateOnly? deathDate = null, string biography = null)
        {
            return personList.Where(p =>
                (surname == null || p.Surname.Contains(surname)) &&
                (name == null || p.Name.Contains(name)) &&
                (lastName == null || p.LastName.Contains(lastName)) &&
                (gender == null || p.GenderName == gender) &&
                (!birthDate.HasValue || p.BirthDate == birthDate.Value) &&
                (!deathDate.HasValue || p.DeathDate == deathDate.Value) &&
                (biography == null || p.Biography.Contains(biography))
            ).ToList();
        }

        public List<Person> GetAllPersons() => new List<Person>(personList);

        // Сохранение в файл
        public void SaveToFile(string filePath)
        {
            var json = JsonSerializer.Serialize(personList, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, json);
        }

        // Загрузка из файла
        public void LoadFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var loadedPersons = JsonSerializer.Deserialize<List<Person>>(json);
                if (loadedPersons != null && loadedPersons.Any())
                {
                    personList.Clear();
                    personList.AddRange(loadedPersons);
                    // Устанавливаем nextPersonId как максимальный ID + 1
                    nextPersonId = loadedPersons.Max(p => p.PersonId) + 1;
                }
                else
                {
                    personList.Clear();
                    nextPersonId = 1;
                }
            }
        }
    }
}
