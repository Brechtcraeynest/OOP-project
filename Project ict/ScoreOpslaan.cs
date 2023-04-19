using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_ict
{
    internal class ScoreOpslaan
    {
        public string Name { get; set; }
        public int Score { get; set; }

        public ScoreOpslaan(string name, int score)
        {
            Name = name;
            Score = score;
        }
    }
}
