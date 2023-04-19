using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_ict
{
    internal class Coins : Powerup
    {
        public override string Toevoegen()
        {
            // Roep de basisimplementatie van Toevoegen aan om de teller met één te verhogen
            base.Toevoegen();
            return count.ToString();
        }

        public override string Resetten()
        {
            // Roep de basisimplementatie van Resetten aan om de teller op nul te zetten
            base.Resetten();
            return count.ToString();
        }

        public override string Tonen()
        {
            // Roep de basisimplementatie van Tonen aan om het totaal aantal power-ups te retourneren
            return base.Tonen();
        }
    }
}
