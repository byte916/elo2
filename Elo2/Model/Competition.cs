using System;
using System.Collections.Generic;
using System.Text;

namespace Elo2.Model
{
    class Competition
    {
        public List<Team> teamList { get; set; }
        public List<Game> gameList { get; set; }
    }

    class Team
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    class Game
    {
        public int id { get; set; }
        public int teamOne { get; set; }
        public int teamTwo { get; set; }
        public int goalsOne { get; set; }
        public int goalsTwo { get; set; }
    }
}