using System;
using System.Collections.Generic;
using System.Linq;

namespace SengokuSLG.Models
{
    // --- Enums ---
    public enum SoldierType
    {
        Ashigaru,   // 徒士
        Mounted,    // 馬上衆
        Servant     // 従僕
    }

    public enum SquadPosition
    {
        Forward,    // 前線
        Rear        // 後衛
    }

    public enum CommanderPersonality
    {
        Cautious,   // 慎重 (Fatigue 70)
        Standard,   // 標準 (Fatigue 80)
        Aggressive  // 猪突 (Fatigue 90)
    }

    public enum CasualtyType
    {
        Death,      // 死亡
        Disabled,   // 廃兵
        Injury,     // 負傷
        Fleeing     // 逃散
    }

    public enum BattlePhase
    {
        Pushing,    // 押し合い
        Pursuit     // 追撃
    }

    public enum BattleResultType
    {
        Win,        // 勝ち
        Draw,       // 拮抗
        Lose        // 負け
    }

    public enum BattleOutcome
    {
        GreatVictory,   // 大勝
        Victory,        // 勝ち
        Draw,           // 引き分け
        Defeat,         // 敗北
        CrushingDefeat  // 大敗
    }

    // --- Data Models ---

    public class BattleSoldier
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public SoldierType Type { get; set; }
        public string OriginVillageId { get; set; } = "";
        public int CombatPower { get; set; }
        public bool IsOwn { get; set; } // 自家兵 or 借り兵
        public string SquadId { get; set; } = "";
    }

    public class BattleSquad
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string CommanderId { get; set; } = ""; // 小頭
        public List<BattleSoldier> Soldiers { get; set; } = new List<BattleSoldier>();
        
        // Stats (0-100)
        public int Training { get; set; }
        public int Experience { get; set; }
        public int Fatigue { get; set; }
        public int Morale { get; set; }
        public int Formation { get; set; }

        public SquadPosition Position { get; set; } = SquadPosition.Forward;
        public bool IsConfused { get; set; }
        public bool IsCollapsing { get; set; } // 士気0で崩壊

        // Calculated Properties
        public int CurrentSoldierCount => Soldiers.Count;
        public int BasePower => Soldiers.Where(s => s.Type != SoldierType.Servant).Sum(s => s.CombatPower); // 従僕は戦力外

        // Helper for UI
        public int AshigaruCount => Soldiers.Count(s => s.Type == SoldierType.Ashigaru);
        public int MountedCount => Soldiers.Count(s => s.Type == SoldierType.Mounted);
        public int ServantCount => Soldiers.Count(s => s.Type == SoldierType.Servant);
    }

    public class BattleCompany
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string CommanderId { get; set; } = ""; // 組頭
        public CommanderPersonality Personality { get; set; } = CommanderPersonality.Standard;
        public List<BattleSquad> Squads { get; set; } = new List<BattleSquad>();
        public SquadPosition Position { get; set; } = SquadPosition.Forward;
        public bool IsCollapsing { get; set; }

        // Aggregated Stats
        public double AverageTraining => Squads.Any() ? Squads.Average(s => s.Training) : 0;
        public double AverageExperience => Squads.Any() ? Squads.Average(s => s.Experience) : 0;
        public double AverageFatigue => Squads.Any() ? Squads.Average(s => s.Fatigue) : 0;
        public double AverageMorale => Squads.Any() ? Squads.Average(s => s.Morale) : 0;
        public int TotalSoldiers => Squads.Sum(s => s.CurrentSoldierCount);
    }

    public class BattleBattalion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string CommanderId { get; set; } = ""; // 足軽大将
        public List<BattleCompany> Companies { get; set; } = new List<BattleCompany>();
        public bool IsPlayer { get; set; }
        public bool IsEnemy { get; set; }

        // Aggregated Stats
        public double AverageTraining => Companies.Any() ? Companies.Average(c => c.AverageTraining) : 0;
        public double AverageExperience => Companies.Any() ? Companies.Average(c => c.AverageExperience) : 0;
        public double AverageFatigue => Companies.Any() ? Companies.Average(c => c.AverageFatigue) : 0;
        public double AverageMorale => Companies.Any() ? Companies.Average(c => c.AverageMorale) : 0;
        public int TotalSoldiers => Companies.Sum(c => c.TotalSoldiers);
    }

    public class BattleCasualty
    {
        public string SoldierId { get; set; } = "";
        public string Name { get; set; } = "";
        public SoldierType Type { get; set; }
        public string OriginVillageId { get; set; } = "";
        public bool IsOwn { get; set; }
        public string SquadId { get; set; } = "";
        public CasualtyType CasualtyType { get; set; }
        public BattlePhase Phase { get; set; }
    }

    public class WarMerit
    {
        public string VassalId { get; set; } = "";
        public string VassalName { get; set; } = "";
        public Rank Rank { get; set; }
        
        public int MoraleDamageScore { get; set; }
        public int CollapseBonusScore { get; set; }
        public int PursuitDamageScore { get; set; }
        public int EfficiencyBonusScore { get; set; }
        public double BattleModifier { get; set; } = 1.0;
        public int BorrowedTroopPenalty { get; set; }
        
        public int TotalScore => (int)((MoraleDamageScore + CollapseBonusScore + PursuitDamageScore + EfficiencyBonusScore) * BattleModifier) - BorrowedTroopPenalty;
    }

    public class PushingState
    {
        public int SelfPower { get; set; }
        public int EnemyPower { get; set; }
        public BattleResultType Result { get; set; }
        public int PowerDifference => SelfPower - EnemyPower;
    }

    public class BattleContextV09
    {
        public int TurnCount { get; set; } = 0;
        public BattleBattalion PlayerBattalion { get; set; } = new BattleBattalion();
        public BattleBattalion EnemyBattalion { get; set; } = new BattleBattalion();
        public List<string> Logs { get; set; } = new List<string>();
        public PushingState CurrentPushingState { get; set; } = new PushingState();
        public List<BattleCasualty> Casualties { get; set; } = new List<BattleCasualty>();
        public List<WarMerit> Merits { get; set; } = new List<WarMerit>();
        public bool IsBattleEnded { get; set; }
        public BattleOutcome Outcome { get; set; }

        public void AddLog(string message)
        {
            Logs.Add($"[{TurnCount}] {message}");
        }
    }
}
