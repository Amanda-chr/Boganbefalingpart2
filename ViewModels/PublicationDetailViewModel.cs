using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boganbefaling_eksamen
{
    public class PublicationDetailViewModel : ViewModelBase //Jeg ved ikke om jeg skal bruge den? Lavede den ifm. mit forsøg på at lave et nyt vindue, der kan vise al info om den enkelte publikation.
    {
        private Publikation _selectedPublication;

        public Publikation SelectedPublication
        {
            get => _selectedPublication;
            set
            {
                _selectedPublication = value;
                OnPropertyChanged(nameof(SelectedPublication));
            }
        }

        public PublicationDetailViewModel(Publikation publication)
        {
            SelectedPublication = publication;
        }
    }
}
