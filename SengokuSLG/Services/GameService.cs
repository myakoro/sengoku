using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private Random _random = new Random();

        public Player Player { get; private set; }
        public Village VillageA { get; private set; }
        public Village VillageB { get; private set; }
        public Lord Lord { get; private set; }
        public ObservableCollection<string> Logs { get; private set; }
        public ObservableCollection<DailyLog> DailyLogs { get; private set; }
        public ObservableCollection<Vassal> Vassals { get; private set; }
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
                Favor = 0,
                Rank = Rank.Toshi,
                AbilityCombat = 60,
                AbilityLeadership = 40,
                AbilityPolitics = 60,
                AbilityIntrigue = 60,
                Achievement = 0,
                JubokuSlots = 3
            };
            VillageA = new Village("村A", 1000, 50, 100, 10);
            VillageB = new Village("村B", 500, 30, 50, 5);
            Lord = new Lord();
            CurrentDate = new GameDate(1587, 3, 12); 
            Logs = new ObservableCollection<string>();
            DailyLogs = new ObservableCollection<DailyLog>();
            Vassals = new ObservableCollection<Vassal>();
            
            // Initialize Vassals (3 Juboku)
            for (int i = 0; i < 3; i++)
            {
                var v = GenerateVassal(Rank.Juboku);
                Vassals.Add(v);
                Player.JubokuIds.Add(v.Id);
            }

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

        private Vassal GenerateVassal(Rank rank)
        {
            var v = new Vassal
            {
                Name = $"家臣{_random.Next(100, 999)}",
                Age = _random.Next(18, 40),
                Rank = rank,
                AbilityCombat = _random.Next(30, 80),
                AbilityLeadership = _random.Next(10, 60),
                AbilityPolitics = _random.Next(30, 80),
                AbilityIntrigue = _random.Next(30, 80),
                Loyalty = rank == Rank.Juboku ? _random.Next(70, 90) : _random.Next(45, 65),
                Achievement = 0
            };
            return v;
        }

        // ===== ADVISOR LOGIC =====
        public void AppointAdvisor(string vassalId)
        {
            var vassal = Vassals.FirstOrDefault(v => v.Id == vassalId);
            if (vassal == null) return;

            // Dismiss current advisor if any
            if (!string.IsNullOrEmpty(Player.AdvisorId))
            {
                var current = Vassals.FirstOrDefault(v => v.Id == Player.AdvisorId);
                if (current != null)
                {
                    current.IsAdvisor = false;
                    current.Loyalty -= 3;
                }
            }

            // Appoint new
            Player.AdvisorId = vassalId;
            vassal.IsAdvisor = true;
            vassal.Loyalty += 5;
            
            // Full disclosure
            vassal.AbilityDisclosure.FullDisclosed = true;
            vassal.AbilityDisclosure.CombatDisclosed = true;
            vassal.AbilityDisclosure.LeadershipDisclosed = true;
            vassal.AbilityDisclosure.PoliticsDisclosed = true;
            vassal.AbilityDisclosure.IntrigueDisclosed = true;

            AddLog("人事", "補佐官任命", vassal.Name, "成功");
        }

        public void DismissAdvisor()
        {
            if (string.IsNullOrEmpty(Player.AdvisorId)) return;
            var current = Vassals.FirstOrDefault(v => v.Id == Player.AdvisorId);
            if (current != null)
            {
                current.IsAdvisor = false;
                current.Loyalty -= 3;
                AddLog("人事", "補佐官解任", current.Name, "成功");
            }
            Player.AdvisorId = null;
        }

        private (int politics, int intrigue) CalculateAdvisorBonus()
        {
            if (string.IsNullOrEmpty(Player.AdvisorId)) return (0, 0);
            var advisor = Vassals.FirstOrDefault(v => v.Id == Player.AdvisorId);
            if (advisor == null) return (0, 0);

            int polBonus = Math.Max(advisor.AbilityPolitics - Player.AbilityPolitics, 0) * 4 / 10; // 40%
            int intBonus = Math.Max(advisor.AbilityIntrigue - Player.AbilityIntrigue, 0) * 4 / 10; // 40%
            
            return (polBonus, intBonus);
        }

        // ===== PUBLIC DUTIES (6 types) =====
        // Success rate based on Politics + Advisor Bonus

        private bool AttemptPublicDuty(int difficulty)
        {
            var (polBonus, _) = CalculateAdvisorBonus();
            int effectivePol = Player.AbilityPolitics + polBonus;
            
            // Simple success check: (Ability / Difficulty) * 100 > Random(0-100)
            // For v0.55, let's make it mostly successful but influenced by stats
            double successChance = (double)effectivePol / difficulty;
            if (successChance > 0.9) successChance = 0.95;
            if (successChance < 0.1) successChance = 0.1;
            
            return _random.NextDouble() < successChance;
        }

        public void ExecuteSecurityMaintenance(Village village)
        {
            bool success = AttemptPublicDuty(50); // Difficulty 50
            if (success)
            {
                village.Security += 3;
                Player.AchievementMilitary += 1;
                AddLog("公務", "治安維持", village.Name, "成功");
            }
            else
            {
                AddLog("公務", "治安維持", village.Name, "失敗");
            }
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            IncrementDutyParticipation();
        }

        public void ExecutePatrol(Village village)
        {
            bool success = AttemptPublicDuty(40);
            if (success)
            {
                village.Security += 1;
                Player.AchievementMilitary += 1;
                AddLog("公務", "巡察", village.Name, "成功");
            }
            else
            {
                AddLog("公務", "巡察", village.Name, "失敗");
            }
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            IncrementDutyParticipation();
        }

        public void ExecuteLandSurvey(Village village)
        {
            bool success = AttemptPublicDuty(60);
            if (success)
            {
                Player.AchievementPolitical += 2;
                Player.Favor += 1;
                AddLog("公務", "検地補助", village.Name, "成功");
            }
            else
            {
                AddLog("公務", "検地補助", village.Name, "失敗");
            }
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            IncrementDutyParticipation();
        }

        public void ExecuteConstruction(Village village)
        {
            bool success = AttemptPublicDuty(55);
            if (success)
            {
                village.Development += 1;
                Player.AchievementPolitical += 1;
                AddLog("公務", "普請補助", village.Name, "成功");
            }
            else
            {
                AddLog("公務", "普請補助", village.Name, "失敗");
            }
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            IncrementDutyParticipation();
        }

        public void ExecuteDocumentCreation()
        {
            bool success = AttemptPublicDuty(45);
            if (success)
            {
                Player.AchievementSecret += 1;
                AddLog("公務", "書状作成", "", "成功");
            }
            else
            {
                AddLog("公務", "書状作成", "", "失敗");
            }
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            IncrementDutyParticipation();
        }

        public void ExecuteInformationGathering()
        {
            var (_, intBonus) = CalculateAdvisorBonus();
            int effectiveInt = Player.AbilityIntrigue + intBonus;
            double successChance = (double)effectiveInt / 60.0; // Difficulty 60
            
            if (_random.NextDouble() < successChance)
            {
                Player.AchievementSecret += 2;
                Player.Favor += 1;
                AddLog("公務", "情報収集", "", "成功");
            }
            else
            {
                AddLog("公務", "情報収集", "", "失敗");
            }
            HasPublicDutyThisMonth = true;
            _publicDutyCountThisMonth++;
            IncrementDutyParticipation();
        }

        private void IncrementDutyParticipation()
        {
            // Only Toshi and above participate in duties (simplified: all vassals except Juboku)
            foreach (var v in Vassals.Where(v => v.Rank >= Rank.Toshi))
            {
                v.DutyParticipationCount++;
                if (v.DutyParticipationCount >= 3) v.AbilityDisclosure.PoliticsDisclosed = true;
                if (v.DutyParticipationCount >= 6)
                {
                    v.AbilityDisclosure.IntrigueDisclosed = true;
                    v.AbilityDisclosure.LeadershipDisclosed = true;
                }
            }
        }

        // ===== PRIVATE TASKS (4 types) =====
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
            HasSpecialtyPreparationFlag = true;
            AddLog("私事", "名産開発の準備", village.Name, "成功");
        }

        // ===== COMBAT LOGIC =====
        public void ExecuteBattle()
        {
            // Guard Squad Power Calculation
            int guardPower = Player.AbilityCombat;
            if (Player.JubokuIds.Count > 0)
            {
                var lead = Vassals.FirstOrDefault(v => v.Id == Player.JubokuIds[0]);
                if (lead != null) guardPower += (int)(lead.AbilityCombat * 0.2);
            }
            if (Player.JubokuIds.Count > 1)
            {
                for (int i = 1; i < Player.JubokuIds.Count; i++)
                {
                    var v = Vassals.FirstOrDefault(val => val.Id == Player.JubokuIds[i]);
                    if (v != null) guardPower += (int)(v.AbilityCombat * 0.1);
                }
            }

            // Simple battle simulation
            int enemyPower = _random.Next(50, 100);
            bool win = guardPower > enemyPower;
            
            if (win)
            {
                Player.AchievementMilitary += 20; // Small victory
                Player.Achievement += 20;
                AddLog("戦闘", "小競り合い", "", "勝利");
                
                // Loyalty increase
                foreach (var v in Vassals) v.Loyalty += 3;
            }
            else
            {
                AddLog("戦闘", "小競り合い", "", "敗北");
                foreach (var v in Vassals) v.Loyalty -= 5;
            }

            // Participation & Disclosure
            foreach (var v in Vassals.Where(v => Player.JubokuIds.Contains(v.Id) || v.Rank >= Rank.Toshi))
            {
                v.CombatParticipationCount++;
                if (v.CombatParticipationCount >= 3) v.AbilityDisclosure.CombatDisclosed = true;
            }
        }

        // ===== PROMOTION LOGIC =====
        public bool CheckPlayerPromotion()
        {
            if (Player.Rank == Rank.Toshi && Player.Achievement >= 200)
            {
                return true;
            }
            if (Player.Rank == Rank.Kumigashira && Player.Achievement >= 500)
            {
                return true;
            }
            return false;
        }

        public void PromotePlayer()
        {
            if (Player.Rank == Rank.Toshi)
            {
                Player.Rank = Rank.Kumigashira;
                Player.JubokuSlots = 2;
                AddLog("出世", "組頭昇格", "", "成功");
            }
            else if (Player.Rank == Rank.Kumigashira)
            {
                Player.Rank = Rank.Busho;
                Player.JubokuSlots = 0;
                AddLog("出世", "部将昇格", "", "成功");
            }
            
            // Handle Juboku overflow
            while (Player.JubokuIds.Count > Player.JubokuSlots)
            {
                // Auto-promote or dismiss (Simplified: Dismiss last for now, UI should handle this but logic needs to be safe)
                // In v0.55 spec: "Show dialog". Since this is service logic, we'll just keep them in list but maybe flag them?
                // For simplicity/safety in auto-mode, we'll remove the last one from JubokuIds but keep in Vassals as "Unassigned" or just dismiss.
                // Spec says: "Promote to Toshi or Dismiss".
                // Let's promote the overflow to Toshi automatically for now to avoid data loss.
                var overflowId = Player.JubokuIds.Last();
                Player.JubokuIds.Remove(overflowId);
                var v = Vassals.FirstOrDefault(val => val.Id == overflowId);
                if (v != null)
                {
                    v.Rank = Rank.Toshi;
                    AddLog("人事", "自動昇格", v.Name, "徒士へ");
                }
            }
        }

        public void PromoteVassal(string vassalId, Rank targetRank)
        {
            var v = Vassals.FirstOrDefault(val => val.Id == vassalId);
            if (v == null) return;

            if (targetRank == Rank.Toshi && v.Rank == Rank.Juboku)
            {
                v.Rank = Rank.Toshi;
                v.Loyalty += 10;
                v.Achievement = 0;
                Player.JubokuIds.Remove(v.Id);
                AddLog("人事", "家臣昇格", v.Name, "徒士へ");
            }
            else if (targetRank == Rank.Kumigashira && v.Rank == Rank.Toshi)
            {
                if (v.Achievement >= 30 && Player.Rank >= Rank.Kumigashira)
                {
                    v.Rank = Rank.Kumigashira;
                    v.Loyalty += 12;
                    AddLog("人事", "家臣昇格", v.Name, "組頭へ");
                }
            }
        }

        // ===== LOGGING =====
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
            logText += $" {result}";
            Logs.Insert(0, logText);
            
            while (Logs.Count > 5) Logs.RemoveAt(Logs.Count - 1);
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
            
            CurrentDate = new GameDate(newYear, newMonth, newDay);
            
            if (newMonth != previousMonth)
            {
                ProcessMonthly();
            }
        }

        private void ProcessMonthly()
        {
            // 1. Income
            int totalIncome = VillageA.Income + VillageB.Income + Player.Salary;
            Player.Money += totalIncome;
            
            // 2. Juboku Salary
            int jubokuSalary = Player.JubokuIds.Count * 15;
            Player.Money -= jubokuSalary;
            _monthlyExpense += jubokuSalary;

            // 3. Achievement Calculation
            int militaryGain = Player.AchievementMilitary - _previousMonthMilitary;
            int politicalGain = Player.AchievementPolitical - _previousMonthPolitical;
            int secretGain = Player.AchievementSecret - _previousMonthSecret;
            
            // Add to total achievement for promotion
            int totalGain = militaryGain + politicalGain + secretGain;
            Player.Achievement += totalGain;

            // Vassal Achievement (Toshi+)
            foreach (var v in Vassals.Where(v => v.Rank >= Rank.Toshi))
            {
                // Simplified: Toshi gets 1 achievement per month if public duty was successful
                if (HasPublicDutyThisMonth) v.Achievement += 1;
            }

            // 4. Loyalty Update
            foreach (var v in Vassals)
            {
                if (HasPublicDutyThisMonth) v.Loyalty += 1;
                // Cap at 100
                if (v.Loyalty > 100) v.Loyalty = 100;
            }

            // 5. Advisor Growth
            if (!string.IsNullOrEmpty(Player.AdvisorId))
            {
                var advisor = Vassals.FirstOrDefault(v => v.Id == Player.AdvisorId);
                if (advisor != null)
                {
                    if (_random.NextDouble() < 0.3) advisor.AbilityPolitics++;
                    if (_random.NextDouble() < 0.3) advisor.AbilityIntrigue++;
                }
            }

            // 6. Player Promotion Check
            bool promoted = false;
            Rank? newRank = null;
            if (CheckPlayerPromotion())
            {
                PromotePlayer();
                promoted = true;
                newRank = Player.Rank;
            }

            // 7. Evaluation Update
            int evaluationChange = 0;
            if (HasPublicDutyThisMonth) evaluationChange += 1;
            if (totalGain >= 5) evaluationChange += 1;
            if (Player.Favor >= 3) evaluationChange += 1;
            Player.Evaluation += evaluationChange;
            
            // 8. Create Summary
            var summary = new MonthlySummary
            {
                Year = CurrentDate.Year,
                Month = CurrentDate.Month - 1 == 0 ? 12 : CurrentDate.Month - 1,
                PublicDutyCount = _publicDutyCountThisMonth,
                PublicDutySuccessRate = 100, // Simplified
                AchievementMilitaryGain = militaryGain,
                AchievementPoliticalGain = politicalGain,
                AchievementSecretGain = secretGain,
                EvaluationChange = evaluationChange,
                IncomeTotal = totalIncome,
                ExpenseTotal = _monthlyExpense,
                MoneyBalance = totalIncome - _monthlyExpense,
                VillageADevelopmentChange = VillageA.Development - _villageADevelopmentStart,
                VillageBDevelopmentChange = VillageB.Development - _villageBDevelopmentStart,
                VillageAPopulationChange = VillageA.Population - _villageAPopulationStart,
                VillageBPopulationChange = VillageB.Population - _villageBPopulationStart,
                PlayerPromoted = promoted,
                PlayerNewRank = newRank
            };
            
            // Reset counters
            HasPublicDutyThisMonth = false;
            HasSpecialtyPreparationFlag = false;
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
            
            OnMonthlyProcessed?.Invoke(this, summary);
        }

        public event EventHandler<MonthlySummary> OnMonthlyProcessed;
    }
}
