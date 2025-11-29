using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SengokuSLG.Models;

namespace SengokuSLG.Services
{
    public class GameService : INotifyPropertyChanged
    {
        private DateTime _currentDate;
        private bool _hasDonePublicDuty;

        public Player Player { get; private set; }
        public Village VillageA { get; private set; }
        public Village VillageB { get; private set; }
        public Lord Lord { get; private set; }
        public ObservableCollection<string> Logs { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public DateTime CurrentDate
        {
            get => _currentDate;
            private set { _currentDate = value; OnPropertyChanged(); }
        }

        public bool HasDonePublicDuty
        {
            get => _hasDonePublicDuty;
            private set { _hasDonePublicDuty = value; OnPropertyChanged(); }
        }

        public GameService()
        {
            Initialize();
        }

        private void Initialize()
        {
            Player = new Player();
            VillageA = new Village("村A", 1000, 50, 100, 10);
            VillageB = new Village("村B", 500, 30, 50, 5);
            Lord = new Lord();
            CurrentDate = new DateTime(1587, 3, 12); 
            Logs = new ObservableCollection<string>();
            HasDonePublicDuty = false;
        }

        public void ExecuteAction(string actionType, string target)
        {
            string log = $"{CurrentDate:MM/dd}: {actionType} ({target}) を実行しました。";
            
            if (actionType == "巡察")
            {
                var v = target == "村A" ? VillageA : VillageB;
                v.Security += 1;
                Player.Evaluation += 1;
                HasDonePublicDuty = true;
                log += " 治安+1, 評価+1";
            }
            else if (actionType == "治安維持")
            {
                var v = target == "村A" ? VillageA : VillageB;
                v.Security += 3;
                Player.Evaluation += 1;
                HasDonePublicDuty = true;
                log += " 治安+3, 評価+1";
            }
            else if (actionType == "書役仕事")
            {
                Player.Evaluation += 1;
                HasDonePublicDuty = true;
                log += " 評価+1";
            }
            else if (actionType == "開発")
            {
                var v = target == "村A" ? VillageA : VillageB;
                v.Development += 1;
                Player.Money -= 10;
                log += " 開発+1, 金-10";
            }
            else if (actionType == "取引")
            {
                Player.Money += 10;
                log += " 金+10";
            }

            Logs.Insert(0, log);
            AdvanceDay();
        }

        private void AdvanceDay()
        {
            CurrentDate = CurrentDate.AddDays(1);
            
            if (CurrentDate.Day == 1)
            {
                ProcessMonthly();
            }
        }

        private void ProcessMonthly()
        {
            int totalIncome = VillageA.Income + VillageB.Income;
            Player.Money += totalIncome;
            string log = $"【月次処理】収入+{totalIncome}";

            if (HasDonePublicDuty)
            {
                Player.Evaluation += 1;
                log += ", 公務評価+1";
            }

            HasDonePublicDuty = false;
            Logs.Insert(0, log);
            
            // Here we should trigger the Monthly Summary View.
            // In this simple architecture, we might need an event.
            OnMonthlyProcessed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnMonthlyProcessed;
    }
}
