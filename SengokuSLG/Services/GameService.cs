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
        private int _villageAIndustryLevelStart;
        private int _villageBIndustryLevelStart;
        private int _villageAPopulationStart;
        private int _villageBPopulationStart;
        private Random _random = new Random();

        private EconomicService _economicService;
        private MarriageService _marriageService;
        private FamilyService _familyService;
        private SuccessionService _successionService;

        public Player Player { get; private set; }
        public Village VillageA { get; private set; }
        public Village VillageB { get; private set; }
        public Lord Lord { get; private set; }
        public ObservableCollection<string> Logs { get; private set; }
        public ObservableCollection<DailyLog> DailyLogs { get; private set; }
        public ObservableCollection<Vassal> Vassals { get; private set; }

        public bool HasSpecialtyPreparationFlag { get; private set; }
        public House PlayerHouse { get; private set; }
        public SuccessionContext PendingSuccession { get; private set; }
        public ObservableCollection<MonthlyEvent> MonthlyEvents { get; private set; }
        public BattleService BattleService { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => 
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
                Rank = Rank.Bajoshu,
                AbilityCombat = 60,
                AbilityLeadership = 40,
                AbilityPolitics = 60,
                AbilityIntrigue = 60,
                Achievement = 0,
                JubokuSlots = 3
            };
            VillageA = new Village("村A") { Population = 1000, Security = 50, TaxIncome = 100 };
            VillageA.IndustrySlots.Add(new IndustrySlot(IndustryType.Agriculture, 1)); // 初期産業

            VillageB = new Village("村B") { Population = 500, Security = 30, TaxIncome = 50 };
            VillageB.IndustrySlots.Add(new IndustrySlot(IndustryType.Smithing, 1)); // 初期産業

            // Initialize merchants for each village
            InitializeMerchants(VillageA);
            InitializeMerchants(VillageB);

            Lord = new Lord();
            CurrentDate = new GameDate(1587, 3, 23); 
            Logs = new ObservableCollection<string>();
            DailyLogs = new ObservableCollection<DailyLog>();
            Vassals = new ObservableCollection<Vassal>();
            

            
            _economicService = new EconomicService();
            _marriageService = new MarriageService();
            _familyService = new FamilyService();
            _successionService = new SuccessionService();

            PlayerHouse = new House 
            { 
                Name = "羽柴家", 
                Kakaku = 50, 
                IsPlayerHouse = true 
            };

            MonthlyEvents = new ObservableCollection<MonthlyEvent>();

            // Initialize Vassals (3 Juboku)
            for (int i = 0; i < 3; i++)
            {
                var v = GenerateVassal(Rank.Juboku);
                Vassals.Add(v);
                Player.JubokuIds.Add(v.Id);
            }

            // --- Comprehensive Sample Data for Marriage & Family ---
            
            // Player setup
            Player.MaritalStatus = MaritalStatus.Married;
            Player.SpouseId = "SPOUSE_01";
            Player.FatherId = "FATHER_01";
            Player.MotherId = "MOTHER_01";
            
            // Spouse (Wife)
            Vassals.Add(new Vassal 
            { 
                Id = "SPOUSE_01", 
                Name = "お市", 
                Age = 25, 
                Gender = Gender.Female,
                Origin = "一門", 
                Rank = Rank.Juboku,
                Loyalty = 100,
                IsAdult = true,
                IsGenpuku = true,
                MaritalStatus = MaritalStatus.Married,
                SpouseId = Player.Id,
                BirthYear = 1562
            });

            // Father (Alive)
            Vassals.Add(new Vassal
            {
                Id = "FATHER_01",
                Name = "田中一郎",
                Age = 55,
                Gender = Gender.Male,
                Origin = "一門",
                Rank = Rank.Toshi,
                Loyalty = 100,
                IsAdult = true,
                IsGenpuku = true,
                MaritalStatus = MaritalStatus.Married,
                SpouseId = "MOTHER_01",
                BirthYear = 1532,
                AbilityCombat = 70,
                AbilityLeadership = 60,
                AbilityPolitics = 50,
                AbilityIntrigue = 40
            });

            // Mother (Alive)
            Vassals.Add(new Vassal
            {
                Id = "MOTHER_01",
                Name = "田中花",
                Age = 52,
                Gender = Gender.Female,
                Origin = "一門",
                Rank = Rank.Juboku,
                Loyalty = 100,
                IsAdult = true,
                IsGenpuku = true,
                MaritalStatus = MaritalStatus.Married,
                SpouseId = "FATHER_01",
                BirthYear = 1535
            });

            // Son (Genpuku済み)
            Vassals.Add(new Vassal
            {
                Id = "SON_01",
                Name = "田中太郎",
                Age = 16,
                Gender = Gender.Male,
                Rank = Rank.Juboku,
                IsAdult = true,
                IsGenpuku = true,
                FatherId = Player.Id,
                MotherId = "SPOUSE_01",
                BirthYear = 1571,
                Origin = "一門",
                Loyalty = 100,
                AbilityCombat = 55,
                AbilityLeadership = 45,
                AbilityPolitics = 30,
                AbilityIntrigue = 25
            });

            // Son (元服前)
            Vassals.Add(new Vassal
            {
                Id = "SON_02",
                Name = "田中次郎",
                Age = 12,
                Gender = Gender.Male,
                Rank = Rank.Juboku,
                IsAdult = false,
                IsGenpuku = false,
                FatherId = Player.Id,
                MotherId = "SPOUSE_01",
                BirthYear = 1575,
                Origin = "一門",
                Loyalty = 100,
                AbilityCombat = 40,
                AbilityLeadership = 35,
                AbilityPolitics = 45,
                AbilityIntrigue = 30
            });

            // Daughter
            Vassals.Add(new Vassal
            {
                Id = "DAUGHTER_01",
                Name = "田中花子",
                Age = 10,
                Gender = Gender.Female,
                Rank = Rank.Juboku,
                IsAdult = false,
                IsGenpuku = false,
                FatherId = Player.Id,
                MotherId = "SPOUSE_01",
                BirthYear = 1577,
                Origin = "一門",
                Loyalty = 100,
                AbilityCombat = 10,
                AbilityLeadership = 30,
                AbilityPolitics = 50,
                AbilityIntrigue = 45
            });

            // 譜代家臣
            Vassals.Add(new Vassal
            {
                Id = "VASSAL_01",
                Name = "佐藤忠信",
                Age = 40,
                Gender = Gender.Male,
                Origin = "譜代",
                Rank = Rank.Toshi,
                Loyalty = 95,
                IsAdult = true,
                IsGenpuku = true,
                MaritalStatus = MaritalStatus.Single,
                BirthYear = 1547,
                AbilityCombat = 75,
                AbilityLeadership = 65,
                AbilityPolitics = 40,
                AbilityIntrigue = 30
            });

            Vassals.Add(new Vassal
            {
                Id = "VASSAL_02",
                Name = "鈴木重秀",
                Age = 35,
                Gender = Gender.Male,
                Origin = "譜代",
                Rank = Rank.Toshi,
                Loyalty = 90,
                IsAdult = true,
                IsGenpuku = true,
                MaritalStatus = MaritalStatus.Single,
                BirthYear = 1552,
                AbilityCombat = 80,
                AbilityLeadership = 70,
                AbilityPolitics = 30,
                AbilityIntrigue = 50
            });

            // 新参家臣
            Vassals.Add(new Vassal
            {
                Id = "VASSAL_03",
                Name = "高橋紹運",
                Age = 28,
                Gender = Gender.Male,
                Origin = "新参",
                Rank = Rank.Juboku,
                Loyalty = 80,
                IsAdult = true,
                IsGenpuku = true,
                MaritalStatus = MaritalStatus.Single,
                BirthYear = 1559,
                AbilityCombat = 85,
                AbilityLeadership = 75,
                AbilityPolitics = 60,
                AbilityIntrigue = 55
            });

            // Brother (Alive, Genpuku済み)
            Vassals.Add(new Vassal
            {
                Id = "BROTHER_01",
                Name = "田中三郎",
                Age = 23,
                Gender = Gender.Male,
                Rank = Rank.Toshi,
                IsAdult = true,
                IsGenpuku = true,
                FatherId = "FATHER_01",
                MotherId = "MOTHER_01",
                BirthYear = 1564,
                Origin = "一門",
                Loyalty = 95,
                AbilityCombat = 65,
                AbilityLeadership = 55,
                AbilityPolitics = 40,
                AbilityIntrigue = 35
            });

            // Sister
            Vassals.Add(new Vassal
            {
                Id = "SISTER_01",
                Name = "田中春",
                Age = 20,
                Gender = Gender.Female,
                Rank = Rank.Juboku,
                IsAdult = true,
                IsGenpuku = true,
                FatherId = "FATHER_01",
                MotherId = "MOTHER_01",
                BirthYear = 1567,
                Origin = "一門",
                Loyalty = 100,
                MaritalStatus = MaritalStatus.Single
            });

            // Grandfather (Deceased)
            Vassals.Add(new Vassal
            {
                Id = "GRANDFATHER_01",
                Name = "田中源蔵",
                Age = 78,
                Gender = Gender.Male,
                Rank = Rank.Busho,
                IsAdult = true,
                IsGenpuku = true,
                IsDead = true,
                BirthYear = 1509,
                Origin = "一門",
                Loyalty = 100,
                AbilityCombat = 80,
                AbilityLeadership = 75,
                AbilityPolitics = 60,
                AbilityIntrigue = 50
            });

            // Grandmother (Deceased)
            Vassals.Add(new Vassal
            {
                Id = "GRANDMOTHER_01",
                Name = "田中梅",
                Age = 75,
                Gender = Gender.Female,
                Rank = Rank.Juboku,
                IsAdult = true,
                IsGenpuku = true,
                IsDead = true,
                BirthYear = 1512,
                Origin = "一門",
                Loyalty = 100
            });

            // Add marriage offers to PlayerHouse
            PlayerHouse.MarriageCandidates.Add(new MarriageOffer
            {
                SourceHouseName = "織田家分家",
                TargetPersonId = "SON_01",
                TargetPersonName = "田中太郎",
                CandidatePersonId = Guid.NewGuid().ToString(),
                CandidatePersonName = "お濃",
                IsLegalWife = true,
                Dowry = 300,
                RankDifference = 0,
                SuccessProb = 0.75f
            });

            PlayerHouse.MarriageCandidates.Add(new MarriageOffer
            {
                SourceHouseName = "武田家分家",
                TargetPersonId = "BROTHER_01",
                TargetPersonName = "田中三郎",
                CandidatePersonId = Guid.NewGuid().ToString(),
                CandidatePersonName = "お松",
                IsLegalWife = true,
                Dowry = 400,
                RankDifference = 1,
                SuccessProb = 0.85f,
                ConnectionOffer = new List<string> { "Influential Merchant" }
            });

            PlayerHouse.MarriageCandidates.Add(new MarriageOffer
            {
                SourceHouseName = "上杉家分家",
                TargetPersonId = Player.Id,
                TargetPersonName = Player.Name,
                CandidatePersonId = Guid.NewGuid().ToString(),
                CandidatePersonName = "お菊",
                IsLegalWife = false, // 側室
                Dowry = 200,
                RankDifference = -1,
                SuccessProb = 0.65f
            });
            // ---------------------------------------------------

            HasPublicDutyThisMonth = false;
            HasSpecialtyPreparationFlag = false;
            _publicDutyCountThisMonth = 0;
            _monthlyExpense = 0;
            _previousMonthMilitary = 0;
            _previousMonthPolitical = 0;
            _previousMonthSecret = 0;
            _previousEvaluation = 0;
            _villageAIndustryLevelStart = VillageA.IndustrySlots.Sum(s => s.Level);
            _villageBIndustryLevelStart = VillageB.IndustrySlots.Sum(s => s.Level);
            _villageAPopulationStart = VillageA.Population;
            _villageBPopulationStart = VillageB.Population;

            // Initialize BattleService with Sample Data
            BattleService = new BattleService(this);
            BattleService.InitializeSampleData();
        }

        private void InitializeMerchants(Village village)
        {
            // Add 2-3 initial merchants to each village
            var merchantCount = _random.Next(2, 4);
            for (int i = 0; i < merchantCount; i++)
            {
                var tierIndex = _random.Next(0, 3);
                var tier = (MerchantTier)tierIndex;
                var merchant = new Merchant($"商人{_random.Next(100, 999)}", tier);
                
                // Set additional properties
                merchant.Credit = _random.Next(20, 60);
                merchant.TotalTradeAmount = _random.Next(0, 500);
                merchant.TotalTradeCount = _random.Next(0, 10);
                merchant.VisitFrequency = _random.Next(5, 15);
                merchant.NextVisitDay = _random.Next(1, 30);

                // Add some sample products using constructor
                merchant.ProductList.Add(new Product("rice", "米", 10, ProductCategory.Food));
                merchant.ProductList.Add(new Product("sake", "酒", 20, ProductCategory.Luxury));
                merchant.ProductList.Add(new Product("iron", "鉄", 30, ProductCategory.Material));

                village.MerchantsVisiting.Add(merchant);
            }
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

            // Check if vassal has completed Genpuku
            if (!vassal.IsGenpuku)
            {
                AddLog("人事", "補佐官任命", vassal.Name, "失敗 (元服前)");
                return;
            }

            // Dismiss current advisor if any
            if (!string.IsNullOrEmpty(Player.AdvisorId))
            {
                var current = Vassals.FirstOrDefault(v => v.Id == Player.AdvisorId);
                if (current != null)
                {
                    current.IsAdvisor = false;
                    current.Loyalty -= 7;
                }
            }

            // Appoint new
            Player.AdvisorId = vassalId;
            vassal.IsAdvisor = true;
            vassal.Loyalty += 5;

            AddLog("人事", "補佐官任命", vassal.Name, "成功");
        }

        public void DismissAdvisor()
        {
            if (string.IsNullOrEmpty(Player.AdvisorId)) return;
            var current = Vassals.FirstOrDefault(v => v.Id == Player.AdvisorId);
            if (current != null)
            {
                current.IsAdvisor = false;
                current.Loyalty -= 7;
                AddLog("人事", "補佐官解任", current.Name, "成功");
            }
            Player.AdvisorId = null;
        }

        private (int leadership, int politics, int intrigue) CalculateAdvisorBonus()
        {
            if (string.IsNullOrEmpty(Player.AdvisorId)) return (0, 0, 0);
            var advisor = Vassals.FirstOrDefault(v => v.Id == Player.AdvisorId);
            if (advisor == null) return (0, 0, 0);

            int leadBonus = Math.Max(advisor.AbilityLeadership - Player.AbilityLeadership, 0) * 4 / 10; // 40%
            int polBonus = Math.Max(advisor.AbilityPolitics - Player.AbilityPolitics, 0) * 4 / 10; // 40%
            int intBonus = Math.Max(advisor.AbilityIntrigue - Player.AbilityIntrigue, 0) * 4 / 10; // 40%
            
            return (leadBonus, polBonus, intBonus);
        }

        // ===== PUBLIC DUTIES (6 types) =====
        // Success rate based on Politics + Advisor Bonus

        private bool AttemptPublicDuty(int difficulty)
        {
            var (_, polBonus, __) = CalculateAdvisorBonus();
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
                Player.Achievement += 1; // Add to cumulative achievement immediately
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
                Player.Achievement += 1; // Add to cumulative achievement immediately
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
                Player.Achievement += 2; // Add to cumulative achievement immediately
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
                village.Security += 2;
                if (village.Security > 100) village.Security = 100;
                village.Population += 2;
                Player.AchievementPolitical += 1;
                Player.Achievement += 1; // Add to cumulative achievement immediately
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
                Player.Achievement += 1; // Add to cumulative achievement immediately
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
            var (_, __, intBonus) = CalculateAdvisorBonus();
            int effectiveInt = Player.AbilityIntrigue + intBonus;
            double successChance = (double)effectiveInt / 60.0; // Difficulty 60
            
            if (_random.NextDouble() < successChance)
            {
                Player.AchievementSecret += 2;
                Player.Achievement += 2; // Add to cumulative achievement immediately
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
        // ===== PRIVATE TASKS (4 types) =====
        public void ExecuteVillageDevelopment(Village village)
        {
            // 産業開発: 最初のスロットを成長させる（簡易実装）
            var slot = village.IndustrySlots.FirstOrDefault();
            if (slot != null)
            {
                var advisor = string.IsNullOrEmpty(Player.AdvisorId) ? null : Vassals.FirstOrDefault(v => v.Id == Player.AdvisorId);
                _economicService.ProcessIndustryGrowth(slot, Player, advisor);
                
                village.Population += 5;
                Player.Money -= 10;
                _monthlyExpense += 10;
                AddLog("私事", "産業開発", $"{village.Name}の{GetIndustryName(slot.Type)}", "成功");
            }
            else
            {
                AddLog("私事", "産業開発", village.Name, "産業なし");
            }
        }

        private string GetIndustryName(IndustryType type)
        {
            return type switch
            {
                IndustryType.Agriculture => "農業",
                IndustryType.Smithing => "鍛冶",
                IndustryType.Weaving => "織物",
                IndustryType.Brewing => "醸造",
                IndustryType.Mining => "鉱山",
                _ => type.ToString()
            };
        }

        public void ExecuteRoadMaintenance(Village village)
        {
            village.Security += 2;
            if (village.Security > 100) village.Security = 100;
            
            Player.Money -= 5;
            _monthlyExpense += 5;
            AddLog("私事", "道整備", village.Name, "成功(治安+2)");
        }

        public void ExecuteVillageMerchantTrade(Village village)
        {
            // 簡易実装: 最初の行商と取引
            var merchant = village.MerchantsVisiting.FirstOrDefault();
            if (merchant == null)
            {
                // 行商がいない場合はダミー取引（v0.7初期状態用）
                merchant = new Merchant("行商", MerchantTier.Traveling);
                village.MerchantsVisiting.Add(merchant);
            }

            _economicService.ProcessMerchantTransaction(merchant, 100, village.CurrentMeisan != null);
            
            Player.Money += 10; // 取引利益
            AddLog("私事", "商人取引", $"{village.Name} ({merchant.Name})", "成功");
        }

        public void ExecuteSpecialtyPreparation(Village village)
        {
            village.MeisanProgress += 10;
            if (village.MeisanProgress > 100) village.MeisanProgress = 100;

            Player.Money -= 15;
            _monthlyExpense += 15;
            HasSpecialtyPreparationFlag = true;
            AddLog("私事", "名産開発", village.Name, "進捗+10");
        }

        // ===== COMBAT LOGIC (v0.9) =====
        private BattleService _battleService;

        public BattleContextV09 ExecuteBattle()
        {
            if (_battleService == null) _battleService = new BattleService(this);

            AddLog("戦闘", "戦闘開始", "", "v0.9 Logic");
            
            // 1. Initialize Battle
            var context = _battleService.InitializeBattle();

            // 2. Battle Loop
            while (!context.IsBattleEnded)
            {
                _battleService.ProcessTurn(context);
            }

            // 3. Calculate Merits
            _battleService.CalculateWarMerits(context);

            // 4. Apply Results to Game State
            ApplyBattleResultsV09(context);
            
            return context;
        }

        public BattleContextV09 StartBattleV09()
        {
            if (_battleService == null) _battleService = new BattleService(this);
            if (_battleService.CurrentContext != null) return _battleService.CurrentContext;
            return _battleService.InitializeBattle();
        }

        public void ProcessBattleTurnV09(BattleContextV09 context)
        {
            if (_battleService == null) _battleService = new BattleService(this);
            _battleService.ProcessTurn(context);
            
            if (context.IsBattleEnded)
            {
                _battleService.CalculateWarMerits(context);
                ApplyBattleResultsV09(context);
            }
        }

        private void ApplyBattleResultsV09(BattleContextV09 context)
        {
            // Log outcome
            AddLog("戦果", context.Outcome.ToString(), "", $"Turns: {context.TurnCount}");

            // Apply Merits to Player (Simplified)
            var playerMerit = context.Merits.FirstOrDefault(m => m.VassalId == "PLAYER");
            if (playerMerit != null)
            {
                Player.Achievement += playerMerit.TotalScore;
                Player.AchievementMilitary += playerMerit.TotalScore / 2; // Rough estimate
                AddLog("戦功", "論功行賞", "自分", $"功績+{playerMerit.TotalScore}");
            }

            // Participation
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

        public bool ProcessMarriage(MarriageOffer offer)
        {
            bool success = _marriageService.ProcessMarriage(PlayerHouse, Player, offer);
            if (success)
            {
                AddLog("家", "婚姻成立", offer.CandidatePersonName, "成功");
            }
            else
            {
                AddLog("家", "縁談交渉", offer.CandidatePersonName, "破談");
            }
            return success;
        }

        public float CalculateMarriageProbability(MarriageOffer offer)
        {
            // Target House Kakaku/Rank unknown in offer? 
            // Offer has RankDifference.
            // We can estimate target parameters from RankDifference.
            // Or just use a simplified calculation based on offer data.
            // The service method expects House and Rank.
            // Let's use a simplified version or update service.
            // Service: CalculateSuccessProbability(House playerHouse, Player player, int targetHouseKakaku, int targetRank)
            // We can derive targetKakaku from PlayerHouse.Kakaku - (offer.RankDifference * 10) approx?
            // Let's assume targetRank is Player.Rank - offer.RankDifference.
            
            int targetRankVal = (int)Player.Rank - offer.RankDifference;
            int targetKakaku = PlayerHouse.Kakaku - (offer.RankDifference * 20); // Approx
            
            return _marriageService.CalculateSuccessProbability(PlayerHouse, Player, targetKakaku, targetRankVal);
        }

        public void TriggerSuccession(Vassal heir)
        {
            PendingSuccession = _successionService.PrepareSuccession(PlayerHouse, Player, heir);
            // UI should bind to PendingSuccession and show dialog
            OnPropertyChanged(nameof(PendingSuccession));
        }

        public void ConfirmSuccession()
        {
            if (PendingSuccession == null) return;
            var heir = Vassals.FirstOrDefault(v => v.Id == PendingSuccession.HeirId);
            if (heir != null)
            {
                _successionService.ApplySuccession(PlayerHouse, Player, heir, PendingSuccession, Vassals.ToList());
                PendingSuccession = null;
                OnPropertyChanged(nameof(PendingSuccession));
                AddLog("家", "世代交代", "", $"{heir.Name}が跡を継ぎました。");
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
            
            // Daily Economic Processing
            _economicService.UpdateMerchantsSchedule(VillageA, CurrentDate.Day);
            _economicService.UpdateMerchantsSchedule(VillageB, CurrentDate.Day);

            if (newMonth != previousMonth)
            {
                ProcessMonthly();
            }
            if (newMonth != previousMonth)
            {
                ProcessMonthly();
                if (newYear != CurrentDate.Year)
                {
                    ProcessYearly();
                }
            }
        }

        private void ProcessYearly()
        {
            // Age update
            Player.Age++;
            foreach(var v in Vassals) v.Age++;

            // Death Check (Simple probability for > 60)
            if (Player.Age > 60 && _random.NextDouble() < 0.1)
            {
                // Find Heir (First adult son, or just first vassal for now)
                var heir = Vassals.FirstOrDefault(v => v.IsHeir) ?? Vassals.FirstOrDefault(v => v.IsAdult && v.Origin == "一門");
                if (heir != null)
                {
                    TriggerSuccession(heir);
                }
            }
        }

        private void ProcessMonthly()
        {
            // 1. Economic Monthly Processing
            _economicService.CheckIndustryLevelUp(VillageA);
            // 1. Economic Monthly Processing
            _economicService.CheckIndustryLevelUp(VillageA);
            _economicService.CheckIndustryLevelUp(VillageB);

            // v0.8 Logic
            _marriageService.GenerateMarriageOffers(PlayerHouse, Player, CurrentDate.Month, MonthlyEvents);
            _familyService.CheckBirth(Player, Vassals.ToList(), CurrentDate.Year, MonthlyEvents);
            var genpukuLogs = _familyService.CheckGenpuku(Vassals.ToList(), Player, MonthlyEvents);
            foreach (var log in genpukuLogs) AddLog("家族", "元服", "", log);

            var meisanA = _economicService.CheckMeisanGeneration(VillageA);
            if (meisanA != null) _economicService.ApproveMeisan(VillageA, meisanA); // Auto-approve for now

            var meisanB = _economicService.CheckMeisanGeneration(VillageB);
            if (meisanB != null) _economicService.ApproveMeisan(VillageB, meisanB);

            _economicService.CheckVillagePromotion(VillageA);
            _economicService.CheckVillagePromotion(VillageB);

            _economicService.CheckMerchantPromotion(VillageA);
            _economicService.CheckMerchantPromotion(VillageB);

            _economicService.ProcessMonthlyTax(VillageA);
            _economicService.ProcessMonthlyTax(VillageB);

            // Income (Tax goes to Player for now, or Village.Money is used for development)
            // Assuming TaxIncome is what Player gets.
            int totalIncome = VillageA.TaxIncome + VillageB.TaxIncome + Player.Salary;
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
                VillageAIndustryLevelChange = VillageA.IndustrySlots.Sum(s => s.Level) - _villageAIndustryLevelStart,
                VillageBIndustryLevelChange = VillageB.IndustrySlots.Sum(s => s.Level) - _villageBIndustryLevelStart,
                VillageAPopulationChange = VillageA.Population - _villageAPopulationStart,
                VillageBPopulationChange = VillageB.Population - _villageBPopulationStart,
                PlayerPromoted = promoted,
                PlayerNewRank = newRank
            };
            
            // Reset counters
            MonthlyEvents.Clear(); // Clear events from previous month
            HasPublicDutyThisMonth = false;
            HasSpecialtyPreparationFlag = false;
            _publicDutyCountThisMonth = 0;
            _monthlyExpense = 0;
            _previousMonthMilitary = Player.AchievementMilitary;
            _previousMonthPolitical = Player.AchievementPolitical;
            _previousMonthSecret = Player.AchievementSecret;
            _previousEvaluation = Player.Evaluation;
            _villageAIndustryLevelStart = VillageA.IndustrySlots.Sum(s => s.Level);
            _villageBIndustryLevelStart = VillageB.IndustrySlots.Sum(s => s.Level);
            _villageAPopulationStart = VillageA.Population;
            _villageBPopulationStart = VillageB.Population;
            
            OnMonthlyProcessed?.Invoke(this, summary);
        }

        public event EventHandler<MonthlySummary> OnMonthlyProcessed;
    }
}
