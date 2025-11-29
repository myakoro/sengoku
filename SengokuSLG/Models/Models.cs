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
    }

    public class Village : BaseModel
    {
        private int _security;
        private int _development;

        public string Name { get; set; }
        public int Population { get; set; }
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
}
