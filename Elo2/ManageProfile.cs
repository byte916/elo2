using System;
using System.Collections.Generic;
using System.IO;
using Elo2.Model;
using Newtonsoft.Json;

namespace Elo2
{
    static class ManageProfile
    {
        public static void Run()
        {
            Console.Clear();
            Console.Title = "Калькулятор ЭЛО rev 10 от 24.01.2020";
            Console.WriteLine(
                "Что будем делать?\n" +
                "\n" +
                "1 - Создать соревнование\n" +
                "2 - Открыть соревнование\n");

            var choise = Console.ReadKey();

            switch (choise.KeyChar)
            {
                case '1':
                    CreateCompetition();
                    break;
                case '2':
                    OpenCompetition();
                    break;
                default:
                    Run();
                    break;
            }
        }

        public static void CreateCompetition()
        {
            Console.Clear();
            Console.WriteLine("Введите название соревнование");
            var name = Console.ReadLine();
            // Если нет папки с соревнованиями
            if (!Directory.Exists("competitions"))
            {
                Directory.CreateDirectory("competitions");
            }

            // Если нет файла со списком соревнований
            if (!File.Exists("competitions\\index.json"))
            {
                File.Create("competitions\\index.json").Close();
                
                File.WriteAllText("competitions\\index.json", "[]");
            }

            var indexString = File.ReadAllText("competitions\\index.json");

            var index = JsonConvert.DeserializeObject<List<IndexFile>>(indexString);

            var guid = Guid.NewGuid().ToString();

            index.Add(new IndexFile() {file = guid, name = name});

            File.WriteAllText("competitions\\index.json", JsonConvert.SerializeObject(index));
            File.Create("competitions\\" + guid + ".json").Close();
            File.WriteAllText("competitions\\" + guid + ".json", "{}");
            Run();
        }

        public static void OpenCompetition()
        {
            Console.Clear();
            // Если нет файла со списком
            if (!File.Exists("competitions\\index.json"))
            {
                Console.WriteLine("Соревнований не найдено");
                Run();
                return;
            }

            var indexString = File.ReadAllText("competitions\\index.json");

            var index = JsonConvert.DeserializeObject<List<IndexFile>>(indexString);
            if (index.Count == 0)
            {
                Console.WriteLine("Соревнований не найдено");
                Run();
                return;
            }

            for (var i = 0; i < index.Count; i++)
            {
                var indexFile = index[i];
                Console.WriteLine(i+1 + " - " + indexFile.name);
            }

            var choice = Console.ReadKey();
            if (!int.TryParse(choice.KeyChar.ToString(), out var choiceNumber))
            {
                OpenCompetition();
                return;
            }
            //var choise = Convert.ToInt16(Console.ReadLine()); 
            
            // Если выбранный вариант вне диапазона
            if (choiceNumber < 1 || choiceNumber > index.Count)
            {
                OpenCompetition();
                return;
            }

            ManageCompetition.OpenCompetition(index[choiceNumber - 1]);
        }
    }
}