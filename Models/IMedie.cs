using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boganbefaling_eksamen
{
    public interface IMedie
    {
        string Type { get; set; }
        string Titel { get; set; }
        string Forfatter { get; set; }
        List<string> Genrer { get; set; }
        int PublikationsAar { get; set; }
        string Udgiver { get; set; }
    }
}
