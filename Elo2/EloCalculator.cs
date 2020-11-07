using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Elo2.Model;

namespace Elo2
{
    class EloCalculator
    {

        public List<decimal> CalculateForecast(TeamForecast[] teamList, List<Game> gameList)
        {
            var delta = Convert.ToDecimal(ConfigurationManager.AppSettings["delta"]);
            var iteration = Convert.ToInt32(ConfigurationManager.AppSettings["maxIteration"]);
            var currentElo = teamList.ToDictionary(t => t.Id, t => t.Elo);
            decimal currentDelta = 0;
            for (int i = 1; i < iteration; i++)
            {
                CalculateForecastIteration(teamList, gameList);
                
                var newElo = teamList.ToDictionary(t=> t.Id, t=>t.Elo);
                currentDelta = 0;
                foreach (var @decimal in currentElo)
                {
                    currentDelta += Math.Abs(newElo.First(e => e.Key == @decimal.Key).Value - @decimal.Value);
                }
                currentElo = newElo;
                currentDelta /= currentElo.Count;
                if (i != 1 && currentDelta <= delta)
                {
                    return new List<decimal> {currentDelta, i};
                }

            }
            return new List<decimal> { currentDelta, iteration };
        }

        /// <summary> Посчитать Эло для всего соревнования </summary>
        /// <param name="teamList">Список команд. В этот же список записываются результаты рассчёта</param>
        /// <param name="gameList"></param>
        private void CalculateForecastIteration(TeamForecast[] teamList, List<Game> gameList)
        {
            foreach (var game in gameList)
            {
                var teamOne = teamList.First(t => t.Id == game.teamOne);
                var teamTwo = teamList.First(t => t.Id == game.teamTwo);
                var eloOne = teamOne.Elo;
                var eloTwo = teamTwo.Elo;
                Calculate(ref eloOne, ref eloTwo, game.goalsOne, game.goalsTwo);
                teamOne.Elo = eloOne;
                teamTwo.Elo = eloTwo;
            }
        }

        /// <summary>
        /// Посчитать Эло для двух команд
        /// </summary>
        /// <param name="eloOne">Эло команды 1</param>
        /// <param name="eloTwo">Эло команды 2</param>
        /// <param name="goalsOne">Количество голов забитых командой 1</param>
        /// <param name="goalsTwo">Количество голов забитых командой 2</param>
        public void Calculate(ref decimal eloOne, ref decimal eloTwo, int goalsOne, int goalsTwo)
        {
            // Принцип расчёта взят отсюда - https://ru.wikipedia.org/wiki/%D0%A4%D1%83%D1%82%D0%B1%D0%BE%D0%BB%D1%8C%D0%BD%D1%8B%D0%B9_%D1%80%D0%B5%D0%B9%D1%82%D0%B8%D0%BD%D0%B3_%D0%AD%D0%BB%D0%BE#%D0%91%D0%B0%D0%B7%D0%BE%D0%B2%D1%8B%D0%B5_%D0%BF%D1%80%D0%B8%D0%BD%D1%86%D0%B8%D0%BF%D1%8B_%D1%80%D0%B0%D1%81%D1%87%D0%B5%D1%82%D0%B0

            // Индекс разности голов
            decimal g = 0;
            if (Math.Abs(goalsOne - goalsTwo) <= 1)
            {
                g = 1;
            } else if (Math.Abs(goalsOne - goalsTwo) == 2)
            {
                g = 1.5m;
            }
            else
            {
                g = (11 + Math.Abs(goalsOne - goalsTwo)) / 8m;
            }
            // Индекс весомости матча. 30 Для обычных игр.
            int k = 30;
            // Результат матча для обоих команд
            decimal wOne, wTwo;
            if (goalsOne > goalsTwo)
            {
                wOne = 1;
                wTwo = 0;
            }
            else if (goalsOne == goalsTwo)
            {
                wOne = 0.5m;
                wTwo = 0.5m;
            }
            else
            {
                wOne = 0;
                wTwo = 1;
            }
            // Ожидаемый результат
            decimal weOne = GetWe(eloOne, eloTwo);
            decimal weTwo = GetWe(eloTwo, eloOne);

            eloOne = eloOne + k * g * (wOne - weOne);
            eloTwo = eloTwo + k * g * (wTwo - weTwo);
        }

        /// <summary>
        /// Получить ожидаемый результат для команды one
        /// </summary>
        /// <returns></returns>
        private decimal GetWe(decimal eloOne, decimal eloTwo)
        {
            return 1m / (decimal)(Math.Pow(10d, -1 * (((double)eloOne - (double)eloTwo) / 400)) + 1);
        }

        public void Forecast(decimal eloOne, decimal eloTwo, out decimal fcOne, out decimal fcTwo)
        {
            fcOne = GetWe(eloOne, eloTwo);
            fcTwo = GetWe(eloTwo, eloOne);
        }
    }
}