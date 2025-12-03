using System;
using System.Collections.Generic;
using System.Linq;
using SengokuSLG.Models;

namespace SengokuSLG.Services
{
    public class EconomicService
    {
        private Random _random = new Random();

        // ==========================================
        // 6-1. 村 → 町 昇格ロジック
        // ==========================================
        public void CheckVillagePromotion(Village village)
        {
            if (village.Type == VillageType.Town) return;

            // 昇格条件
            // 人口 >= 100
            // AND 産業合計レベル >= 2
            // AND 累計取引回数 >= 10
            // AND 治安 >= 50

            int totalIndustryLevel = village.IndustrySlots.Sum(s => s.Level);

            if (village.Population >= 100 &&
                totalIndustryLevel >= 2 &&
                village.TotalTradeCount >= 10 &&
                village.Security >= 50)
            {
                PromoteToTown(village);
            }
        }

        private void PromoteToTown(Village village)
        {
            village.Type = VillageType.Town;
            village.MerchantsStationed = new List<Merchant>();
            
            // ログ出力などはGameService側でハンドリングするか、ここでイベント発火
            // ここではデータ操作のみ
            Console.WriteLine($"[EconomicService] {village.Name} promoted to Town!");

            // 常駐商人誘致判定（初期）
            CheckMerchantPromotion(village);
        }

        // ==========================================
        // 6-2. 産業成長ロジック
        // ==========================================
        
        // 日次処理（私事「産業開発」実行時）
        public void ProcessIndustryGrowth(IndustrySlot slot, Player player, Vassal advisor)
        {
            int baseGrowth = 10;
            double playerAbility = player.AbilityPolitics / 100.0;
            
            double advisorBonus = 1.0;
            if (advisor != null)
            {
                advisorBonus = 1.0 + (advisor.AbilityPolitics - player.AbilityPolitics) * 0.4 / 100.0;
            }

            // Progress += BaseGrowth * PlayerAbility * AdvisorBonus
            double growth = baseGrowth * playerAbility * advisorBonus;
            slot.Progress += (int)Math.Max(1, growth); // 最低1は保証
        }

        // 月次処理（レベルアップ判定）
        public void CheckIndustryLevelUp(Village village)
        {
            foreach (var slot in village.IndustrySlots)
            {
                int threshold = GetLevelUpThreshold(slot.Level);
                if (slot.Progress >= threshold && slot.Level < 10)
                {
                    slot.Level++;
                    slot.Progress -= threshold;
                    Console.WriteLine($"[EconomicService] Industry {slot.Type} leveled up to {slot.Level}");
                }
            }
        }

        private int GetLevelUpThreshold(int currentLevel)
        {
            if (currentLevel == 1) return 100;
            if (currentLevel == 2) return 150;
            if (currentLevel == 3) return 200;
            if (currentLevel == 4) return 250;
            return 300 + (currentLevel - 5) * 50;
        }

        // ==========================================
        // 6-3. 名産生成ロジック
        // ==========================================

        // 月次判定
        public Meisan CheckMeisanGeneration(Village village)
        {
            if (village.CurrentMeisan != null) return null; // 既に名産がある

            foreach (var slot in village.IndustrySlots)
            {
                int requiredLevel = GetMeisanRequiredLevel(slot.Type);
                if (slot.Level >= requiredLevel)
                {
                    // 確率20%
                    if (_random.Next(0, 100) < 20)
                    {
                        // 名産候補発生（ここでは即時生成として実装、本来はイベント承認フロー）
                        return CreateMeisan(slot.Type, village.Name);
                    }
                }
            }
            return null;
        }

        private int GetMeisanRequiredLevel(IndustryType type)
        {
            switch (type)
            {
                case IndustryType.Agriculture: return 3;
                case IndustryType.Smithing: return 4;
                case IndustryType.Weaving: return 3;
                case IndustryType.Brewing: return 3;
                case IndustryType.Mining: return 5;
                default: return 99;
            }
        }

        public void ApproveMeisan(Village village, Meisan meisan)
        {
            village.CurrentMeisan = meisan;
            village.MeisanProgress = 100;
            Console.WriteLine($"[EconomicService] Meisan {meisan.Name} created!");
        }

        private Meisan CreateMeisan(IndustryType type, string villageName)
        {
            // 簡易生成ロジック
            string name = $"{villageName}の";
            MeisanType meisanType;
            int creditBonus = 10;
            float priceMultiplier = 1.5f;

            switch (type)
            {
                case IndustryType.Agriculture:
                    name += "名酒"; // 農業+醸造だが簡易化
                    meisanType = MeisanType.MeiShu;
                    creditBonus = 15;
                    break;
                case IndustryType.Smithing:
                    name += "名刀";
                    meisanType = MeisanType.MeiTo;
                    creditBonus = 30;
                    priceMultiplier = 3.0f;
                    break;
                case IndustryType.Weaving:
                    name += "名織物";
                    meisanType = MeisanType.MeiOrimono;
                    creditBonus = 20;
                    priceMultiplier = 2.0f;
                    break;
                case IndustryType.Brewing:
                    name += "美酒";
                    meisanType = MeisanType.MeiShu;
                    creditBonus = 15;
                    break;
                case IndustryType.Mining:
                    name += "秘薬";
                    meisanType = MeisanType.MeiYaku;
                    creditBonus = 25;
                    priceMultiplier = 2.5f;
                    break;
                default:
                    name += "名産";
                    meisanType = MeisanType.MeiTou;
                    break;
            }

            return new Meisan(Guid.NewGuid().ToString(), meisanType, name, type, GetMeisanRequiredLevel(type), creditBonus, priceMultiplier);
        }

        // ==========================================
        // 6-4. 商人信用ロジック
        // ==========================================

        public void ProcessMerchantTransaction(Merchant merchant, int tradeAmount, bool hasMeisan)
        {
            // CreditGain = (TradeAmount / 100) + (TradeCount * 2) + MeisanBonus
            int meisanBonus = hasMeisan ? 20 : 0; // 平均値
            int creditGain = (tradeAmount / 100) + 2 + meisanBonus;

            merchant.Credit += creditGain;
            if (merchant.Credit > 100) merchant.Credit = 100;

            merchant.TotalTradeAmount += tradeAmount;
            merchant.TotalTradeCount++;
        }

        // ==========================================
        // 6-5. 行商 → 常駐商人 誘致ロジック
        // ==========================================

        public void CheckMerchantPromotion(Village village)
        {
            if (village.Type != VillageType.Town) return;

            // 誘致条件: CreditRank >= Regular (31以上)
            // 実際はプレイヤー選択だが、ここでは自動または候補抽出ロジック
            
            var candidates = village.MerchantsVisiting
                .Where(m => m.CreditRank >= CreditRank.Regular)
                .ToList();

            foreach (var merchant in candidates)
            {
                // 簡易実装: 条件を満たせば即座に常駐化（枠制限などはv0.8以降）
                if (!village.MerchantsStationed.Contains(merchant))
                {
                    village.MerchantsVisiting.Remove(merchant);
                    village.MerchantsStationed.Add(merchant);
                    merchant.Tier = MerchantTier.Town;
                    Console.WriteLine($"[EconomicService] Merchant {merchant.Name} is now stationed!");
                }
            }
        }

        // ==========================================
        // 6-6. 経済 → 軍事 連動
        // ==========================================

        public int CalculateAcquisitionDays(Village village)
        {
            int baseDays = 30;
            
            int totalIndustryLevel = village.IndustrySlots.Sum(s => s.Level);
            
            double totalCredit = 0;
            int merchantCount = 0;
            foreach(var m in village.MerchantsVisiting) { totalCredit += m.Credit; merchantCount++; }
            foreach(var m in village.MerchantsStationed) { totalCredit += m.Credit; merchantCount++; }
            
            double avgCredit = merchantCount > 0 ? totalCredit / merchantCount : 0;

            // EconomicPower = 1.0 + (TotalIndustryLevel / 10) + (MerchantCreditAvg / 100)
            double economicPower = 1.0 + (totalIndustryLevel / 10.0) + (avgCredit / 100.0);

            return (int)(baseDays / economicPower);
        }

        public int CalculateMaxArmorLevel(Village village)
        {
            int baseMax = 3;
            int totalIndustryLevel = village.IndustrySlots.Sum(s => s.Level);
            bool hasMeisan = village.CurrentMeisan != null;

            // EconomicBonus = Floor(TotalIndustryLevel / 5) + (HasMeisan ? 1 : 0)
            int economicBonus = (totalIndustryLevel / 5) + (hasMeisan ? 1 : 0);

            return baseMax + economicBonus;
        }

        // ==========================================
        // 7. 時間進行統合ヘルパー
        // ==========================================

        public void UpdateMerchantsSchedule(Village village, int currentDay)
        {
            // 行商のスケジュール更新
            foreach (var merchant in village.MerchantsVisiting)
            {
                // 滞在期間終了チェックなどはここで行う
                // 次回来訪日設定
                if (merchant.NextVisitDay <= currentDay)
                {
                    merchant.NextVisitDay = currentDay + merchant.VisitFrequency;
                }
            }
        }

        public void ProcessMonthlyTax(Village village)
        {
            int totalIndustryLevel = village.IndustrySlots.Sum(s => s.Level);
            // TaxIncome = Population * 0.1 + TotalIndustryLevel * 5
            int tax = (int)(village.Population * 0.1) + (totalIndustryLevel * 5);
            village.TaxIncome = tax;
            village.Money += tax;
        }
    }
}
