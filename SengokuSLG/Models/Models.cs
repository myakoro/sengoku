using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SengokuSLG.Models
{
    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class Player : BaseModel
    {
        private int _money;
        private int _evaluation;
        private int _achievementMilitary;
        private int _achievementPolitical;
        private int _achievementSecret;
        private int _favor;
        
        public string Name { get; set; } = "田中太郎";
        public string Affiliation { get; set; } = "織田家（羽柴家）";
        public string Rank { get; set; } = "書役";
        public string Role { get; set; } = "書役";

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
        public string ActionType { get; set; } // "公務" or "私事"
        public string TaskName { get; set; }
        public string Target { get; set; } // "村A", "村B", or ""
        public string Result { get; set; }
    }

    public class MonthlySummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int PublicDutyCount { get; set; }
        public int PublicDutySuccessRate { get; set; } = 100; // v0.4では常に100%
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
    }
}
