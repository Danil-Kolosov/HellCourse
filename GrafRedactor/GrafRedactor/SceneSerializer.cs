using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace GrafRedactor
{
    public class SceneSerializer
    {
        public static void SaveToFile(string filePath, List<FigureElement> elements, GroupManager groupManager)
        {
            // Сохраняем индексы элементов вместо ID
            var sceneData = new JObject
            {
                ["Version"] = "1.0",
                ["Figures"] = new JArray(),
                ["GroupManagerData"] = groupManager.SerializeForSave(elements)
            };

            // Сохраняем все фигуры
            var figuresArray = (JArray)sceneData["Figures"];
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                var elementJson = element.Serialize();
                elementJson["Index"] = i; // Сохраняем индекс для связи
                figuresArray.Add(elementJson);
            }

            string json = JsonConvert.SerializeObject(sceneData, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static (List<FigureElement> Figures, GroupManager GroupManager) LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден", filePath);

            string json = File.ReadAllText(filePath);
            var sceneData = JObject.Parse(json);

            // 1. Восстанавливаем ВСЕ фигуры
            var figures = new List<FigureElement>();
            var figuresArray = (JArray)sceneData["Figures"];

            // Сначала создаем все фигуры
            foreach (JObject elementData in figuresArray)
            {
                var element = FigureElement.CreateFromData(elementData);
                figures.Add(element);
            }

            // 2. Восстанавливаем группы, передавая ссылки на созданные фигуры
            var groupManager = new GroupManager();
            groupManager.DeserializeFromLoad(
                (JObject)sceneData["GroupManagerData"],
                figures);

            return (figures, groupManager);
        }
    }
}
