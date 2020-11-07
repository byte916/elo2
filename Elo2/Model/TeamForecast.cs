using System;
using System.Collections.Generic;
using System.Text;

namespace Elo2.Model
{
    class TeamForecast
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Elo { get; set; }
    }
}
