using System;
using System.Collections.Generic;
using System.Linq;
using SengokuSLG.Models;

namespace SengokuSLG.Services
{
    public class SuccessionService
    {
        public SuccessionContext PrepareSuccession(House house, Player deceasedLord, Vassal heir)
        {
            var context = new SuccessionContext
            {
                HeirId = heir.Id,
                InitialRank = DetermineInitialRank(deceasedLord.Rank, heir.PersonalRole)
            };

            // 継承率設定 (仕様書 6.1)
            context.InheritanceRates["Kakaku"] = 1.0f;
            context.InheritanceRates["Debts"] = 0.6f;
            context.InheritanceRates["MerchantCredit"] = 0.9f;
            context.InheritanceRates["TempleCredit"] = 0.9f;
            context.InheritanceRates["Connections"] = 1.0f;

            // 忠誠低下 (仕様書 6.1)
            // 成人前(IsAdult=false)なら-20, 成人後なら-10
            context.LoyaltyPenalty = heir.IsAdult ? 10 : 20;

            return context;
        }

        public void ApplySuccession(House house, Player player, Vassal heir, SuccessionContext context, List<Vassal> vassals)
        {
            // 1. パラメータ継承 (House)
            // Debts
            var newDebts = new Dictionary<string, int>();
            foreach (var debt in house.Debts)
            {
                newDebts[debt.Key] = (int)(debt.Value * context.InheritanceRates["Debts"]);
            }
            house.Debts = newDebts;

            // Credits
            house.MerchantCredit = (int)(house.MerchantCredit * context.InheritanceRates["MerchantCredit"]);
            house.TempleCredit = (int)(house.TempleCredit * context.InheritanceRates["TempleCredit"]);

            // 2. プレイヤー(当主)の入れ替え
            // ここではPlayerオブジェクトのプロパティをHeirのものに書き換えるアプローチをとる
            // (参照を維持するため)
            player.Name = heir.Name;
            player.Age = heir.Age;
            player.AbilityCombat = heir.AbilityCombat;
            player.AbilityLeadership = heir.AbilityLeadership;
            player.AbilityPolitics = heir.AbilityPolitics;
            player.AbilityIntrigue = heir.AbilityIntrigue;
            player.Rank = context.InitialRank; // 初期役職適用
            player.PersonalRole = heir.PersonalRole;
            player.MaritalStatus = MaritalStatus.Single; // 未婚リセット(配偶者は継承しない)
            player.SpouseId = null;
            player.IsAdult = heir.IsAdult;
            player.IsGenpuku = heir.IsGenpuku;
            player.BirthYear = heir.BirthYear;
            // IDはPlayerのまま維持するか、HeirのIDにするか。
            // 既存システムがPLAYER IDに依存している可能性があるため、IDは維持するが、
            // 別人であることを示すためにName等は変更済み。

            // 3. 忠誠低下
            foreach (var vassal in vassals)
            {
                if (vassal.Id == heir.Id) continue; // 継承者自身は除外
                vassal.Loyalty = Math.Max(0, vassal.Loyalty - context.LoyaltyPenalty);
            }

            // 4. 世代カウント
            house.GenerationCount++;

            // 5. Heirを家臣リストから削除 (当主になったため)
            var heirInList = vassals.FirstOrDefault(v => v.Id == heir.Id);
            if (heirInList != null)
            {
                vassals.Remove(heirInList);
            }
        }

        private Rank DetermineInitialRank(Rank parentRank, PersonalRole childRole)
        {
            // 仕様書 3.6 / 5.3 / 6.2
            // 1. 基準値: 親の役職 - 2段階
            int parentRankVal = (int)parentRank;
            int baseRankVal = Math.Max(0, parentRankVal - 2);

            // 2. 本人役割に対応する役職
            int roleRankVal = RoleToRankValue(childRole);

            // 3. 決定: Max(基準値, 本人役割)
            int finalRankVal = Math.Max(baseRankVal, roleRankVal);

            // 4. 範囲制限: 上限は組頭(2)
            finalRankVal = Math.Min(finalRankVal, (int)Rank.Kumigashira);
            finalRankVal = Math.Max(finalRankVal, (int)Rank.Juboku);

            return (Rank)finalRankVal;
        }

        private int RoleToRankValue(PersonalRole role)
        {
            switch (role)
            {
                case PersonalRole.Servant: return 0;
                case PersonalRole.ToshiAssistant: return 1;
                case PersonalRole.ToshiPractical: return 1;
                case PersonalRole.ToshiSenior: return 1;
                case PersonalRole.KumigashiraPractical: return 2;
                case PersonalRole.BushoAssistant: return 2;
                default: return 0;
            }
        }
    }
}
