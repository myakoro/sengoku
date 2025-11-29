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
        private object _dialogViewModel;

        public MainViewModel()
        {
            _gameService = new GameService();
            _gameService.PropertyChanged += OnServicePropertyChanged;
            _gameService.OnMonthlyProcessed += OnMonthlyProcessed;

            // Initial View
            CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask);
            
            NavigateDailyCommand = new RelayCommand(() => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask));
            NavigateVillageCommand = new RelayCommand(() => CurrentView = new VillageViewModel(_gameService));
        }

        private void NavigateToPublicDuty()
        {
            CurrentView = new PublicDutySelectionViewModel(_gameService, 
                () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask),
                OnActionExecuted);
        }

        private void NavigateToPrivateTask()
        {
            CurrentView = new PrivateTaskSelectionViewModel(_gameService, 
                () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask),
                OnActionExecuted);
        }

        private void OnActionExecuted(string taskName, string target)
        {
            // Show Result Dialog
            // For simplicity in this MVVM setup without a dialog service, we'll switch the view to the Result View
            // In a real app, this might be a modal window.
            // But per specs, "Action Result Dialog" is a screen/overlay.
            // Let's implement it as a View switching for now, or we can use a Popup in the View.
            // Given the constraints, switching view is safest to ensure it works.
            // Wait, the spec says "Dialog". 
            // Let's try to use a separate property for DialogViewModel to show it as an overlay in MainWindow.
            
            // Actually, let's stick to View switching for simplicity and robustness if "Dialog" is just a name.
            // But "Dialog" implies overlay. 
            // Let's add a DialogViewModel property to MainViewModel.
            
            var log = _gameService.DailyLogs[0]; // The latest log
            DialogViewModel = new ActionResultViewModel(log, () => DialogViewModel = null);
            
            // Also return to Main Screen behind the dialog
            CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask);
        }

        private void OnServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameService.CurrentDate))
            {
                OnPropertyChanged(nameof(CurrentDateDisplay));
            }
            if (e.PropertyName == nameof(GameService.HasPublicDutyThisMonth))
            {
                OnPropertyChanged(nameof(IsPublicDutyDone));
            }
        }

        private void OnMonthlyProcessed(object sender, MonthlySummary summary)
        {
            // Switch to Monthly Summary
            CurrentView = new MonthlySummaryViewModel(_gameService, summary, () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask));
        }

        public Player Player => _gameService.Player;
        public string CurrentDateDisplay => _gameService.CurrentDate.ToString("D");
        public bool IsPublicDutyDone => _gameService.HasPublicDutyThisMonth;
        public ObservableCollection<string> Logs => _gameService.Logs;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public object DialogViewModel
        {
            get => _dialogViewModel;
            set { _dialogViewModel = value; OnPropertyChanged(); }
        }

        public ICommand NavigateDailyCommand { get; }
        public ICommand NavigateVillageCommand { get; }
    }

    public class MainScreenViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand NavigatePublicDutyCommand { get; }
        public ICommand NavigatePrivateTaskCommand { get; }

        public MainScreenViewModel(GameService service, Action onPublicDuty, Action onPrivateTask)
        {
            _gameService = service;
            _gameService.PropertyChanged += OnServicePropertyChanged;
            NavigatePublicDutyCommand = new RelayCommand(onPublicDuty);
            NavigatePrivateTaskCommand = new RelayCommand(onPrivateTask);
        }

        private void OnServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameService.CurrentDate))
            {
                OnPropertyChanged(nameof(CurrentDateDisplay));
            }
            if (e.PropertyName == nameof(GameService.HasPublicDutyThisMonth))
            {
                OnPropertyChanged(nameof(IsPublicDutyDone));
            }
        }

        public GameService Service => _gameService;
        public string CurrentDateDisplay => $"{_gameService.CurrentDate.Year}年 {_gameService.CurrentDate.Month}月 {_gameService.CurrentDate.Day}日";
        public bool IsPublicDutyDone => _gameService.HasPublicDutyThisMonth;
    }

    public class PublicDutySelectionViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand ExecuteSecurityMaintenanceCommand { get; }
        public ICommand ExecutePatrolCommand { get; }
        public ICommand ExecuteLandSurveyCommand { get; }
        public ICommand ExecuteConstructionCommand { get; }
        public ICommand ExecuteDocumentCreationCommand { get; }
        public ICommand ExecuteInformationGatheringCommand { get; }
        public ICommand BackCommand { get; }

        public PublicDutySelectionViewModel(GameService service, Action onBack, Action<string, string> onActionExecuted)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);

            ExecuteSecurityMaintenanceCommand = new RelayCommandWithParam(p => {
                var target = p as string; // "村A" or "村B"
                var village = target == "村A" ? _gameService.VillageA : _gameService.VillageB;
                _gameService.ExecuteSecurityMaintenance(village);
                onActionExecuted("治安維持", target);
            });

            ExecutePatrolCommand = new RelayCommandWithParam(p => {
                var target = p as string;
                var village = target == "村A" ? _gameService.VillageA : _gameService.VillageB;
                _gameService.ExecutePatrol(village);
                onActionExecuted("巡察", target);
            });

            ExecuteLandSurveyCommand = new RelayCommandWithParam(p => {
                var target = p as string;
                var village = target == "村A" ? _gameService.VillageA : _gameService.VillageB;
                _gameService.ExecuteLandSurvey(village);
                onActionExecuted("検地補助", target);
            });

            ExecuteConstructionCommand = new RelayCommandWithParam(p => {
                var target = p as string;
                var village = target == "村A" ? _gameService.VillageA : _gameService.VillageB;
                _gameService.ExecuteConstruction(village);
                onActionExecuted("普請補助", target);
            });

            ExecuteDocumentCreationCommand = new RelayCommand(() => {
                _gameService.ExecuteDocumentCreation();
                onActionExecuted("書状作成", "");
            });

            ExecuteInformationGatheringCommand = new RelayCommand(() => {
                _gameService.ExecuteInformationGathering();
                onActionExecuted("情報収集", "");
            });
        }
    }

    public class PrivateTaskSelectionViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand ExecuteVillageDevelopmentCommand { get; }
        public ICommand ExecuteRoadMaintenanceCommand { get; }
        public ICommand ExecuteVillageMerchantTradeCommand { get; }
        public ICommand ExecuteSpecialtyPreparationCommand { get; }
        public ICommand BackCommand { get; }

        public PrivateTaskSelectionViewModel(GameService service, Action onBack, Action<string, string> onActionExecuted)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);

            ExecuteVillageDevelopmentCommand = new RelayCommandWithParam(p => {
                var target = p as string;
                var village = target == "村A" ? _gameService.VillageA : _gameService.VillageB;
                _gameService.ExecuteVillageDevelopment(village);
                onActionExecuted("知行村の開発", target);
            });

            ExecuteRoadMaintenanceCommand = new RelayCommandWithParam(p => {
                var target = p as string;
                var village = target == "村A" ? _gameService.VillageA : _gameService.VillageB;
                _gameService.ExecuteRoadMaintenance(village);
                onActionExecuted("道整備", target);
            });

            ExecuteVillageMerchantTradeCommand = new RelayCommandWithParam(p => {
                var target = p as string;
                var village = target == "村A" ? _gameService.VillageA : _gameService.VillageB;
                _gameService.ExecuteVillageMerchantTrade(village);
                onActionExecuted("村商人との取引", target);
            });

            ExecuteSpecialtyPreparationCommand = new RelayCommandWithParam(p => {
                var target = p as string;
                var village = target == "村A" ? _gameService.VillageA : _gameService.VillageB;
                _gameService.ExecuteSpecialtyPreparation(village);
                onActionExecuted("名産開発の準備", target);
            });
        }
    }

    public class ActionResultViewModel : ViewModelBase
    {
        public DailyLog Log { get; }
        public ICommand CloseCommand { get; }

        public ActionResultViewModel(DailyLog log, Action onClose)
        {
            Log = log;
            CloseCommand = new RelayCommand(onClose);
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
        public MonthlySummary Summary { get; }
        public ICommand NextMonthCommand { get; }

        public MonthlySummaryViewModel(GameService service, MonthlySummary summary, Action onNext)
        {
            Service = service;
            Summary = summary;
            NextMonthCommand = new RelayCommand(onNext);
        }
    }
}
