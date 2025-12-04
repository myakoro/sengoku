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
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class RelayCommandWithParam : ICommand
    {
        private readonly Action<object?> _execute;
        public RelayCommandWithParam(Action<object?> execute) { _execute = execute; }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged
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
            CurrentView = CreateMainScreenViewModel();
            
            NavigateDailyCommand = new RelayCommand(() => CurrentView = CreateMainScreenViewModel());
            NavigateVillageCommand = new RelayCommand(() => CurrentView = new VillageViewModel(_gameService, NavigateToIndustryDevelopment, NavigateToMerchantTrade));
            NavigateVassalListCommand = new RelayCommand(NavigateToVassalList);
            NavigateMarriageCommand = new RelayCommand(NavigateToMarriage);
            NavigateFamilyCommand = new RelayCommand(NavigateToFamily);
            NavigateMilitaryCommand = new RelayCommand(NavigateToMilitaryFormation);
        }

        private MainScreenViewModel CreateMainScreenViewModel()
        {
            return new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting, NavigateToBattleLog, NavigateToMarriage, NavigateToFamily);
        }

        private void NavigateToMarriage()
        {
            CurrentView = new MarriageViewModel(_gameService, 
                () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting, NavigateToBattleLog, NavigateToMarriage, NavigateToFamily),
                NavigateToMarriageNegotiation);
        }

        private void NavigateToMarriageNegotiation(MarriageOffer offer)
        {
            CurrentView = new MarriageNegotiationViewModel(_gameService, offer,
                NavigateToMarriage,
                (success) => {
                    var log = _gameService.DailyLogs[0];
                    DialogViewModel = new ActionResultViewModel(log, () => {
                        DialogViewModel = null;
                        _gameService.AdvanceDay();
                    });
                    CurrentView = CreateMainScreenViewModel();
                });
        }

        private void NavigateToFamily()
        {
            CurrentView = new FamilyViewModel(_gameService,
                () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting, NavigateToBattleLog, NavigateToMarriage, NavigateToFamily));
        }

        private void NavigateToMilitaryFormation()
        {
            CurrentView = new MilitaryFormationViewModel(_gameService,
                () => CurrentView = CreateMainScreenViewModel(),
                NavigateToBattalionManagement,
                NavigateToSquadStatusPanel,
                NavigateToBattlefield);
        }

        private void NavigateToBattalionManagement()
        {
            CurrentView = new BattalionManagementViewModel(_gameService,
                NavigateToMilitaryFormation,
                company => NavigateToCompanyManagement(company));
        }

        private void NavigateToCompanyManagement(object? companyObj)
        {
            var company = companyObj as BattleCompany;
            if (company == null) return;
            
            CurrentView = new CompanyManagementViewModel(_gameService,
                NavigateToBattalionManagement,
                NavigateToSquadDetail,
                company);
        }

        private void NavigateToSquadDetail()
        {
            CurrentView = new SquadDetailViewModel(_gameService,
                NavigateToBattalionManagement);
        }

        private void NavigateToSquadStatusPanel()
        {
            CurrentView = new SquadStatusPanelViewModel(_gameService,
                NavigateToMilitaryFormation,
                NavigateToSquadDetail);
        }

        private void NavigateToBattlefield()
        {
            CurrentView = new BattlefieldUIViewModel(_gameService,
                NavigateToMilitaryFormation,
                NavigateToPursuitDecision,
                NavigateToCasualtyReport);
        }

        private void NavigateToPursuitDecision()
        {
            CurrentView = new PursuitDecisionViewModel(_gameService,
                NavigateToBattlefield);
        }

        private void NavigateToCasualtyReport()
        {
            CurrentView = new CasualtyReportViewModel(_gameService,
                () => CurrentView = CreateMainScreenViewModel());
        }

        private void NavigateToMeritReport()
        {
            CurrentView = new MeritReportViewModel(_gameService,
                () => CurrentView = CreateMainScreenViewModel());
        }

        private void NavigateToBattleResultSummary()
        {
            CurrentView = new BattleResultSummaryViewModel(_gameService,
                () => CurrentView = CreateMainScreenViewModel());
        }

        private void NavigateToIndustryDevelopment(Village village)
        {
            CurrentView = new IndustryDevelopmentViewModel(village, 
                () => CurrentView = new VillageViewModel(_gameService, NavigateToIndustryDevelopment, NavigateToMerchantTrade));
        }

        private void NavigateToMerchantTrade(Merchant merchant)
        {
            CurrentView = new MerchantTradeViewModel(merchant, 
                () => CurrentView = new VillageViewModel(_gameService, NavigateToIndustryDevelopment, NavigateToMerchantTrade));
        }

        private void NavigateToPublicDuty()
        {
            CurrentView = new PublicDutySelectionViewModel(_gameService, 
                () => CurrentView = CreateMainScreenViewModel(),
                OnActionExecuted);
        }

        private void NavigateToPrivateTask()
        {
            CurrentView = new PrivateTaskSelectionViewModel(_gameService, 
                () => CurrentView = CreateMainScreenViewModel(),
                OnActionExecuted);
        }

        private void NavigateToVassalList()
        {
            CurrentView = new VassalListViewModel(_gameService, 
                () => CurrentView = CreateMainScreenViewModel(),
                NavigateToVassalDetail);
        }

        private void NavigateToVassalDetail(string vassalId)
        {
            CurrentView = new VassalDetailViewModel(_gameService, vassalId,
                () => CurrentView = new VassalListViewModel(_gameService, 
                    () => CurrentView = CreateMainScreenViewModel(),
                    NavigateToVassalDetail));
        }

        private void NavigateToAdvisorSetting()
        {
            CurrentView = new AdvisorSettingViewModel(_gameService,
                () => CurrentView = CreateMainScreenViewModel());
        }

        private void NavigateToBattleLog(BattleContextV09 context)
        {
            CurrentView = new BattleLogViewModel(context,
                () => CurrentView = CreateMainScreenViewModel());
        }

        private void OnActionExecuted(string taskName, string target)
        {
            var log = _gameService.DailyLogs[0];
            DialogViewModel = new ActionResultViewModel(log, () => {
                DialogViewModel = null;
                _gameService.AdvanceDay();
            });
            CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting, NavigateToBattleLog, NavigateToMarriage, NavigateToFamily);
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
            CurrentView = new MonthlySummaryViewModel(_gameService, summary, () => CurrentView = new MainScreenViewModel(_gameService, NavigateToPublicDuty, NavigateToPrivateTask, NavigateToVassalList, NavigateToAdvisorSetting, NavigateToBattleLog, NavigateToMarriage, NavigateToFamily));
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
        public ICommand NavigateMarriageCommand { get; }
        public ICommand NavigateFamilyCommand { get; }
        public ICommand NavigateMilitaryCommand { get; }
    }

    public class MainScreenViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand NavigatePublicDutyCommand { get; }
        public ICommand NavigatePrivateTaskCommand { get; }
        public ICommand NavigateVassalListCommand { get; }
        public ICommand NavigateAdvisorSettingCommand { get; }
        public ICommand ExecuteBattleCommand { get; }
        public ICommand NavigateMarriageCommand { get; }
        public ICommand NavigateFamilyCommand { get; }

        public MainScreenViewModel(GameService service, Action onPublicDuty, Action onPrivateTask, Action onVassalList, Action onAdvisorSetting, Action<BattleContextV09> onBattleLog, Action onMarriage, Action onFamily)
        {
            _gameService = service;
            _gameService.PropertyChanged += OnServicePropertyChanged;
            
            // Subscribe to Player property changes for advisor updates
            _gameService.Player.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_gameService.Player.AdvisorId))
                {
                    OnPropertyChanged(nameof(HasAdvisor));
                    OnPropertyChanged(nameof(LeadershipBonus));
                    OnPropertyChanged(nameof(PoliticsBonus));
                    OnPropertyChanged(nameof(IntrigueBonus));
                    OnPropertyChanged(nameof(EffectiveLeadership));
                    OnPropertyChanged(nameof(EffectivePolitics));
                    OnPropertyChanged(nameof(EffectiveIntrigue));
                }
            };
            
            NavigatePublicDutyCommand = new RelayCommand(onPublicDuty);
            NavigatePrivateTaskCommand = new RelayCommand(onPrivateTask);
            NavigateVassalListCommand = new RelayCommand(onVassalList);
            NavigateAdvisorSettingCommand = new RelayCommand(onAdvisorSetting);
            NavigateMarriageCommand = new RelayCommand(onMarriage);
            NavigateFamilyCommand = new RelayCommand(onFamily);
            
            // Debug/Simulate Battle Command
            ExecuteBattleCommand = new RelayCommand(() => {
                try
                {
                    var battleContext = _gameService.ExecuteBattle();
                    onBattleLog(battleContext);
                }
                catch (Exception ex)
                {
                    _gameService.Logs.Insert(0, $"★★★ エラー発生: {ex.Message}");
                }
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
        
        public bool HasAdvisor => !string.IsNullOrEmpty(_gameService.Player.AdvisorId);
        
        public int LeadershipBonus
        {
            get
            {
                if (string.IsNullOrEmpty(_gameService.Player.AdvisorId)) return 0;
                var advisor = _gameService.Vassals.FirstOrDefault(v => v.Id == _gameService.Player.AdvisorId);
                if (advisor == null) return 0;
                return Math.Max(advisor.AbilityLeadership - _gameService.Player.AbilityLeadership, 0) * 4 / 10;
            }
        }
        
        public int PoliticsBonus
        {
            get
            {
                if (string.IsNullOrEmpty(_gameService.Player.AdvisorId)) return 0;
                var advisor = _gameService.Vassals.FirstOrDefault(v => v.Id == _gameService.Player.AdvisorId);
                if (advisor == null) return 0;
                return Math.Max(advisor.AbilityPolitics - _gameService.Player.AbilityPolitics, 0) * 4 / 10;
            }
        }
        
        public int IntrigueBonus
        {
            get
            {
                if (string.IsNullOrEmpty(_gameService.Player.AdvisorId)) return 0;
                var advisor = _gameService.Vassals.FirstOrDefault(v => v.Id == _gameService.Player.AdvisorId);
                if (advisor == null) return 0;
                return Math.Max(advisor.AbilityIntrigue - _gameService.Player.AbilityIntrigue, 0) * 4 / 10;
            }
        }
        
        public int EffectiveLeadership => _gameService.Player.AbilityLeadership + LeadershipBonus;
        public int EffectivePolitics => _gameService.Player.AbilityPolitics + PoliticsBonus;
        public int EffectiveIntrigue => _gameService.Player.AbilityIntrigue + IntrigueBonus;
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
                onActionExecuted("村商人との取引", target ?? "");
            });

            ExecuteSpecialtyPreparationCommand = new RelayCommandWithParam(p => {
                var target = p as string;
                var village = target == "村A" ? _gameService.VillageA : _gameService.VillageB;
                _gameService.ExecuteSpecialtyPreparation(village);
                onActionExecuted("名産開発の準備", target ?? "");
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
            
            // Base filter: Only show Genpuku males who are alive
            _vassalView.Filter = v => {
                var vassal = v as Vassal;
                if (vassal == null) return false;
                return vassal.IsGenpuku && vassal.Gender == Gender.Male && !vassal.IsDead;
            };
            
            BackCommand = new RelayCommand(onBack);
            DetailCommand = new RelayCommandWithParam(id => onDetail((string)id));
            
            FilterCommand = new RelayCommandWithParam(param => {
                string filter = param as string;
                _vassalView.Filter = v => {
                    var vassal = v as Vassal;
                    if (vassal == null) return false;
                    
                    // Always apply base filter
                    if (!vassal.IsGenpuku || vassal.Gender != Gender.Male || vassal.IsDead) return false;
                    
                    if (string.IsNullOrEmpty(filter) || filter == "All") return true;
                    
                    if (filter == "Juboku") return vassal.Rank == Rank.Juboku;
                    if (filter == "Toshi") return vassal.Rank == Rank.Toshi;
                    if (filter == "Bajoshu") return vassal.Rank == Rank.Bajoshu;
                    if (filter == "Kogashira") return vassal.Rank == Rank.Kogashira;
                    if (filter == "Kumigashira") return vassal.Rank == Rank.Kumigashira;
                    if (filter == "AshigaruDaisho") return vassal.Rank == Rank.AshigaruDaisho;
                    if (filter == "Jidaisho") return vassal.Rank == Rank.Jidaisho;
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
        public ICommand ToggleAdvisorCommand { get; }

        public VassalDetailViewModel(GameService service, string vassalId, Action onBack)
        {
            _gameService = service;
            Vassal = _gameService.Vassals.FirstOrDefault(v => v.Id == vassalId);
            BackCommand = new RelayCommand(onBack);
            
            ToggleAdvisorCommand = new RelayCommand(() => {
                if (Vassal.IsAdvisor)
                {
                    _gameService.DismissAdvisor();
                }
                else
                {
                    _gameService.AppointAdvisor(vassalId);
                }
            });
        }
    }

    public class AdvisorSettingViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ObservableCollection<Vassal> Vassals { get; }
        public ICommand BackCommand { get; }
        public ICommand AppointCommand { get; }
        public ICommand DismissCommand { get; }

        public AdvisorSettingViewModel(GameService service, Action onBack)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
            
            // Filter to show only Genpuku males who are alive
            Vassals = new ObservableCollection<Vassal>(
                _gameService.Vassals.Where(v => v.IsGenpuku && v.Gender == Gender.Male && !v.IsDead));
            
            // Subscribe to Player property changes
            _gameService.Player.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_gameService.Player.AdvisorId))
                {
                    OnPropertyChanged(nameof(CurrentAdvisor));
                    OnPropertyChanged(nameof(HasAdvisor));
                }
            };
            
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
        public ICommand OpenIndustryDevelopmentCommand { get; }
        public ICommand OpenMerchantTradeCommand { get; }

        public VillageViewModel(GameService service, Action<Village> onOpenIndustry, Action<Merchant> onOpenMerchant) 
        { 
            Service = service;
            OpenIndustryDevelopmentCommand = new RelayCommandWithParam(p => onOpenIndustry(p as Village));
            OpenMerchantTradeCommand = new RelayCommandWithParam(p => onOpenMerchant(p as Merchant));
        }
    }

    public class IndustryDevelopmentViewModel : ViewModelBase
    {
        public Village CurrentVillage { get; }
        public ICommand CloseCommand { get; }

        public IndustryDevelopmentViewModel(Village village, Action onClose)
        {
            CurrentVillage = village;
            CloseCommand = new RelayCommand(onClose);
        }
    }

    public class MerchantTradeViewModel : ViewModelBase
    {
        public Merchant CurrentMerchant { get; }
        public ObservableCollection<TradeTransaction> TradeHistory { get; }
        public ICommand CloseCommand { get; }

        public MerchantTradeViewModel(Merchant merchant, Action onClose)
        {
            CurrentMerchant = merchant;
            CloseCommand = new RelayCommand(onClose);
            TradeHistory = new ObservableCollection<TradeTransaction>
            {
                new TradeTransaction { Date = "1560/4/10", Amount = 500 },
                new TradeTransaction { Date = "1560/3/25", Amount = 300 },
                new TradeTransaction { Date = "1560/3/10", Amount = 200 }
            };
        }
    }

    public class TradeTransaction
    {
        public string Date { get; set; }
        public int Amount { get; set; }
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

    public class BattleLogViewModel : ViewModelBase
    {
        public BattleContextV09 Context { get; }
        public ObservableCollection<string> Logs { get; }
        public ICommand CloseCommand { get; }

        public BattleLogViewModel(BattleContextV09 context, Action onClose)
        {
            Context = context;
            Logs = new ObservableCollection<string>(context.Logs);
            CloseCommand = new RelayCommand(onClose);
        }
    }

    public class MarriageViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ObservableCollection<MarriageOffer> Offers => _gameService.PlayerHouse.MarriageCandidates;
        public ICommand BackCommand { get; }
        public ICommand NegotiateCommand { get; }

        public MarriageViewModel(GameService service, Action onBack, Action<MarriageOffer> onNegotiate)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
            NegotiateCommand = new RelayCommandWithParam(p => onNegotiate(p as MarriageOffer));
        }
    }

    public class MarriageNegotiationViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public MarriageOffer Offer { get; }
        public float SuccessProbability { get; }
        public ICommand BackCommand { get; }
        public ICommand ExecuteCommand { get; }

        public MarriageNegotiationViewModel(GameService service, MarriageOffer offer, Action onBack, Action<bool> onExecute)
        {
            _gameService = service;
            Offer = offer;
            SuccessProbability = _gameService.CalculateMarriageProbability(offer);
            BackCommand = new RelayCommand(onBack);
            ExecuteCommand = new RelayCommand(() => {
                bool success = _gameService.ProcessMarriage(offer);
                onExecute(success);
            });
        }
    }

    public class FamilyViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ObservableCollection<Vassal> Children { get; }
        public ICommand BackCommand { get; }

        public FamilyViewModel(GameService service, Action onBack)
        {
            _gameService = service;
            // Filter children (IsAdult=false or Origin="一門")
            var children = _gameService.Vassals.Where(v => v.FatherId == _gameService.Player.Id || v.Origin == "一門");
            Children = new ObservableCollection<Vassal>(children);
            
            BackCommand = new RelayCommand(onBack);
        }
    }

    public class SuccessionViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public SuccessionContext Context => _gameService.PendingSuccession;
        public Vassal Heir { get; }
        public ICommand ConfirmCommand { get; }

        public SuccessionViewModel(GameService service, Action onConfirm)
        {
            _gameService = service;
            Heir = _gameService.Vassals.FirstOrDefault(v => v.Id == Context.HeirId);
            ConfirmCommand = new RelayCommand(() => {
                _gameService.ConfirmSuccession();
                onConfirm();
            });
        }
    }

    // ===== v0.9 Military System ViewModels =====

    public class MilitaryFormationViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand BackCommand { get; }
        public ICommand NavigateToBattalionCommand { get; }
        public ICommand NavigateToSquadStatusCommand { get; }
        public ICommand NavigateToBattlefieldCommand { get; }

        public MilitaryFormationViewModel(GameService service, Action onBack, Action onNavigateToBattalion, Action onNavigateToSquadStatus, Action onNavigateToBattlefield)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
            NavigateToBattalionCommand = new RelayCommand(onNavigateToBattalion);
            NavigateToSquadStatusCommand = new RelayCommand(onNavigateToSquadStatus);
            NavigateToBattlefieldCommand = new RelayCommand(onNavigateToBattlefield);
        }

        public Player Player => _gameService.Player;
        public ObservableCollection<Vassal> Vassals => _gameService.Vassals;
        public BattleBattalion Battalion => _gameService.BattleService.CurrentBattalion;
    }

    public class BattalionManagementViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand BackCommand { get; }
        public ICommand NavigateToCompanyCommand { get; }

        public BattalionManagementViewModel(GameService service, Action onBack, Action<object?> onNavigateToCompany)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
            NavigateToCompanyCommand = new RelayCommandWithParam(onNavigateToCompany);
        }

        public Player Player => _gameService.Player;
        public BattleBattalion Battalion => _gameService.BattleService.CurrentBattalion;
    }

    public class CompanyManagementViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand BackCommand { get; }
        public ICommand NavigateToSquadCommand { get; }

        public CompanyManagementViewModel(GameService service, Action onBack, Action onNavigateToSquad, BattleCompany selectedCompany)
        {
            _gameService = service;
            SelectedCompany = selectedCompany;
            BackCommand = new RelayCommand(onBack);
            NavigateToSquadCommand = new RelayCommand(onNavigateToSquad);
        }

        public Player Player => _gameService.Player;
        public BattleCompany SelectedCompany { get; }
    }

    public class SquadDetailViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand BackCommand { get; }

        public SquadDetailViewModel(GameService service, Action onBack)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
        }

        public Player Player => _gameService.Player;
        // For v0.9 sample, just show the first squad of the first company
        public BattleSquad SelectedSquad => _gameService.BattleService.CurrentBattalion?.Companies.FirstOrDefault()?.Squads.FirstOrDefault();
    }

    public class SquadStatusPanelViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand BackCommand { get; }
        public ICommand NavigateToSquadCommand { get; }

        public SquadStatusPanelViewModel(GameService service, Action onBack, Action onNavigateToSquad)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
            NavigateToSquadCommand = new RelayCommand(onNavigateToSquad);
        }

        public Player Player => _gameService.Player;
        public BattleBattalion Battalion => _gameService.BattleService.CurrentBattalion;
    }

    public class BattlefieldUIViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand BackCommand { get; }
        public ICommand NavigateToPursuitCommand { get; }
        public ICommand NavigateToCasualtyCommand { get; }

        public BattlefieldUIViewModel(GameService service, Action onBack, Action onNavigateToPursuit, Action onNavigateToCasualty)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
            NavigateToPursuitCommand = new RelayCommand(onNavigateToPursuit);
            NavigateToCasualtyCommand = new RelayCommand(onNavigateToCasualty);
        }

        public Player Player => _gameService.Player;
        public BattleContextV09 Context 
        {
            get
            {
                if (_gameService.BattleService.CurrentContext == null)
                {
                    _gameService.BattleService.InitializeBattle();
                }
                return _gameService.BattleService.CurrentContext;
            }
        }
    }

    public class CasualtySummaryItem
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public int Dead { get; set; }
        public int Disabled { get; set; }
        public int Wounded { get; set; }
        public int Deserted { get; set; }
        public int Total => Dead + Disabled + Wounded + Deserted;
        public int Remaining { get; set; }
    }

    public class CasualtyReportViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand BackCommand { get; }

        public CasualtyReportViewModel(GameService service, Action onBack)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
        }

        public BattleContextV09 Context => _gameService.BattleService.CurrentContext;

        public int PlayerDeaths => Context?.Casualties.Count(c => c.IsOwn && c.CasualtyType == CasualtyType.Death) ?? 0;
        public int PlayerDisabled => Context?.Casualties.Count(c => c.IsOwn && c.CasualtyType == CasualtyType.Disabled) ?? 0;
        public int PlayerInjured => Context?.Casualties.Count(c => c.IsOwn && c.CasualtyType == CasualtyType.Injury) ?? 0;
        public int PlayerFleeing => Context?.Casualties.Count(c => c.IsOwn && c.CasualtyType == CasualtyType.Fleeing) ?? 0;
        public int PlayerTotal => PlayerDeaths + PlayerDisabled + PlayerInjured + PlayerFleeing;

        public int EnemyDeaths => Context?.Casualties.Count(c => !c.IsOwn && c.CasualtyType == CasualtyType.Death) ?? 0;
        public int EnemyDisabled => Context?.Casualties.Count(c => !c.IsOwn && c.CasualtyType == CasualtyType.Disabled) ?? 0;
        public int EnemyInjured => Context?.Casualties.Count(c => !c.IsOwn && c.CasualtyType == CasualtyType.Injury) ?? 0;
        public int EnemyFleeing => Context?.Casualties.Count(c => !c.IsOwn && c.CasualtyType == CasualtyType.Fleeing) ?? 0;
        public int EnemyTotal => EnemyDeaths + EnemyDisabled + EnemyInjured + EnemyFleeing;

        public List<CasualtySummaryItem> Casualties
        {
            get
            {
                if (Context == null) return new List<CasualtySummaryItem>();
                
                var summary = new List<CasualtySummaryItem>();
                
                if (Context.PlayerBattalion != null)
                {
                    foreach(var company in Context.PlayerBattalion.Companies)
                    {
                        foreach(var squad in company.Squads)
                        {
                            var squadCasualties = Context.Casualties.Where(c => c.SquadId == squad.Id).ToList();
                            // Only add if there are casualties or it's a player squad
                            if (squadCasualties.Any() || true) 
                            {
                                summary.Add(new CasualtySummaryItem
                                {
                                    Name = squad.CommanderName,
                                    Position = "小頭",
                                    Dead = squadCasualties.Count(c => c.CasualtyType == CasualtyType.Death),
                                    Disabled = squadCasualties.Count(c => c.CasualtyType == CasualtyType.Disabled),
                                    Wounded = squadCasualties.Count(c => c.CasualtyType == CasualtyType.Injury),
                                    Deserted = squadCasualties.Count(c => c.CasualtyType == CasualtyType.Fleeing),
                                    Remaining = squad.CurrentSoldierCount
                                });
                            }
                        }
                    }
                }
                return summary;
            }
        }

        public Player Player => _gameService.Player;
    }

    public class PursuitDecisionViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand BackCommand { get; }
        public ICommand ExecutePursuitCommand { get; }

        public PursuitDecisionViewModel(GameService service, Action onBack)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
            ExecutePursuitCommand = new RelayCommand(() => {
                // TODO: Execute pursuit logic
                onBack();
            });
        }

        public Player Player => _gameService.Player;
        public BattleContextV09 Context => _gameService.BattleService.CurrentContext;
    }

    public class MeritReportViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand BackCommand { get; }

        public MeritReportViewModel(GameService service, Action onBack)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
        }

        public BattleContextV09 Context => _gameService.BattleService.CurrentContext;
        public Player Player => _gameService.Player;

        public List<MeritItem> Merits
        {
            get
            {
                if (Context == null || Context.Merits == null) return new List<MeritItem>();
                
                return Context.Merits.Select(m => new MeritItem
                {
                    Name = m.VassalName,
                    Position = m.Rank.ToString(),
                    MoraleDamage = m.MoraleDamageScore,
                    Collapse = m.CollapseBonusScore,
                    Pursuit = m.PursuitDamageScore,
                    Efficiency = m.EfficiencyBonusScore,
                    Modifier = m.BattleModifier.ToString("F1"),
                    Total = m.TotalScore
                }).ToList();
            }
        }
    }

    public class MeritItem
    {
        public string Name { get; set; } = "";
        public string Position { get; set; } = "";
        public int MoraleDamage { get; set; }
        public int Collapse { get; set; }
        public int Pursuit { get; set; }
        public int Efficiency { get; set; }
        public string Modifier { get; set; } = "";
        public int Total { get; set; }
    }

    public class BattleResultSummaryViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        public ICommand BackCommand { get; }

        public BattleResultSummaryViewModel(GameService service, Action onBack)
        {
            _gameService = service;
            BackCommand = new RelayCommand(onBack);
        }

        public BattleContextV09 Context => _gameService.BattleService.CurrentContext;
        public Player Player => _gameService.Player;
    }
}
