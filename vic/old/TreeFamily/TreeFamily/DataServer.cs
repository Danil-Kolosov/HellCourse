using System;
using System.IO;
//using System.Text.;

namespace FamilyTree
{
    public static class DataSaver
    {
        public static void SaveAllData(PersonRepository personRepo,
                                     FamilyConnectionRepository connectionRepo,
                                     string basePath = "data")
        {
            try
            {
                // Создаем папку если нет
                Directory.CreateDirectory(basePath);

                // Сохраняем все в отдельные файлы
                personRepo.SaveToFile(Path.Combine(basePath, "persons.json"));
                connectionRepo.SaveToFile(Path.Combine(basePath, "connections.json"));

                Console.WriteLine("Данные успешно сохранены!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        public static void LoadAllData(PersonRepository personRepo,
                                     FamilyConnectionRepository connectionRepo,
                                     string basePath = "data")
        {
            try
            {
                personRepo.LoadFromFile(Path.Combine(basePath, "persons.json"));
                connectionRepo.LoadFromFile(Path.Combine(basePath, "connections.json"));

                Console.WriteLine("Данные успешно загружены!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
            }
        }
    }
}
