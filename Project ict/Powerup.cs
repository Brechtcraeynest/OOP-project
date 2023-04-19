using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_ict
{
    internal class Powerup
    {
        protected int count;

        public virtual string Toevoegen()
        {
            // Zorg ervoor dat wanneer een power-up wordt toegevoegd, de teller met één wordt verhoogd
            count++;
            return count.ToString();
        }

        public virtual string Resetten()
        {
            // Zorg ervoor dat wanneer de power-up-teller wordt gereset, deze op nul wordt gezet
            count = 0;
            return count.ToString();
        }

        public virtual string Tonen()
        {
            // Zorg ervoor dat wanneer de power-up-teller wordt weergegeven, deze het totale aantal power-ups retourneert
            return $"{count}";
        }
    }
}
