using System;
using System.Collections.Generic;
using System.Linq;
using SengokuSLG.Models;

namespace SengokuSLG.Services
{
    public class MarriageService
    {
        private Random _random = new Random();

        public void GenerateMarriageOffers(House playerHouse, Player player, int currentMonth, System.Collections.ObjectModel.ObservableCollection<MonthlyEvent> monthlyEvents)
        {
            // Spec 3.1: Generate offers every 3 months
            if (currentMonth % 3 != 0) return;

            // Simple check: Don't generate if too many existing candidates
            if (playerHouse.MarriageCandidates.Count >= 5) return;

            // Generation conditions: Affinity, House Rank + Position
            // Simplified probability check (20% + House Rank bonus)
            double chance = 0.2 + (playerHouse.Kakaku * 0.01) + ((int)player.Rank * 0.05);
            if (_random.NextDouble() > chance) return;

            // Generate Offer
            // Spec 3.1: Focus on same rank or slightly lower
            int rankDiff = _random.Next(-2, 1); // -2, -1, 0
            
            var offer = new MarriageOffer
            {
                SourceHouseName = GenerateRandomHouseName(),
                TargetPersonId = player.Id,
                TargetPersonName = player.Name,
                CandidatePersonId = Guid.NewGuid().ToString(),
                CandidatePersonName = GenerateRandomName(false),
                IsLegalWife = player.MaritalStatus != MaritalStatus.Married,
                Dowry = _random.Next(1, 6) * 100, // 100-500 kan
                RankDifference = rankDiff
            };

            // Connection offer (probability)
            if (_random.NextDouble() > 0.7)
            {
                offer.ConnectionOffer.Add("Influential Merchant");
            }

            playerHouse.MarriageCandidates.Add(offer);
            
            // Generate event notification
            monthlyEvents.Add(new MonthlyEvent(EventType.Marriage, $"{offer.SourceHouseName} sent marriage proposal"));
        }

        public float CalculateSuccessProbability(House playerHouse, Player player, int targetHouseKakaku, int targetRank)
        {
            // 仕様書 3.2: 自家からの縁談成功率
            // BaseScore = (家格スコア * 0.6) + (役職スコア * 0.3) + (その他 * 0.1)
            
            // 簡易計算: 相手との差分で計算
            int kakakuDiff = playerHouse.Kakaku - targetHouseKakaku;
            int rankDiff = (int)player.Rank - targetRank;

            float baseProb = 50.0f; // 基礎50%
            baseProb += kakakuDiff * 5.0f; // 家格差1につき5%
            baseProb += rankDiff * 10.0f;  // 役職差1につき10%

            return Math.Clamp(baseProb, 10.0f, 90.0f); // 10-90%の範囲
        }

        public bool ProcessMarriage(House playerHouse, Player player, MarriageOffer offer)
        {
            // 成功判定
            float prob = CalculateSuccessProbability(playerHouse, player, 50, (int)Rank.Toshi); // 簡易的に相手ランクを仮定
            // 実際にはOfferに相手の情報が必要だが、ここでは簡易実装として確率を使用
            // UIで表示されているSuccessProbabilityと合わせる必要があるため、GameServiceから確率を受け取るか、ここで再計算するか
            // ここでは簡易的に 50% + ランク差 で判定
            
            // UIの表示とロジックを一致させるため、確率計算はGameService経由で行われているはず。
            // ここでは単純にランダム判定を行う。
            // ただし、CalculateSuccessProbabilityはHouseとPlayerから計算するが、Offerには相手の情報が足りない。
            // 既存のCalculateSuccessProbabilityを使用する。
            
            // 相手の家格とランクはOfferに含まれていないため、仮の値を使用するか、Offerにプロパティを追加する必要がある。
            // ここでは、Offer.RankDifference から逆算する。
            int targetRank = (int)player.Rank - offer.RankDifference;
            float probability = CalculateSuccessProbability(playerHouse, player, playerHouse.Kakaku, targetRank); // 家格差は0と仮定（Offerにないため）
            
            bool success = _random.NextDouble() * 100 < probability;

            if (!success) return false;

            // ステータス更新
            if (offer.IsLegalWife)
            {
                player.MaritalStatus = MaritalStatus.Married;
            }
            player.SpouseId = offer.CandidatePersonId;
            
            // 持参金
            // (Player model has Money, but House usually manages it. Assuming Player.Money is House Money)
            player.Money += offer.Dowry;

            // 家格経験値処理 (仕様書 3.3)
            if (offer.RankDifference >= 0) // 格上・同格
            {
                // 差が大きいほど増加
                int increase = 10 + (offer.RankDifference * 5);
                playerHouse.KakakuExp += increase;
            }
            else // 格下
            {
                if (offer.IsLegalWife)
                {
                    if (offer.RankDifference <= -3)
                    {
                        playerHouse.KakakuExp -= 50; // 大きく減少
                        // TODO: 家中の強い不満 (Loyalty penalty handled in GameService or here?)
                    }
                    else if (offer.RankDifference == -2)
                    {
                        playerHouse.KakakuExp -= 10; // 減少(小)
                    }
                    // 同格〜1差下: 影響なし
                }
                else // 側室
                {
                    if (offer.RankDifference <= -3)
                    {
                        playerHouse.KakakuExp -= 2; // ごく僅かに減少
                    }
                }
            }

            // 候補から削除
            var existing = playerHouse.MarriageCandidates.FirstOrDefault(x => x.Id == offer.Id);
            if (existing != null)
            {
                playerHouse.MarriageCandidates.Remove(existing);
            }
            
            return true;
        }

        private string GenerateRandomHouseName()
        {
            string[] names = { "織田", "武田", "上杉", "北条", "毛利", "島津", "伊達", "徳川" };
            return names[_random.Next(names.Length)] + "家分家";
        }

        private string GenerateRandomName(bool isMale)
        {
            string[] femaleNames = { "お市", "寧々", "まつ", "千代", "ガラシャ", "帰蝶", "茶々", "初", "江" };
            return femaleNames[_random.Next(femaleNames.Length)];
        }
    }
}
