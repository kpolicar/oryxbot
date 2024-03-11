using System;
using System.Drawing;
using System.Numerics;
using System.Threading;
using OryxBot.Client.Linux.Bot.Game;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
using static OryxBot.Shared.RunConfiguration;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;


namespace OryxBot.Client.Linux.Bot.Services
{
    public class InputActionFactory : ActionFactory, HasDependencies
    {
        private Input input = null!;
        private AlbionDataProvider dataProvider = null!;
        private BotManager bot = null!;
        private BotJob job;
        private bool rightMouseIsDown = false;
        private Vector2 previousDirection;


        public void BindDependencies(ServiceContainer serviceContainer) {
            input = serviceContainer.GetService<Input>();
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            bot = (serviceContainer.GetService<BotManagerContract>() as BotManager)!;
            
            bot.JobChanged += (sender, args) => job = args.Job;
            bot.Stopped += (_, _) => {
                if (rightMouseIsDown)
                    input.RightMouseUp();
            };
        }

        public void MoveTowards(Position target) =>
            MoveTowards(target, false, false);

        public void MoveInSameDirection() =>
            MoveTowards(previousDirection);

        public void MoveAwayFrom(Position target) {
            var origin = LocalCharacter.Instance.PredictedPosition;
            var direction = new Vector2(target.X - origin.X, target.Y - origin.Y);
            direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float)System.Math.PI/4));
            direction = Vector2.Transform(direction, Matrix3x2.CreateRotation((float) System.Math.PI));
            direction = Vector2.Normalize(direction);
            previousDirection = direction;
            
            MoveTowards(direction);
        }

        public void MoveTowards(Position target, bool tryGetUnstuck, bool forceReclick = false) {
            var fixingCourse = false;
            EnforceBotIsRunning();
            var origin = LocalCharacter.Instance.PredictedPosition;
            
            var direction = new Vector2(target.X - origin.X, target.Y - origin.Y);
            
            direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float)System.Math.PI/4));
            
            // Try to get unstuck
            if (tryGetUnstuck) {
                fixingCourse = true;
                
                direction = LocalCharacter.Instance.RecentlyChangedCluster
                    ? Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float) System.Math.PI / 3f))
                    : Vector2.Transform(direction, Matrix3x2.CreateRotation(-2f*(float) System.Math.PI / 3f));
            }
            
            direction = Vector2.Normalize(direction);
            previousDirection = direction;

            
            MoveTowards(direction);
            if (fixingCourse)
                Thread.Sleep(1500);
        }

        private void MoveTowards(Vector2 direction, bool forceReclick=false) {
            var gameDirection = Vector2.Transform(direction, Matrix3x2.CreateRotation((float) System.Math.PI / 4));
            LocalCharacter.Instance.Direction = gameDirection;
            
            input.MoveCursorRelativeToCenter(direction);
            
            if (!rightMouseIsDown) {
                input.RightMouseDown();
                rightMouseIsDown = true;
            }

            if (job is TradeMissionRun) {
                forceReclick |= !LocalCharacter.Instance.Moving;
            }
            if (forceReclick) {
                if (rightMouseIsDown)
                    input.RightMouseUp();
                Thread.Sleep(50);
                input.RightMouseDown(); 
            }
        }

        public void InteractWith(Position target) {
            EnforceBotIsRunning();
            var origin = LocalCharacter.Instance.PredictedPosition;
            var direction = new Vector2(target.X - origin.X, target.Y - origin.Y);
            direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float)System.Math.PI/4));
            direction = Vector2.Normalize(direction);
            LocalCharacter.Instance.Direction = direction;

            if (rightMouseIsDown) {
                rightMouseIsDown = false;
                input.RightMouseUp();
            }
            input.MoveCursorRelativeToCenter(direction);
            input.Click();
        }

        public void BankRewardItems() {
            EnforceBotIsRunning();
            input.DragAndDrop(AlbionInterface.FirstItemInInventory, AlbionInterface.ThirdItemInBank);
        }

        public void UnbankTokenItem(ContractType contract) =>
            UnbankTokenItem(contract, 300);
        
        public void UnbankTokenItem(ContractType contract, int delay) {
            EnforceBotIsRunning();
            input.Click(AlbionInterface.FirstItemInBank);

            var numOfSplits = (int) contract - 1;

            for (int i = 0; i < numOfSplits; i++) {
                Thread.Sleep(delay);
                EnforceBotIsRunning();
                input.Click(AlbionInterface.IncreaseSplitQuantityButton);
            }
            
            Thread.Sleep(delay);
            EnforceBotIsRunning();
            input.Click(AlbionInterface.SplitButton);
            
            Thread.Sleep(delay);
            EnforceBotIsRunning();
            input.Click(AlbionInterface.CloseSplitButton);
            
            Thread.Sleep(delay);
            EnforceBotIsRunning();
            input.DragAndDrop(AlbionInterface.SecondItemInBank, AlbionInterface.FirstItemInInventory);
        }

        public void NpcQuestOpenTradeMissionsTab() {
            EnforceBotIsRunning();
            input.Click(AlbionInterface.QuestNpcTradeMissionsTab);
        }

        public void NpcQuestOpenTradeMissionsContractTab(City city, City destination) {
            EnforceBotIsRunning();
            input.Click(AlbionInterface.QuestNpcOpenTradeMissionContract(city, destination));
        }

        public void NpcQuestSelectTradeMissionsContract(City city, City destination, ContractType contract=ContractType.Minor) {
            EnforceBotIsRunning();

            input.Click(AlbionInterface.QuestNpcSelectTradeMissionContract(city, destination, contract));
        }

        public void NpcQuestAcceptTradeMissionsContract() {
            EnforceBotIsRunning();
            input.Click(AlbionInterface.QuestNpcAcceptTradeMissionContract);
        }

        public void NpcQuestProgress() {
            EnforceBotIsRunning();
            input.Click(AlbionInterface.QuestNpcProgressContract);
        }

        public void StopAllActions() {
            EnforceBotIsRunning();
            if (rightMouseIsDown) {
                rightMouseIsDown = false;
                input.RightMouseUp();
            }
            Thread.Sleep(15);
            input.Key('s');
        }

        public void CenterCursor() {
            EnforceBotIsRunning();
            input.MoveCursorRelativeToCenter(new Vector2(0,0));
        }

        private void EnforceBotIsRunning() {
            if (!bot.IsRunning)
                throw new OperationCanceledException();
        }

        public void Respawn() {
            EnforceBotIsRunning();
            input.Click(AlbionInterface.RespawnButton);
        }
    }
}
