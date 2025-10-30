using System;
using System.Collections.Generic;

namespace FamilyTree
{
    public class FamilyConnection: Data
    {
        private int personId1;
        private int personId2;
        private int connectionTypeId;

        public int PersonId1
        {
            get => personId1;
            set
            {
                personId1 = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }
        public int PersonId2
        {
            get => personId2;
            set
            {
                personId2 = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }
        public int ConnectionTypeId
        {
            get => connectionTypeId;
            set
            {
                connectionTypeId = value;
                UpdateModifiedDate(); // Автоматическое обновление!
            }
        }

        public FamilyConnection() { }

        public FamilyConnection(int pId1, int pId2, int estConTypeId)
        {
            PersonId1 = pId1;
            PersonId2 = pId2;
            ConnectionTypeId = estConTypeId;
            UpdateModifiedDate(); // Автоматическое обновление!
        }

        public void UpdateFamilyConnection(int pId1, int pId2, int estConTypeId)
        {
            PersonId1 = pId1;
            PersonId2 = pId2;
            ConnectionTypeId = estConTypeId;
            UpdateModifiedDate(); // Автоматическое обновление!
        }

        public List<string> GetFamilyConnectionInfo(PersonRepository personRepo)
        {
            var person1 = personRepo.GetPerson(PersonId1);
            var person2 = personRepo.GetPerson(PersonId2);
            var connectionTypeName = ConnectionType.GetConnectionTypeName(ConnectionTypeId);

            return new List<string>
        {
            $"Связь: {connectionTypeName}",
            $"Персона 1: {person1?.Surname} {person1?.Name} {person1?.LastName}",
            $"Персона 2: {person2?.Surname} {person2?.Name} {person2?.LastName}"
        };
        }

        public bool InvolvesPerson(int personId)
        {
            return PersonId1 == personId || PersonId2 == personId;
        }

        public int GetOtherPersonId(int personId)
        {
            if (PersonId1 == personId) return PersonId2;
            if (PersonId2 == personId) return PersonId1;
            throw new ArgumentException("Персона не участвует в этой связи");
        }
    }
}
