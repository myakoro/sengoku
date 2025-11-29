using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SengokuSLG.Models;

namespace SengokuSLG.Services
{
    public class GameService : INotifyPropertyChanged
    {
        private GameDate _currentDate;
        private bool _hasPublicDutyThisMonth;
        private int _publicDutyCountThisMonth;
        private int _monthlyExpense;
        private int _previousMonthMilitary;
        private int _previousMonthPolitical;
        private int _previousMonthSecret;
        private int _previousEvaluation;
        private int _villageADevelopmentStart;
        private int _villageBDevelopmentStart;
        private int _villageAPopulationStart;
        private int _villageBPopulationStart;

        public Player Player { get; private set; }
        public Village VillageA { get; private set; }
        public Village VillageB { get; private set; }
        public Lord Lord { get; private set; }
        public ObservableCollection<string> Logs { get; private set; }
        public ObservableCollection<DailyLog> DailyLogs { get; private set; }
        public bool HasSpecialtyPreparationFlag { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public GameDate CurrentDate
        {
            get => _currentDate;
            private set { _currentDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(CurrentDateDisplay)); }
        }

        public string CurrentDateDisplay => _currentDate?.ToString() ?? "";

        public bool HasPublicDutyThisMonth
        {
            get => _hasPublicDutyThisMonth;
            private set { _hasPublicDutyThisMonth = value; OnPropertyChanged(); }
        }

        public GameService()
        {
            Initialize();
        }

        private void Initialize()
        {
            Player = new Player 
            { 
                Money = 100,
                Evaluation = 0,
                AchievementMilitary = 0,
                AchievementPolitical = 0,
                AchievementSecret = 0,
                Favor = 0
            };
            VillageA = new Village("村A", 1000, 50, 100, 10);
            VillageB = new Village("村B", 500, 30, 50, 5);
            Lord = new Lord();
            CurrentDate = new GameDate(1587, 3, 12); 
            Logs = new ObservableCollection<string>();
            DailyLogs = new ObservableCollection<DailyLog>();
            HasPublicDutyThisMonth = false;
            HasSpecialtyPreparationFlag = false;
            _publicDutyCountThisMonth = 0;
            _monthlyExpense = 0;
            _previousMonthMilitary = 0;
            _previousMonthPolitical = 0;
            _previousMonthSecret = 0;
            _previousEvaluation = 0;
            _villageADevelopmentStart = VillageA.Development;
            _villageBDevelopmentStart = VillageB.Development;
            _villageAPopulationStart = VillageA.Population;
            _villageBPopulationStart = VillageB.Population;
        }

        // ===== PUBLIC DUTIES (6 types) =====
        // All public duties MUST grant achievements, MUST NOT grant money

        public void ExecuteSecurityMaintenance(Village village)
        {
            village.Security += 3;
            Player.AchievementMilitary += 1;
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            
            AddLog("公務", "治安維持", village.Name, "成功");
        }

        public void ExecutePatrol(Village village)
        {
            village.Security += 1;
            Player.AchievementMilitary += 1;
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            
            AddLog("公務", "巡察", village.Name, "成功");
        }

        public void ExecuteLandSurvey(Village village)
        {
            Player.AchievementPolitical += 2;
            Player.Favor += 1;
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            
            AddLog("公務", "検地補助", village.Name, "成功");
        }

        public void ExecuteConstruction(Village village)
        {
            village.Development += 1;
            Player.AchievementPolitical += 1;
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            
            AddLog("公務", "普請補助", village.Name, "成功");
        }

        public void ExecuteDocumentCreation()
        {
            Player.AchievementSecret += 1;
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            
            AddLog("公務", "書状作成", "", "成功");
        }

        public void ExecuteInformationGathering()
        {
            Player.AchievementSecret += 2;
            Player.Favor += 1;
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            
            AddLog("公務", "情報収集", "", "成功");
        }

        // ===== PRIVATE TASKS (4 types) =====
        // All private tasks MUST NOT grant achievements

        public void ExecuteVillageDevelopment(Village village)
        {
            village.Development += 2;
            village.Population += 5;
            Player.Money -= 10;
            _monthlyExpense += 10;
            
            AddLog("私事", "知行村の開発", village.Name, "成功");
        }

        public void ExecuteRoadMaintenance(Village village)
        {
            village.Development += 1;
            Player.Money -= 5;
            _monthlyExpense += 5;
            
            AddLog("私事", "道整備", village.Name, "成功");
        }

        public void ExecuteVillageMerchantTrade(Village village)
        {
            Player.Money += 10;
            
            AddLog("私事", "村商人との取引", village.Name, "成功");
        }

        public void ExecuteSpecialtyPreparation(Village village)
        {
            village.Development += 1;
            Player.Money -= 15;
            _monthlyExpense += 15;
            HasSpecialtyPreparationFlag = true; // 名産開発フラグON（将来拡張用）
            
            AddLog("私事", "名産開発の準備", village.Name, "成功");
        }

        private void AddLog(string actionType, string taskName, string target, string result)
        {
            var dailyLog = new DailyLog
            {
                Date = new GameDate(CurrentDate.Year, CurrentDate.Month, CurrentDate.Day),
                ActionType = actionType,
                TaskName = taskName,
                Target = target,
                Result = result
            };
            DailyLogs.Insert(0, dailyLog);
            
            string logText = $"{CurrentDate.Month}/{CurrentDate.Day}: {taskName}";
            if (!string.IsNullOrEmpty(target))
            {
                logText += $"({target})";
            }
            logText += " 成功";
            Logs.Insert(0, logText);
            
            // Keep only recent 5 logs in simple log
            while (Logs.Count > 5)
            {
                Logs.RemoveAt(Logs.Count - 1);
            }
        }

        public void AdvanceDay()
        {
            int previousMonth = CurrentDate.Month;
            int newDay = CurrentDate.Day + 1;
            int newMonth = CurrentDate.Month;
            int newYear = CurrentDate.Year;
            
            if (newDay > 30)
            {
                newDay = 1;
                newMonth++;
                if (newMonth > 12)
                {
                    newMonth = 1;
                    newYear++;
                }
            }
            
            // Create new GameDate to trigger PropertyChanged properly
            CurrentDate = new GameDate(newYear, newMonth, newDay);
            
            // Month boundary detection (30 days fixed per month)
            if (newMonth != previousMonth)
            {
                ProcessMonthly();
            }
        }

        private void ProcessMonthly()
        {
            // Calculate income
            int totalIncome = VillageA.Income + VillageB.Income;
            Player.Money += totalIncome;
            
            // Calculate achievement gains
            int militaryGain = Player.AchievementMilitary - _previousMonthMilitary;
            int politicalGain = Player.AchievementPolitical - _previousMonthPolitical;
            int secretGain = Player.AchievementSecret - _previousMonthSecret;
            
            // Calculate village changes
            int villageADevChange = VillageA.Development - _villageADevelopmentStart;
            int villageBDevChange = VillageB.Development - _villageBDevelopmentStart;
            int villageAPopChange = VillageA.Population - _villageAPopulationStart;
            int villageBPopChange = VillageB.Population - _villageBPopulationStart;
            
            // Update evaluation
            int evaluationChange = 0;
            if (HasPublicDutyThisMonth)
            {
                evaluationChange += 1;
            }
            if (militaryGain + politicalGain + secretGain >= 5)
            {
                evaluationChange += 1;
            }
            if (Player.Favor >= 3)
            {
                evaluationChange += 1;
            }
            Player.Evaluation += evaluationChange;
            
            // Create monthly summary
            var summary = new MonthlySummary
            {
                Year = CurrentDate.Year,
                Month = CurrentDate.Month - 1 == 0 ? 12 : CurrentDate.Month - 1,
                PublicDutyCount = _publicDutyCountThisMonth,
                PublicDutySuccessRate = 100, // v0.4では常に100%
                AchievementMilitaryGain = militaryGain,
                AchievementPoliticalGain = politicalGain,
                AchievementSecretGain = secretGain,
                EvaluationChange = evaluationChange,
                IncomeTotal = totalIncome,
                ExpenseTotal = _monthlyExpense,
                MoneyBalance = totalIncome - _monthlyExpense,
                VillageADevelopmentChange = villageADevChange,
                VillageBDevelopmentChange = villageBDevChange,
                VillageAPopulationChange = villageAPopChange,
                VillageBPopulationChange = villageBPopChange
            };
            
            // Reset monthly counters
            HasPublicDutyThisMonth = false;
            HasSpecialtyPreparationFlag = false; // 名産開発フラグもリセット
            _publicDutyCountThisMonth = 0;
            _monthlyExpense = 0;
            _previousMonthMilitary = Player.AchievementMilitary;
            _previousMonthPolitical = Player.AchievementPolitical;
            _previousMonthSecret = Player.AchievementSecret;
            _previousEvaluation = Player.Evaluation;
            _villageADevelopmentStart = VillageA.Development;
            _villageBDevelopmentStart = VillageB.Development;
            _villageAPopulationStart = VillageA.Population;
            _villageBPopulationStart = VillageB.Population;
            
            // Trigger monthly summary event
            OnMonthlyProcessed?.Invoke(this, summary);
        }

        public event EventHandler<MonthlySummary> OnMonthlyProcessed;
    }
}
