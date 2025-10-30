namespace FamilyTree
{
    public static class Validator
    {
        // Проверки для Person
        public static void ValidatePerson(Person person)
        {
            ValidateBirthDate(person.BirthDate);
            ValidateDeathDate(person.BirthDate, person.DeathDate);
            ValidateGender(person.GenderId);
            ValidateNames(person.Surname, person.Name, person.LastName);
        }

        public static void ValidateBirthDate(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (birthDate > today)
            {
                throw new ArgumentException("Дата рождения не может быть в будущем");
            }
        }

        public static void ValidateDeathDate(DateOnly birthDate, DateOnly? deathDate)
        {
            if (deathDate.HasValue)
            {
                if (deathDate.Value < birthDate)
                {
                    throw new ArgumentException("Дата смерти не может быть раньше даты рождения");
                }

                var today = DateOnly.FromDateTime(DateTime.Today);
                if (deathDate.Value > today)
                {
                    throw new ArgumentException("Дата смерти не может быть в будущем");
                }
            }
        }

        public static void ValidateGender(int genderId)
        {
            if (!Gender.IsValidGenderId(genderId))
            {
                throw new ArgumentException("Неверный идентификатор пола");
            }
        }

        public static void ValidateNames(string surname, string name, string lastName)
        {
            if (string.IsNullOrWhiteSpace(surname))
            {
                throw new ArgumentException("Фамилия не может быть пустой");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Имя не может быть пустым");
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException("Отчество не может быть пустым");
            }
        }

        // Проверки для FamilyConnection
        public static void ValidateFamilyConnection(FamilyConnection connection, PersonRepository personRepo, FamilyConnectionRepository connectionRepo)
        {
            ValidateConnectionType(connection.ConnectionTypeId);
            ValidateNotSelfConnection(connection.PersonId1, connection.PersonId2);
            ValidatePersonsExist(connection.PersonId1, connection.PersonId2, personRepo);
            ValidateUniqueConnection(connection, connectionRepo);

            // Специфичные проверки для типов связей
            switch (connection.ConnectionTypeId)
            {
                case 1: // Мужья-жены
                    ValidateMarriage(connection, personRepo);
                    break;
                case 2: // Родители-дети
                    ValidateParentChild(connection, personRepo, connectionRepo);
                    break;
                case 3: // Братья-сестры
                    ValidateSiblings(connection, personRepo);
                    break;
            }
        }

        public static void ValidateConnectionType(int connectionTypeId)
        {
            if (!ConnectionType.IsValidConnectionType(connectionTypeId))
            {
                throw new ArgumentException("Неверный тип родственной связи");
            }
        }

        public static void ValidateNotSelfConnection(int personId1, int personId2)
        {
            if (personId1 == personId2)
            {
                throw new ArgumentException("Нельзя создать родственную связь человека с самим собой");
            }
        }

        public static void ValidatePersonsExist(int personId1, int personId2, PersonRepository personRepo)
        {
            var person1 = personRepo.GetPerson(personId1);
            var person2 = personRepo.GetPerson(personId2);

            if (person1 == null)
            {
                throw new ArgumentException($"Персона с ID {personId1} не найдена");
            }

            if (person2 == null)
            {
                throw new ArgumentException($"Персона с ID {personId2} не найдена");
            }
        }

        public static void ValidateUniqueConnection(FamilyConnection newConnection, FamilyConnectionRepository connectionRepo)
        {
            var existingConnections = connectionRepo.GetAllConnections();
            if (existingConnections.Any(fc =>
                (fc.PersonId1 == newConnection.PersonId1 && fc.PersonId2 == newConnection.PersonId2 && fc.ConnectionTypeId == newConnection.ConnectionTypeId) ||
                (fc.PersonId1 == newConnection.PersonId2 && fc.PersonId2 == newConnection.PersonId1 && fc.ConnectionTypeId == newConnection.ConnectionTypeId)))
            {
                throw new ArgumentException("Такая связь уже существует");
            }
        }

        // Проверки для конкретных типов связей
        private static void ValidateMarriage(FamilyConnection connection, PersonRepository personRepo)
        {
            var person1 = personRepo.GetPerson(connection.PersonId1);
            var person2 = personRepo.GetPerson(connection.PersonId2);

            if (person1.GenderId == person2.GenderId)
            {
                throw new ArgumentException("Нельзя создать супружескую связь между людьми одного пола");
            }

            // Проверка возраста для брака (например, минимум 18 лет)
            //var minMarriageAge = 18;
            //if (person1.CalculateAge() < minMarriageAge || person2.CalculateAge() < minMarriageAge)
            //{
            //    throw new ArgumentException("Оба супруга должны быть совершеннолетними");
            //}
        }

        private static void ValidateParentChild(FamilyConnection connection, PersonRepository personRepo, FamilyConnectionRepository connectionRepo)
        {
            var parent = personRepo.GetPerson(connection.PersonId1);
            var child = personRepo.GetPerson(connection.PersonId2);

            // Проверка, что родитель старше ребенка минимум на 15 лет
            //var minParentAge = 15;
            //if (parent.BirthDate > child.BirthDate.AddYears(-minParentAge))
            //{
            //    throw new ArgumentException("Родитель должен быть старше ребенка минимум на 15 лет");
            //}

            // Проверка на циклические связи (предок-потомок)
            var ancestorsOfParent = connectionRepo.GetAllAncestorsOfPerson(connection.PersonId1);
            if (ancestorsOfParent.Any(conn => conn.InvolvesPerson(connection.PersonId2)))
            {
                throw new ArgumentException($"Нельзя создать связь: персона {connection.PersonId2} уже является предком {connection.PersonId1}");
            }

            var ancestorsOfChild = connectionRepo.GetAllAncestorsOfPerson(connection.PersonId2);
            if (ancestorsOfChild.Any(conn => conn.InvolvesPerson(connection.PersonId1)))
            {
                throw new ArgumentException($"Нельзя создать связь: персона {connection.PersonId1} уже является предком {connection.PersonId2}");
            }
        }

        private static void ValidateSiblings(FamilyConnection connection, PersonRepository personRepo)
        {
            var person1 = personRepo.GetPerson(connection.PersonId1);
            var person2 = personRepo.GetPerson(connection.PersonId2);

            // Братья/сестры должны иметь разницу в возрасте не более 50 лет (например)
            //var maxAgeDifference = 50;
            //var ageDifference = Math.Abs(person1.BirthDate.Year - person2.BirthDate.Year);
            //if (ageDifference > maxAgeDifference)
            //{
            //    throw new ArgumentException("Слишком большая разница в возрасте для братьев/сестер");
            //}
        }

        // Проверки для методов поиска
        public static void ValidateSearchParameters(int personId1, int personId2)
        {
            if (personId1 == personId2)
            {
                throw new ArgumentException("Нельзя искать родственную связь человека с самим собой");
            }
        }
    }
}
