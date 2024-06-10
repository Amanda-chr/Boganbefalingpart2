using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boganbefaling_eksamen
{
    public class Publikation : IMedie
    {
        public string Type { get; set; }
        public string Titel { get; set; }
        public string Forfatter { get; set; }
        public List<string> Genrer { get; set; }
        public int PublikationsAar { get; set; }
        public string Udgiver { get; set; }
        public double MatchPercentage { get; set; }
    }
    public static class PublikationFactory //har lavet det som en factory
    {
        public static IMedie LavPublikation(string type)
        {
            switch (type.ToLower())
            {
                case "tegneserie":
                    return new Tegneserie();
                case "bog":
                    return new Bog();
                case "lydbog":
                    return new Lydbog();
                default:
                    throw new ArgumentException("Ugyldig medie type");
            } //i forhold til hvor meget kode jeg har rundt omkring der gør det her, så akn jeg stadig ikke gennemskue det smarte....
        }
    }
}
