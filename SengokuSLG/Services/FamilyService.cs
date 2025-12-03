using System;
using System.Collections.Generic;
using System.Linq;
using SengokuSLG.Models;

namespace SengokuSLG.Services
{
    public class FamilyService
    {
        private Random _random = new Random();

        public void CheckBirth(Player player, List<Vassal> vassals, int currentYear, System.Collections.ObjectModel.ObservableCollection<MonthlyEvent> monthlyEvents)
        {
            // Only for married players
            if (player.MaritalStatus != MaritalStatus.Married) return;

            // Probability check (about once per year)
            // Simple implementation: 10%
            if (_random.NextDouble() > 0.1) return;

            // Generate child
            var child = new Vassal
            {
                Name = GenerateChildName(true), // Temporary: male
                Age = 0,
                Rank = Rank.Juboku,
                IsAdult = false,
                IsGenpuku = false,
                FatherId = player.Id,
                MotherId = player.SpouseId,
                BirthYear = currentYear,
                Origin = "Family",
                PersonalRole = PersonalRole.None, // No role yet
                Loyalty = 100 // High for parent's child
            };

            // Generate abilities (parent average ± random)
            int parentAvg = (player.AbilityCombat + player.AbilityPolitics) / 2;
            child.AbilityCombat = Math.Clamp(parentAvg + _random.Next(-10, 11), 1, 100);
            child.AbilityLeadership = Math.Clamp(parentAvg + _random.Next(-10, 11), 1, 100);
            child.AbilityPolitics = Math.Clamp(parentAvg + _random.Next(-10, 11), 1, 100);
            child.AbilityIntrigue = Math.Clamp(parentAvg + _random.Next(-10, 11), 1, 100);

            vassals.Add(child);
            
            // Generate event notification
            monthlyEvents.Add(new MonthlyEvent(EventType.Birth, $"Child born: {child.Name}"));
        }

        public List<string> CheckGenpuku(List<Vassal> vassals, Player player, System.Collections.ObjectModel.ObservableCollection<MonthlyEvent> monthlyEvents)
        {
            var genpukuList = new List<string>();

            foreach (var vassal in vassals)
            {
                if (!vassal.IsGenpuku && vassal.Age >= 15)
                {
                    // Genpuku processing
                    vassal.IsGenpuku = true;
                    vassal.IsAdult = true; // Adult at 15

                    // Determine initial rank (Spec 3.5)
                    Rank initialRank = DetermineInitialRank(player.Rank, vassal.PersonalRole);
                    vassal.Rank = initialRank;

                    string message = $"{vassal.Name} came of age and was appointed as {RankToText(initialRank)}";
                    genpukuList.Add(message);
                    
                    // Generate event notification
                    monthlyEvents.Add(new MonthlyEvent(EventType.Genpuku, message));
                }
            }

            return genpukuList;
        }

        public Rank DetermineInitialRank(Rank parentRank, PersonalRole childRole)
        {
            // 1. Base value: Parent rank - 2 levels
            int parentRankVal = (int)parentRank;
            int baseRankVal = Math.Max(0, parentRankVal - 2); // 0 = Juboku

            // 2. Rank corresponding to personal role
            int roleRankVal = RoleToRankValue(childRole);

            // 3. Decision: Max(base value, personal role)
            int finalRankVal = Math.Max(baseRankVal, roleRankVal);

            // 4. Range limit: Upper limit is Kumigashira(2)
            finalRankVal = Math.Min(finalRankVal, (int)Rank.Kumigashira);
            finalRankVal = Math.Max(finalRankVal, (int)Rank.Juboku);

            return (Rank)finalRankVal;
        }

        private int RoleToRankValue(PersonalRole role)
        {
            switch (role)
            {
                case PersonalRole.Servant: return 0; // Juboku
                case PersonalRole.ToshiAssistant: return 1; // Toshi
                case PersonalRole.ToshiPractical: return 1; // Toshi
                case PersonalRole.ToshiSenior: return 1; // Toshi
                case PersonalRole.KumigashiraPractical: return 2; // Kumigashira
                case PersonalRole.BushoAssistant: return 2; // Kumigashira (capped)
                default: return 0;
            }
        }

        private string RankToText(Rank rank)
        {
            switch (rank)
            {
                case Rank.Juboku: return "Juboku";
                case Rank.Toshi: return "Toshi";
                case Rank.Kumigashira: return "Kumigashira";
                case Rank.Busho: return "Busho";
                case Rank.Jidaisho: return "Jidaisho";
                default: return rank.ToString();
            }
        }

        private string GenerateChildName(bool isMale)
        {
            string[] names = { "Taro", "Jiro", "Saburo", "Shiro", "Goro" };
            return names[_random.Next(names.Length)];
        }
    }
}
