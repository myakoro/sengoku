using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SengokuSLG.Models
{
    // ==========================================
    // v0.7 Economic System Models
    // ==========================================

    public enum VillageType
    {
        Village,
        Town
    }

    public enum RoadLevel
    {
        Road,
        Highway
    }

    public enum IndustryType
    {
        Agriculture,  // 農業
        Smithing,     // 鍛冶
        Weaving,      // 織物
        Brewing,      // 醸造
        Mining        // 鉱山
    }

    public enum MeisanType
    {
        MeiTo,        // 名刀
        MeiShu,       // 名酒
        MeiOrimono,   // 名織物
        MeiYaku,      // 名薬
        MeiTou        // 名陶
    }

    public enum MerchantTier
    {
        Traveling,    // 行商
        Town,         // 街商人
        Regional,     // 地方商人（拡張）
        City          // 都市商人（拡張）
    }

    public enum CreditRank
    {
        Acquaintance, // 面識
        Regular,      // 常連
        VIP           // 上客
    }

    public enum ProductCategory
    {
        Food,         // 食料
        Material,     // 資材
        Weapon,       // 武具
        Luxury        // 贅沢品
    }

    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int BasePrice { get; set; }
        public ProductCategory Category { get; set; }

        public Product(string id, string name, int basePrice, ProductCategory category)
        {
            Id = id;
            Name = name;
            BasePrice = basePrice;
            Category = category;
        }
    }

    public class Meisan : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public MeisanType Type { get; set; }
        public string Name { get; set; }
        public IndustryType RequiredIndustry { get; set; }
        public int RequiredLevel { get; set; }
        public int CreditBonus { get; set; }
        public float PriceMultiplier { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Meisan(string id, MeisanType type, string name, IndustryType reqIndustry, int reqLevel, int creditBonus, float priceMultiplier)
        {
            Id = id;
            Type = type;
            Name = name;
            RequiredIndustry = reqIndustry;
            RequiredLevel = reqLevel;
            CreditBonus = creditBonus;
            PriceMultiplier = priceMultiplier;
        }
    }

    public class IndustrySlot : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        private IndustryType _type;
        public IndustryType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        private int _level;
        public int Level
        {
            get => _level;
            set { _level = value; OnPropertyChanged(); }
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        // 拡張用パラメータ
        public Dictionary<string, int> Parameters { get; set; } = new Dictionary<string, int>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IndustrySlot(IndustryType type, int level = 1)
        {
            Type = type;
            Level = level;
            Progress = 0;
        }
    }

    public class Merchant : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        
        private MerchantTier _tier;
        public MerchantTier Tier
        {
            get => _tier;
            set { _tier = value; OnPropertyChanged(); }
        }

        private int _credit;
        public int Credit
        {
            get => _credit;
            set 
            { 
                _credit = value; 
                UpdateCreditRank();
                OnPropertyChanged(); 
            }
        }

        private CreditRank _creditRank;
        public CreditRank CreditRank
        {
            get => _creditRank;
            set { _creditRank = value; OnPropertyChanged(); }
        }

        public int TotalTradeAmount { get; set; }
        public int TotalTradeCount { get; set; }

        public List<Product> ProductList { get; set; } = new List<Product>();
        public Dictionary<string, int> Inventory { get; set; } = new Dictionary<string, int>();

        // 行商用
        public int VisitFrequency { get; set; } // 日数
        public int NextVisitDay { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Merchant(string name, MerchantTier tier)
        {
            Name = name;
            Tier = tier;
            Credit = 0;
            UpdateCreditRank();
        }

        private void UpdateCreditRank()
        {
            if (Credit >= 61) CreditRank = CreditRank.VIP;
            else if (Credit >= 31) CreditRank = CreditRank.Regular;
            else CreditRank = CreditRank.Acquaintance;
        }
    }

    public class Village : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }

        private VillageType _type;
        public VillageType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        private int _population;
        public int Population
        {
            get => _population;
            set { _population = value; OnPropertyChanged(); }
        }

        private int _security;
        public int Security
        {
            get => _security;
            set { _security = value; OnPropertyChanged(); }
        }

        private int _taxIncome;
        public int TaxIncome
        {
            get => _taxIncome;
            set { _taxIncome = value; OnPropertyChanged(); }
        }

        private int _money;
        public int Money
        {
            get => _money;
            set { _money = value; OnPropertyChanged(); }
        }

        // 産業枠 (ObservableCollection推奨だが、ここではListで定義しViewModelでラップ想定)
        // 設計書に合わせて配列/リスト
        public List<IndustrySlot> IndustrySlots { get; set; } = new List<IndustrySlot>();

        private int _meisanProgress;
        public int MeisanProgress
        {
            get => _meisanProgress;
            set { _meisanProgress = value; OnPropertyChanged(); }
        }

        private Meisan _currentMeisan;
        public Meisan CurrentMeisan
        {
            get => _currentMeisan;
            set { _currentMeisan = value; OnPropertyChanged(); }
        }

        private RoadLevel _roadLevel;
        public RoadLevel RoadLevel
        {
            get => _roadLevel;
            set { _roadLevel = value; OnPropertyChanged(); }
        }

        // 商人リスト
        public List<Merchant> MerchantsVisiting { get; set; } = new List<Merchant>();
        public List<Merchant> MerchantsStationed { get; set; } = new List<Merchant>();

        public int TotalTradeCount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Village(string name)
        {
            Name = name;
            Type = VillageType.Village;
            Population = 50; // 初期値
            Security = 50;
            RoadLevel = RoadLevel.Road;
        }
    }
}
