using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Elo2.Model;
using Newtonsoft.Json;

namespace Elo2
{
    static class ManageCompetition
    {
        private static Competition _competition;
        private static IndexFile _indexFile;

        private static void OpenCompetition()
        {
            OpenCompetition(_indexFile);
        }
        public static void OpenCompetition(IndexFile index)
        {
            _indexFile = index;
            _competition = JsonConvert.DeserializeObject<Competition>(File.ReadAllText("competitions\\" + index.file + ".json"));
            if (_competition == null) _competition = new Competition();
            if (_competition.teamList == null) _competition.teamList = new List<Team>();
            if (_competition.gameList == null) _competition.gameList = new List<Game>();
            Console.Clear();
            Console.Title = index.name;
            Console.WriteLine(index.name + "\n" +
                              "\n" +
                              //TODO: Добавить вывод списка команд и их рейтинги
                              "1 - Редактирование команд\n" +
                              "2 - Редактирование игр\n" +
                              "3 - Прогнозы\n");
            var choise = Console.ReadKey();

            switch (choise.KeyChar)
            {
                case '1':
                    EditTeams();
                    break;
                case '2':
                    EditGames();
                    break;
                case '3':
                    Forecast();
                    break;
                default:
                    OpenCompetition();
                    break;
            }
        }

        public static void EditTeams()
        {
            Console.Clear();
            Console.WriteLine("Список команд:\n");
            for (int i = 0; i < _competition.teamList.Count; i++)
            {
                var team = _competition.teamList[i];
                Console.WriteLine(team.name);
            }

            Console.WriteLine("\n1 - Добавить команду\n" +
                              "2 - Сохранить и вернуться назад");

            var choise = Console.ReadKey();
            switch (choise.KeyChar)
            {
                case '1':
                    var name = "";
                    Console.Clear();
                    Console.Write("Ведите список названий. Каждое название с новой строки. Чтобы закончить ввод оставьте строку пустой.\n");
                    do
                    {
                        name = Console.ReadLine();
                        if (name != "")
                        {
                            _competition.teamList.Add(new Team()
                            {
                                id = _competition.teamList.Count == 0 ? 1 : _competition.teamList.Max(t => t.id) + 1,
                                name = name
                            });
                        }
                    } while (name != "");

                    EditTeams();
                    break;
                case '2':
                    Save();
                    OpenCompetition();
                    break;
                default:
                    EditTeams();
                    break;
            }
        }

        public static void EditGames()
        {
            Console.Clear();
            Console.WriteLine("Последние результаты: \n");
            var lastGames = _competition.gameList.OrderByDescending(g => g.id).Take(10);
            foreach (var lastGame in lastGames)
            {
                var length = GetTeam(lastGame.teamOne).Length + 3 + GetTeam(lastGame.teamTwo).Length;
                Console.WriteLine(GetTeam(lastGame.teamOne) + " - " +GetTeam(lastGame.teamTwo) + GetDelimiter(length) + lastGame.goalsOne + " - " + lastGame.goalsTwo);
            }

            Console.WriteLine("\n1 - Добавить результаты\n" +
                              "2 - Сохранить и вернуться назад\n");

            var choise = Console.ReadKey();
            switch (choise.KeyChar)
            {
                case '1':
                    var result = "";
                    Console.Clear();
                    PrintForecast(new EloCalculator());
                    Console.WriteLine("\nВедите список результатов. Каждый результат с новой строки. Ввод цифр через пробел, например '1 2 3 0' - команда 1 принимала команду 2 и сыграли со счётом 3-0. Чтобы закончить ввод оставьте строку пустой.\n");
                    do
                    {
                        result = Console.ReadLine();
                        var resultSplit = result.Split(' ');
                        if (resultSplit.Length != 4)
                        {
                            result = "";
                            continue;
                        }

                        var game = new Game()
                        {
                            id = _competition.gameList.Any() ? _competition.gameList.Max(c=>c.id) +1 : 0,
                            teamOne = Convert.ToInt32(resultSplit[0]),
                            teamTwo = Convert.ToInt32(resultSplit[1]),
                            goalsOne = Convert.ToInt32(resultSplit[2]),
                            goalsTwo = Convert.ToInt32(resultSplit[3])
                        };
                        _competition.gameList.Add(game);

                        var currentTop = Console.CursorTop;

                        Console.CursorTop = 0;
                        Console.CursorLeft = 0;
                        PrintForecast(new EloCalculator());

                        Console.CursorLeft = 0;
                        Console.CursorTop = currentTop - 1;
                        Console.WriteLine(GetTeam(game.teamOne) + " - " + GetTeam(game.teamTwo) + "\t\t" + game.goalsOne + " - " + game.goalsTwo);
                        //TODO: Добавить ввод результатов
                    } while (result != "");

                    EditGames();
                    break;
                case '2':
                    Save();
                    OpenCompetition();
                    break;
                default:
                    EditGames();
                    break;
            }
        }

        public static void Forecast()
        {
            Console.Clear();
            var calculator = new EloCalculator();
            var teamList = new TeamForecast[0];
            PrintForecast(calculator, out teamList);
            Console.WriteLine("\nВведите список команд. Пустая строка - возврат назад.\n");

            var input = "";
            do
            {
                input = Console.ReadLine();
                var resultSplit = input.Split(' ');
                if (resultSplit.Length != 2)
                {
                    continue;
                }

                var teamOne = Convert.ToInt32(resultSplit[0]);
                var teamTwo = Convert.ToInt32(resultSplit[1]);
                Console.CursorLeft = 0;
                Console.CursorTop = Console.CursorTop - 1;
                decimal fcOne, fcTwo;
                calculator.Forecast(teamList.First(t => t.Id == teamOne).Elo, teamList.First(t => t.Id == teamTwo).Elo,
                    out fcOne, out fcTwo);
                Console.WriteLine(GetTeam(teamOne) + " - " + GetTeam(teamTwo) + "\t\t" + Math.Round(fcOne, 4) + " - " +
                                  Math.Round(fcTwo, 4));
            } while (input != "");
            OpenCompetition();
        }

        public static void Save()
        {
            File.WriteAllText("competitions\\" + _indexFile.file + ".json", JsonConvert.SerializeObject(_competition));
        }

        private static void PrintForecast(EloCalculator calculator)
        {
            TeamForecast[] tf = new TeamForecast[0];
            PrintForecast(calculator, out tf);
        }
        
        private static void PrintForecast(EloCalculator calculator, out TeamForecast[] teamList)
        {

            Console.WriteLine("Команды: \n");

            teamList = _competition.teamList.Select(t => new TeamForecast() { Id = t.id, Name = t.name, Elo = 1000 }).ToArray();
            var forecastResult = calculator.CalculateForecast(teamList, _competition.gameList);
            foreach (var teamForecast in teamList.OrderByDescending(t => t.Elo))
            {
                var delimiter = " \t\t ";
                if (teamForecast.Name.Length >= 11) delimiter = " \t ";
                if (teamForecast.Elo < 1000) delimiter += " ";
                Console.Write(new string(' ', Console.WindowWidth-1));
                Console.CursorLeft = 0;
                Console.WriteLine(teamForecast.Id + " - " + teamForecast.Name + delimiter + teamForecast.Elo.ToString("0.000"));
            }
            Console.WriteLine("Delta = " + forecastResult[0].ToString("0.0000") + ", Iterations = " + forecastResult[1].ToString("0"));
        }

        private static string GetTeam(int id)
        {
            return _competition.teamList.First(t => t.id == id).name;
        }

        private static string GetDelimiter(int length)
        {
            string delimiter = "";
            if (length < 16) delimiter = "\t\t\t";
            else if (length < 24) delimiter = "\t\t";
            else delimiter = "\t";

            return delimiter;
        }
    }
}