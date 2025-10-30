using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace FamilyTree
{
    public class FamilyConnectionRepository
    {
        private List<FamilyConnection> familyConnections = new List<FamilyConnection>();
        public int Size => familyConnections.Count;

        private PersonRepository personRepo;

        public FamilyConnectionRepository(PersonRepository personRepository)
        {
            personRepo = personRepository;
        }

        public void Add(FamilyConnection connection)
        {
            // Вся валидация теперь в одном месте
            Validator.ValidateFamilyConnection(connection, personRepo, this); //он о персон хранилище пока не знает - если делать что бы занл - то на диаграмме просто лапку стрелку в персон лист из этого, типо знает о нем

            familyConnections.Add(connection);
        }

        public bool Remove(int index)
        {
            if (index >= 0 && index < familyConnections.Count)
            {
                familyConnections.RemoveAt(index);
                return true;
            }
            return false;
        }

        public int GetSize() => familyConnections.Count;

        public bool Update(int index, int pId1, int pId2, int estConTypeId)
        {
            if (index >= 0 && index < familyConnections.Count)
            {
                familyConnections[index].UpdateFamilyConnection(pId1, pId2, estConTypeId);
                return true;
            }
            return false;
        }

        public FamilyConnection GetConnection(int index)
        {
            if (index >= 0 && index < familyConnections.Count)
                return familyConnections[index];
            return null;
        }

        public List<FamilyConnection> FindConnectionsByPerson(int personId)
        {
            return familyConnections.Where(fc => fc.InvolvesPerson(personId)).ToList();
        }

        public List<FamilyConnection> FindConnectionsByPersonType(int personId, int connectionTypeId)
        {
            // Проверка валидности типа связи
            if (!ConnectionType.IsValidConnectionType(connectionTypeId))
            {
                throw new ArgumentException("Неверный тип родственной связи");
            }

            return familyConnections.Where(fc =>
                fc.InvolvesPerson(personId) && fc.ConnectionTypeId == connectionTypeId).ToList();
        }

        public List<FamilyConnection> GetAllAncestorsOfPerson(int personId)
        {
            var ancestors = new List<FamilyConnection>();
            var visited = new HashSet<int>();
            FindAncestorsRecursive(personId, ancestors, visited);
            return ancestors;
        }

        private void FindAncestorsRecursive(int currentPersonId, List<FamilyConnection> ancestors, HashSet<int> visited)
        {
            if (!visited.Add(currentPersonId)) return;

            // Ищем связи "родитель-ребенок", где текущий персона - ребенок
            var parentConnections = familyConnections.Where(fc =>
                fc.PersonId2 == currentPersonId && fc.ConnectionTypeId == 2).ToList(); // 2 = родитель-ребенок

            foreach (var connection in parentConnections)
            {
                ancestors.Add(connection);
                FindAncestorsRecursive(connection.PersonId1, ancestors, visited);
            }
        }

        public List<FamilyConnection> GetAllDescendantsOfPerson(int personId)
        {
            var descendants = new List<FamilyConnection>();
            var visited = new HashSet<int>();
            FindDescendantsRecursive(personId, descendants, visited);
            return descendants;
        }

        private void FindDescendantsRecursive(int currentPersonId, List<FamilyConnection> descendants, HashSet<int> visited)
        {
            if (!visited.Add(currentPersonId)) return;

            // Ищем связи "родитель-ребенок", где текущий персона - родитель
            var childConnections = familyConnections.Where(fc =>
                fc.PersonId1 == currentPersonId && fc.ConnectionTypeId == 2).ToList(); // 2 = родитель-ребенок

            foreach (var connection in childConnections)
            {
                descendants.Add(connection);
                FindDescendantsRecursive(connection.PersonId2, descendants, visited);
            }
        }

        public List<FamilyConnection> GetChainOfFamilyConnections(int personId1, int personId2)
        {
            // Проверка параметров поиска
            Validator.ValidateSearchParameters(personId1, personId2);

            // BFS для поиска кратчайшего пути в графе родственных связей
            var queue = new Queue<List<FamilyConnection>>();
            var visited = new HashSet<int> { personId1 };

            // Начальные связи от первой персоны
            var initialConnections = FindConnectionsByPerson(personId1);
            foreach (var connection in initialConnections)
            {
                queue.Enqueue(new List<FamilyConnection> { connection });
            }

            while (queue.Count > 0)
            {
                var currentPath = queue.Dequeue();
                var lastConnection = currentPath.Last();
                var lastPersonId = lastConnection.GetOtherPersonId(
                    currentPath.Count > 1 ? currentPath[^2].GetOtherPersonId(personId1) : personId1
                );

                if (lastPersonId == personId2)
                    return currentPath;

                var nextConnections = FindConnectionsByPerson(lastPersonId);
                foreach (var nextConnection in nextConnections)
                {
                    if (!visited.Contains(nextConnection.GetOtherPersonId(lastPersonId)))
                    {
                        visited.Add(nextConnection.GetOtherPersonId(lastPersonId));
                        var newPath = new List<FamilyConnection>(currentPath) { nextConnection };
                        queue.Enqueue(newPath);
                    }
                }
            }

            return new List<FamilyConnection>(); // Путь не найден
        }

        public List<FamilyConnection> GetAllConnections() => new List<FamilyConnection>(familyConnections);

        // Сохранение в файл
        public void SaveToFile(string filePath)
        {
            var json = JsonSerializer.Serialize(familyConnections, new JsonSerializerOptions
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
                var loadedConnections = JsonSerializer.Deserialize<List<FamilyConnection>>(json);
                familyConnections.Clear();
                familyConnections.AddRange(loadedConnections);
            }
        }
    }
}
