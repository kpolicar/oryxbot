using System;
using System.Numerics;
using System.Threading;
using OryxBot.Bot.Game;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;


namespace OryxBot.Bot.Services
{
    public class InputActionFactory : ActionFactory, HasDependencies
    {
        private Input input = null!;
        private AlbionDataProvider dataProvider = null!;
        private BotManager bot = null!;
        private BotJob job;
        private bool rightMouseIsDown = false;
        private Position previousTarget;


        public void BindDependencies(ServiceContainer serviceContainer) {
            input = serviceContainer.GetService<Input>();
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            bot = (serviceContainer.GetService<BotManagerContract>() as BotManager)!;
            
            bot.JobChanged += (sender, args) => job = args.Job;
        }

        public void MoveTowards(Position target) =>
            MoveTowards(target, false);

        public void MoveInSameDirection() =>
            MoveTowards(previousTarget);
        
        public void MoveTowards(Position target, bool forceReclick = false) {
            var fixingCourse = false;
            EnforceBotIsRunning();
            var origin = LocalCharacter.Instance.Position;
            
            var direction = new Vector2(target.X - origin.X, target.Y - origin.Y);
            
            direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float)Math.PI/4));
            
            // Try to get unstuck
            if (!LocalCharacter.Instance.Moving && !LocalCharacter.Instance.RecentlyChangedCluster) {
                var rand = new Random();
                var directionToChange = rand.Next(-1, 1);
                fixingCourse = true;
                direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(directionToChange * (7f * (float)Math.PI/6f)));
            }
            
            direction = Vector2.Normalize(direction);

            
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
            previousTarget = target;
            
            if (fixingCourse)
                Thread.Sleep(1000);
        }

        public void InteractWith(Position target) {
            EnforceBotIsRunning();
            var origin = LocalCharacter.Instance.Position;
            var direction = new Vector2(target.X - origin.X, target.Y - origin.Y);
            direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float)Math.PI/4));
            direction = Vector2.Normalize(direction);

            input.MoveCursorRelativeToCenter(direction);
            input.Click();
        }

        public void BankRewardItems() {
            EnforceBotIsRunning();
            input.ShiftClick(AlbionInterface.FirstItemInInventory);
        }

        public void UnbankTokenItem() =>
            UnbankTokenItem(1000);
        
        public void UnbankTokenItem(int delay) {
            EnforceBotIsRunning();
            input.Click(AlbionInterface.FirstItemInBank);
            
            Thread.Sleep(delay);
            EnforceBotIsRunning();
            input.Click(AlbionInterface.IncreaseSplitQuantityButton);
            
            Thread.Sleep(delay);
            EnforceBotIsRunning();
            input.Click(AlbionInterface.IncreaseSplitQuantityButton);
            
            Thread.Sleep(delay);
            EnforceBotIsRunning();
            input.Click(AlbionInterface.SplitButton);
            
            Thread.Sleep(delay);
            EnforceBotIsRunning();
            input.Click(AlbionInterface.CloseSplitButton);
            
            Thread.Sleep(delay);
            EnforceBotIsRunning();
            input.ShiftClick(AlbionInterface.SecondItemInBank);
        }

        public void NpcQuestOpenTradeMissionsTab() {
            EnforceBotIsRunning();
            input.Click(AlbionInterface.QuestNpcTradeMissionsTab);
        }

        public void NpcQuestOpenTradeMissionsContractTab() {
            EnforceBotIsRunning();
            input.Click(AlbionInterface.QuestNpcTradeMissionsContractTab);
        }

        public void NpcQuestSelectTradeMissionsContract() {
            EnforceBotIsRunning();
            input.Click(AlbionInterface.QuestNpcSelectTradeMissionContract);
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
            Console.WriteLine(@"Stopping all actions.");
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
