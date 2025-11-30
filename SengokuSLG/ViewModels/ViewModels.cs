using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SengokuSLG.Models;
using SengokuSLG.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

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
            CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting);
            
            NavigateDailyCommand = new RelayCommand(() => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting));
            NavigateVillageCommand = new RelayCommand(() => CurrentView = new VillageViewModel(_gameService));
            NavigateVassalListCommand = new RelayCommand(NavigateToVassalList);
        }

        private void NavigateToPublicDuty()
        {
            CurrentView = new PublicDutySelectionViewModel(_gameService, 
                () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting),
                OnActionExecuted);
        }

        private void NavigateToPrivateTask()
        {
            CurrentView = new PrivateTaskSelectionViewModel(_gameService, 
                () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting),
                OnActionExecuted);
        }

        private void NavigateToVassalList()
        {
            CurrentView = new VassalListViewModel(_gameService, 
                () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting),
                NavigateToVassalDetail);
        }

        private void NavigateToVassalDetail(string vassalId)
        {
            CurrentView = new VassalDetailViewModel(_gameService, vassalId,
                () => CurrentView = new VassalListViewModel(_gameService, 
                    () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting),
                    NavigateToVassalDetail));
        }

        private void NavigateToAdvisorSetting()
        {
            CurrentView = new AdvisorSettingViewModel(_gameService,
                () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting));
        }

        private void OnActionExecuted(string taskName, string target)
        {
            var log = _gameService.DailyLogs[0];
            DialogViewModel = new ActionResultViewModel(log, () => {
                DialogViewModel = null;
                _gameService.AdvanceDay();
            });
            CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting);
        }

        private void OnServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameService.CurrentDate) || 
                e.PropertyName == nameof(GameService.CurrentDateDisplay))
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
            CurrentView = new MonthlySummaryViewModel(_gameService, summary, () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting));
        }

        public Player Player => _gameService.Player;
        public string CurrentDateDisplay => _gameService.CurrentDateDisplay;
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
        public ICommand NavigateVassalListCommand { get; }
    }

    public class MainScreenViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand NavigatePublicDutyCommand { get; }
        public ICommand NavigatePrivateTaskCommand { get; }
        public ICommand NavigateVassalListCommand { get; }
        public ICommand NavigateAdvisorSettingCommand { get; }
        public ICommand ExecuteBattleCommand { get; }

        public MainScreenViewModel(GameService service, Action onPublicDuty, Action onPrivateTask, Action onVassalList, Action onAdvisorSetting)
        {
            _gameService = service;
            _gameService.PropertyChanged += OnServicePropertyChanged;
            NavigatePublicDutyCommand = new RelayCommand(onPublicDuty);
            NavigatePrivateTaskCommand = new RelayCommand(onPrivateTask);
            NavigateVassalListCommand = new RelayCommand(onVassalList);
            NavigateAdvisorSettingCommand = new RelayCommand(onAdvisorSetting);
            
            // Debug/Simulate Battle Command
            ExecuteBattleCommand = new RelayCommand(() => {
                _gameService.ExecuteBattle();
            });
        }

        private void OnServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameService.CurrentDate) || 
                e.PropertyName == nameof(GameService.CurrentDateDisplay))
            {
                OnPropertyChanged(nameof(CurrentDateDisplay));
            }
            if (e.PropertyName == nameof(GameService.HasPublicDutyThisMonth))
            {
                OnPropertyChanged(nameof(IsPublicDutyDone));
            }
        }

        public GameService Service => _gameService;
        public string CurrentDateDisplay => _gameService.CurrentDateDisplay;
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
                var target = p as string;
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

    public class VassalListViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        private ICollectionView _vassalView;

        public ICollectionView VassalView => _vassalView;
        public ICommand BackCommand { get; }
        public ICommand DetailCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand PromoteToToshiCommand { get; }
        public ICommand PromoteToKumigashiraCommand { get; }

        public VassalListViewModel(GameService service, Action onBack, Action<string> onDetail)
        {
            _gameService = service;
            _vassalView = CollectionViewSource.GetDefaultView(_gameService.Vassals);
            
            BackCommand = new RelayCommand(onBack);
            DetailCommand = new RelayCommandWithParam(id => onDetail((string)id));
            
            FilterCommand = new RelayCommandWithParam(param => {
                string filter = param as string;
                _vassalView.Filter = v => {
                    if (string.IsNullOrEmpty(filter) || filter == "All") return true;
                    var vassal = v as Vassal;
                    if (vassal == null) return false;
                    
                    if (filter == "Juboku") return vassal.Rank == Rank.Juboku;
                    if (filter == "Toshi") return vassal.Rank == Rank.Toshi;
                    if (filter == "Kumigashira") return vassal.Rank == Rank.Kumigashira;
                    if (filter == "Busho") return vassal.Rank == Rank.Busho;
                    return true;
                };
                _vassalView.Refresh();
            });

            PromoteToToshiCommand = new RelayCommandWithParam(id => {
                _gameService.PromoteVassal((string)id, Rank.Toshi);
            });
            
            PromoteToKumigashiraCommand = new RelayCommandWithParam(id => {
                _gameService.PromoteVassal((string)id, Rank.Kumigashira);
            });
        }
    }

    public class VassalDetailViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public Vassal Vassal { get; }
        public ICommand BackCommand { get; }
        public ICommand AppointAdvisorCommand { get; }

        public VassalDetailViewModel(GameService service, string vassalId, Action onBack)
        {
            _gameService = service;
            Vassal = _gameService.Vassals.FirstOrDefault(v => v.Id == vassalId);
            BackCommand = new RelayCommand(onBack);
            
            AppointAdvisorCommand = new RelayCommand(() => {
                _gameService.AppointAdvisor(vassalId);
            });
        }
    }

    public class AdvisorSettingViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ObservableCollection<Vassal> Vassals => _gameService.Vassals;
        public ICommand BackCommand { get; }
        public ICommand AppointCommand { get; }
        public ICommand DismissCommand { get; }

        public AdvisorSettingViewModel(GameService service, Action onBack)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
            
            AppointCommand = new RelayCommandWithParam(id => {
                _gameService.AppointAdvisor((string)id);
                OnPropertyChanged(nameof(CurrentAdvisor));
                OnPropertyChanged(nameof(HasAdvisor));
                OnPropertyChanged(nameof(Vassals));
            });
            
            DismissCommand = new RelayCommand(() => {
                _gameService.DismissAdvisor();
                OnPropertyChanged(nameof(CurrentAdvisor));
                OnPropertyChanged(nameof(HasAdvisor));
                OnPropertyChanged(nameof(Vassals));
            });
        }
        
        public Vassal CurrentAdvisor
        {
            get
            {
                if (string.IsNullOrEmpty(_gameService.Player.AdvisorId)) return null;
                return _gameService.Vassals.FirstOrDefault(val => val.Id == _gameService.Player.AdvisorId);
            }
        }
        
        public bool HasAdvisor => !string.IsNullOrEmpty(_gameService.Player.AdvisorId);
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
