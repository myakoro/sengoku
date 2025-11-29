using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SengokuSLG.Models;
using SengokuSLG.Services;
using System.Collections.ObjectModel;

namespace SengokuSLG.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class RelayCommandWithParam : ICommand
    {
        private readonly Action<object> _execute;
        public RelayCommandWithParam(Action<object> execute) { _execute = execute; }
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        private object _currentView;

        public MainViewModel()
        {
            _gameService = new GameService();
            _gameService.PropertyChanged += OnServicePropertyChanged;
            _gameService.OnMonthlyProcessed += OnMonthlyProcessed;

            // Initial View
            CurrentView = new DailyActionViewModel(_gameService);
            
            NavigateDailyCommand = new RelayCommand(() => CurrentView = new DailyActionViewModel(_gameService));
            NavigateVillageCommand = new RelayCommand(() => CurrentView = new VillageViewModel(_gameService));
        }

        private void OnServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameService.CurrentDate))
            {
                OnPropertyChanged(nameof(CurrentDateDisplay));
            }
            if (e.PropertyName == nameof(GameService.HasDonePublicDuty))
            {
                OnPropertyChanged(nameof(IsPublicDutyDone));
            }
        }

        private void OnMonthlyProcessed(object sender, EventArgs e)
        {
            // Switch to Monthly Summary
            CurrentView = new MonthlySummaryViewModel(_gameService, () => CurrentView = new DailyActionViewModel(_gameService));
        }

        public Player Player => _gameService.Player;
        public string CurrentDateDisplay => _gameService.CurrentDate.ToString("D"); // e.g. "Monday, November 29, 2025" -> Japanese locale needed for "天正..." but standard date for now.
        // To match mock "天正15年3月12日", I should format it manually or use a custom calendar.
        // For v0.3, I'll stick to standard or simple string format.
        // Mock: "天正15年3月12日". 1587 is Tensho 15.
        // I'll just format it simply.
        
        public bool IsPublicDutyDone => _gameService.HasDonePublicDuty;
        public ObservableCollection<string> Logs => _gameService.Logs;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand NavigateDailyCommand { get; }
        public ICommand NavigateVillageCommand { get; }
    }

    public class DailyActionViewModel : ViewModelBase
    {
        private readonly GameService _gameService;

        public DailyActionViewModel(GameService service)
        {
            _gameService = service;
            ExecuteActionCommand = new RelayCommandWithParam(Execute);
        }

        public ICommand ExecuteActionCommand { get; }

        private void Execute(object parameter)
        {
            var args = parameter as string; 
            if (string.IsNullOrEmpty(args)) return;
            var parts = args.Split('|');
            _gameService.ExecuteAction(parts[0], parts.Length > 1 ? parts[1] : "");
        }
    }

    public class VillageViewModel : ViewModelBase
    {
        public GameService Service { get; }
        public VillageViewModel(GameService service) { Service = service; }
    }

    public class MonthlySummaryViewModel : ViewModelBase
    {
        public GameService Service { get; }
        public ICommand NextMonthCommand { get; }

        public MonthlySummaryViewModel(GameService service, Action onNext)
        {
            Service = service;
            NextMonthCommand = new RelayCommand(onNext);
        }
    }
}
