using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SengokuSLG.Models
{
    public enum Rank
    {
        Juboku,
        Toshi,
        Kumigashira,
        Busho,
        Jidaisho
    }

    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class AbilityDisclosureFlags
    {
        public bool CombatDisclosed { get; set; }
        public bool LeadershipDisclosed { get; set; }
        public bool PoliticsDisclosed { get; set; }
        public bool IntrigueDisclosed { get; set; }
        public bool FullDisclosed { get; set; }
    }

    public class Vassal : BaseModel
    {
        private Rank _rank;
        private int _loyalty;
        private int _achievement;
        private bool _isAdvisor;
        
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public int Age { get; set; }
        
        public Rank Rank
        {
            get => _rank;
            set { _rank = value; OnPropertyChanged(); }
        }

        public int AbilityCombat { get; set; }
        public int AbilityLeadership { get; set; }
        public int AbilityPolitics { get; set; }
        public int AbilityIntrigue { get; set; }

        public int Loyalty
        {
            get => _loyalty;
            set { _loyalty = value; OnPropertyChanged(); }
        }

        public int Achievement
        {
            get => _achievement;
            set { _achievement = value; OnPropertyChanged(); }
        }

        public bool IsAdvisor
        {
            get => _isAdvisor;
            set { _isAdvisor = value; OnPropertyChanged(); }
        }

        public AbilityDisclosureFlags AbilityDisclosure { get; set; } = new AbilityDisclosureFlags();
        public int CombatParticipationCount { get; set; }
        public int DutyParticipationCount { get; set; }
        
        // v0.55 UI additions
        public int YearOfService { get; set; } = 1;
        public string Origin { get; set; } = "譜代";
        
        public int Salary
        {
            get
            {
                switch (Rank)
                {
                    case Rank.Juboku: return 0;
                    case Rank.Toshi: return 50;
                    case Rank.Kumigashira: return 150;
                    case Rank.Busho: return 400;
                    default: return 50;
                }
            }
        }

        public Vassal() { }
    }

    public class Player : BaseModel
    {
        private int _money;
        private int _evaluation;
        private int _achievementMilitary;
        private int _achievementPolitical;
        private int _achievementSecret;
        private int _favor;
        private Rank _rank = Rank.Toshi;
        private int _abilityCombat;
        private int _abilityLeadership;
        private int _abilityPolitics;
        private int _abilityIntrigue;
        private string _advisorId;
        private int _jubokuSlots = 3;
        private int _achievement; // Cumulative achievement for promotion
        
        public string Name { get; set; } = "田中太郎";
        public string Affiliation { get; set; } = "織田家（羽柴家）";
        
        public Rank Rank
        {
            get => _rank;
            set { _rank = value; OnPropertyChanged(); }
        }

        public int AbilityCombat
        {
            get => _abilityCombat;
            set { _abilityCombat = value; OnPropertyChanged(); }
        }
        
        public int AbilityLeadership
        {
            get => _abilityLeadership;
            set { _abilityLeadership = value; OnPropertyChanged(); }
        }

        public int AbilityPolitics
        {
            get => _abilityPolitics;
            set { _abilityPolitics = value; OnPropertyChanged(); }
        }

        public int AbilityIntrigue
        {
            get => _abilityIntrigue;
            set { _abilityIntrigue = value; OnPropertyChanged(); }
        }

        public string AdvisorId
        {
            get => _advisorId;
            set { _advisorId = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> JubokuIds { get; set; } = new ObservableCollection<string>();

        public int JubokuSlots
        {
            get => _jubokuSlots;
            set { _jubokuSlots = value; OnPropertyChanged(); }
        }

        public int Money
        {
            get => _money;
            set { _money = value; OnPropertyChanged(); }
        }

        public int Evaluation
        {
            get => _evaluation;
            set { _evaluation = value; OnPropertyChanged(); }
        }

        public int AchievementMilitary
        {
            get => _achievementMilitary;
            set { _achievementMilitary = value; OnPropertyChanged(); }
        }

        public int AchievementPolitical
        {
            get => _achievementPolitical;
            set { _achievementPolitical = value; OnPropertyChanged(); }
        }

        public int AchievementSecret
        {
            get => _achievementSecret;
            set { _achievementSecret = value; OnPropertyChanged(); }
        }

        public int Favor
        {
            get => _favor;
            set { _favor = value; OnPropertyChanged(); }
        }

        public int Achievement
        {
            get => _achievement;
            set { _achievement = value; OnPropertyChanged(); }
        }
        
        // Helper property for Salary based on Rank (simplified for v0.55)
        public int Salary
        {
            get
            {
                switch (Rank)
                {
                    case Rank.Toshi: return 100;
                    case Rank.Kumigashira: return 300;
                    case Rank.Busho: return 700;
                    default: return 100;
                }
            }
        }
    }

    public class Village : BaseModel
    {
        private int _security;
        private int _development;
        private int _population;

        public string Name { get; set; }
        
        public int Population
        {
            get => _population;
            set { _population = value; OnPropertyChanged(); }
        }
        
        public int Income { get; set; }

        public int Security
        {
            get => _security;
            set { _security = value; OnPropertyChanged(); }
        }

        public int Development
        {
            get => _development;
            set { _development = value; OnPropertyChanged(); }
        }

        public Village(string name, int population, int security, int income, int development)
        {
            Name = name;
            Population = population;
            Security = security;
            Income = income;
            Development = development;
        }
    }

    public class Lord : BaseModel
    {
        public string Name { get; set; } = "羽柴秀吉";
    }

    public class GameDate
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        public GameDate(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public void AdvanceDay()
        {
            Day++;
            if (Day > 30)
            {
                Day = 1;
                Month++;
                if (Month > 12)
                {
                    Month = 1;
                    Year++;
                }
            }
        }

        public bool IsNewMonth => Day == 1;

        public override string ToString() => $"{Year}年{Month}月{Day}日";
    }

    public class DailyLog
    {
        public GameDate Date { get; set; }
        public string ActionType { get; set; } // "公務" or "私事" or "戦闘"
        public string TaskName { get; set; }
        public string Target { get; set; } // "村A", "村B", or ""
        public string Result { get; set; }
        public List<string> ParticipatingVassalIds { get; set; } = new List<string>();
    }

    public class MonthlySummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int PublicDutyCount { get; set; }
        public int PublicDutySuccessRate { get; set; } = 100;
        public int AchievementMilitaryGain { get; set; }
        public int AchievementPoliticalGain { get; set; }
        public int AchievementSecretGain { get; set; }
        public int EvaluationChange { get; set; }
        public int IncomeTotal { get; set; }
        public int ExpenseTotal { get; set; }
        public int MoneyBalance { get; set; } // 収入 - 支出
        public int VillageADevelopmentChange { get; set; }
        public int VillageBDevelopmentChange { get; set; }
        public int VillageAPopulationChange { get; set; }
        public int VillageBPopulationChange { get; set; }
        
        // v0.55 additions
        public bool PlayerPromoted { get; set; }
        public Rank? PlayerNewRank { get; set; }
        public List<string> VassalsPromoted { get; set; } = new List<string>();
        public Dictionary<string, int> LoyaltyChanges { get; set; } = new Dictionary<string, int>();
    }
}
