using Boganbefaling_eksamen.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using CsvHelper;

namespace Boganbefaling_eksamen
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ImporterJSON _importer;
        private List<Publikation> _publikationer;
        private List<string> _muligeGenrer;
        private List<string> _valgteGenrer;
        private List<Publikation> _anbefaledePublikationer;


        //Tråd som viser tiden
        private DispatcherTimer _saveTimer; 
        private DispatcherTimer _timer;
        private RandomSearchThread _randomSearchThread;
        private SearchStatistics _searchStatistics;

        private DateTime _currentDateTime;
        public DateTime CurrentDateTime
        {
            get { return _currentDateTime; }
            set
            {
                if (_currentDateTime != value)
                {
                    _currentDateTime = value;
                    OnPropertyChanged(nameof(CurrentDateTime));
                }
            }
        }
        //Holder vidst styr på genrene som jeg random genererer?
        public string SearchCounts
        {
            get { return _searchStatistics != null ? _searchStatistics.TotalSearches.ToString() : "0"; }
        }

        public ICommand StartDateTimeCommand { get; }
        public ICommand StopDateTimeCommand { get; }
        public ICommand StartRandomSearchesCommand { get; }
        public ICommand StopRandomSearchesCommand { get; }
        public ICommand SaveSearchHistoryCommand { get; }
        public ICommand AnbefalCommand { get; }
        public ICommand ShowPublicationDetailCommand { get; } //Jeg har forsøgt, og fejlet, på at lave et nyt vindue, når brugeren klikker på en given titel. 
                                                              //Jeg ville gerne have, at et nyt vindue åbnede, som viser al infoen, og som er bundet op på mine childklasser.

        //Anbefal bøger

        public List<Publikation> Publikationer
        {
            get { return _publikationer; }
            set
            {
                _publikationer = value;
                OnPropertyChanged(nameof(Publikationer));
            }
        }

        public List<string> MuligeGenrer
        {
            get { return _muligeGenrer; }
            set
            {
                _muligeGenrer = value;
                OnPropertyChanged(nameof(MuligeGenrer));
            }
        }

        public List<string> ValgteGenrer
        {
            get { return _valgteGenrer; }
            set
            {
                _valgteGenrer = value;
                OnPropertyChanged(nameof(ValgteGenrer));
            }
        }

        public List<Publikation> AnbefaledePublikationer
        {
            get { return _anbefaledePublikationer; }
            set
            {
                _anbefaledePublikationer = value;
                OnPropertyChanged(nameof(AnbefaledePublikationer));
            }
        }


        

        public MainViewModel()
        {
            _importer = new ImporterJSON(@"C:\Users\Amand\Desktop\Eksamen_VP\PublikationData.json");
            Publikationer = _importer.ImporterPublikationer();
            MuligeGenrer = _importer.HentGenrertilWPF();
            ValgteGenrer = new List<string>();

            AnbefalCommand = new RelayCommand(AnbefalBoeger);

            // ShowPublicationDetailCommand = new RelayCommand(ShowPublicationDetail);//også projektet med det nye vindue

            //tråd der viser tiden
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            //vise historik til brugeren
            _searchStatistics = new SearchStatistics(MuligeGenrer);
            InitializeSearchCount();
            _searchStatistics.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SearchStatistics.TotalSearches))
                {
                    OnPropertyChanged(nameof(TotalSearches));
                }
            };

            _randomSearchThread = new RandomSearchThread(_searchStatistics);
            _randomSearchThread.Start();

            //gemme i csv - bruges til at gemme til csv fil hvert minut (indeholder total antal søgninger i dag + randomgenereret søgninger 
            _saveTimer = new DispatcherTimer(); //fortsat - men logikken er off, så den viser kun random genrer-søgning når programmet er åbent, og kun for én søgning)
            _saveTimer.Interval = TimeSpan.FromMinutes(1);
            _saveTimer.Tick += SaveTimer_Tick;
            _saveTimer.Start();

            SaveSearchHistoryCommand = new RelayCommand(SaveSearchHistory);

        }

        public int TotalSearches
        {
            get { return _searchStatistics.TotalSearches; }
        }

        private void InitializeSearchCount()
        {
            int currentHour = DateTime.Now.Hour;
            int realisticSearchCount = new Random().Next(10, 50) * currentHour;
            for (int i = 0; i < realisticSearchCount; i++)
            {
                _searchStatistics.AddSearch(new List<string> { "" });
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentDateTime = DateTime.Now;
            OnPropertyChanged(nameof(CurrentDateTime));
        }

        private void SaveTimer_Tick(object sender, EventArgs e)
        {
            {
                SaveSearchHistory();
            }
        }

        private void SaveSearchHistory()
        {
            string folderPath = @"C:\Users\Amand\Desktop\Eksamen_VP\SearchHistory.csv";
            Directory.CreateDirectory(folderPath);

            string fileName = $"SearchHistory{DateTime.Now:yyyyMMdd}.csv";
            string filePath = Path.Combine(folderPath, fileName);

            var searchHistoryData = _searchStatistics.GetSearchHistoryData();
            CsvHelper.WriteSearchHistoryToCsv(filePath, searchHistoryData);
        }

        //Anbefal bøger

        private void AnbefalBoeger()
        {
            if (ValgteGenrer.Any())
            {
                AnbefaledePublikationer = Publikationer
                    .Select(p =>
                    {
                        double matchCount = p.Genrer.Count(g => ValgteGenrer.Contains(g, StringComparer.OrdinalIgnoreCase));
                        p.MatchPercentage = matchCount / ValgteGenrer.Count;
                        return p;
                    })
                    .Where(p => p.MatchPercentage > 0)
                    .OrderByDescending(p => p.MatchPercentage)
                    .ToList();

                _searchStatistics.AddSearch(ValgteGenrer);
            }
            else
            {
                AnbefaledePublikationer = new List<Publikation>();
            }
        }

        //private void ShowPublicationDetail(object parameter)
        //{
        //    if (parameter is Publikation selectedPublication)
        //    {
        //        var viewModel = new PublicationDetailViewModel(selectedPublication);
        //        var view = new PublicationDetailView
        //        {
        //            DataContext = viewModel
        //        };
        //        view.ShowDialog();
        //    }
        //} Del af forsøg på at lave nyt vindue med al info på en publikation

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
