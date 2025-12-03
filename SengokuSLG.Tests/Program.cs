using System;
using System.Linq;
using SengokuSLG.Models;
using SengokuSLG.Services;

namespace SengokuSLG.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting v0.9 Battle Logic Tests...");

            try
            {
                // 1. Initialize GameService (which initializes BattleService internally)
                var gameService = new GameService();
                Console.WriteLine("GameService Initialized.");

                // 2. Execute Battle
                Console.WriteLine("Executing Battle...");
                var context = gameService.ExecuteBattle();

                // 3. Verify Results
                Console.WriteLine($"Battle Ended. Outcome: {context.Outcome}");
                Console.WriteLine($"Total Turns: {context.TurnCount}");
                Console.WriteLine($"Logs Count: {context.Logs.Count}");

                // Check Squads
                var playerSoldiers = context.PlayerBattalion.TotalSoldiers;
                var enemySoldiers = context.EnemyBattalion.TotalSoldiers;
                Console.WriteLine($"Player Soldiers Remaining: {playerSoldiers}");
                Console.WriteLine($"Enemy Soldiers Remaining: {enemySoldiers}");

                // Check Casualties
                Console.WriteLine($"Total Casualties: {context.Casualties.Count}");
                var deaths = context.Casualties.Count(c => c.CasualtyType == CasualtyType.Death);
                var injuries = context.Casualties.Count(c => c.CasualtyType == CasualtyType.Injury);
                Console.WriteLine($"Deaths: {deaths}, Injuries: {injuries}");

                // Check Merits
                if (context.Merits.Any())
                {
                    var merit = context.Merits.First();
                    Console.WriteLine($"Player Merit: {merit.TotalScore}");
                }
                else
                {
                    Console.WriteLine("No Merits calculated.");
                }

                // Validation Logic
                if (context.TurnCount > 0 && context.Logs.Any())
                {
                    Console.WriteLine("TEST PASSED: Battle executed successfully.");
                }
                else
                {
                    Console.WriteLine("TEST FAILED: Battle did not execute properly.");
                    Environment.Exit(1);
                }

                if (context.Casualties.Any())
                {
                    Console.WriteLine("TEST PASSED: Casualties recorded.");
                }
                else
                {
                    Console.WriteLine("TEST WARNING: No casualties recorded (might be possible but unlikely).");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"TEST FAILED with Exception: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }
    }
}
