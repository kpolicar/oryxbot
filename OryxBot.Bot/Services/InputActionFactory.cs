using System;
using System.Numerics;
using System.Threading;
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
        private Position currentPosition;
        private BotJob job;
        private bool rightMouseIsDown = false;


        public void BindDependencies(ServiceContainer serviceContainer) {
            input = serviceContainer.GetService<Input>();
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            bot = (serviceContainer.GetService<BotManagerContract>() as BotManager)!;
            
            dataProvider.Move += (_, e) => currentPosition = e.Position;
            bot.JobChanged += (sender, args) => job = args.Job;
        }

        public void MoveTowards(Position origin, Position target) =>
            MoveTowards(origin, target, false);

        public void MoveTowards(Position origin, Position target, bool forceReclick = false) {
            var direction = new Vector2(target.X - origin.X, target.Y - origin.Y);
            direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float)Math.PI/4));
            direction = Vector2.Normalize(direction);

            input.MoveCursorRelativeToCenter(direction);
            
            if (!rightMouseIsDown) {
                input.RightMouseDown();
                rightMouseIsDown = true;
            }

            if (job is TradeMissionRun run) {
                forceReclick |= !run.State.Moving;
            }
            if (forceReclick) {
                if (rightMouseIsDown)
                    input.RightMouseUp();
                Thread.Sleep(50);
                input.RightMouseDown(); 
            }
        }

        public void InteractWith(Position origin, Position target) {
            var direction = new Vector2(target.X - origin.X, target.Y - origin.Y);
            direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float)Math.PI/4));
            direction = Vector2.Normalize(direction);

            input.MoveCursorRelativeToCenter(direction);
            input.Click();
        }

        public void BankRewardItems() {
            input.ShiftClick(AlbionInterface.FirstItemInInventory);
        }

        public void UnbankTokenItem() {
            input.ShiftClick(AlbionInterface.FirstItemInBank);
        }

        public void NpcQuestOpenTradeMissionsTab() {
            input.Click(AlbionInterface.QuestNpcTradeMissionsTab);
        }

        public void NpcQuestOpenTradeMissionsContractTab() {
            input.Click(AlbionInterface.QuestNpcTradeMissionsContractTab);
        }

        public void NpcQuestSelectTradeMissionsContract() {
            input.Click(AlbionInterface.QuestNpcSelectTradeMissionContract);
        }

        public void NpcQuestAcceptTradeMissionsContract() {
            input.Click(AlbionInterface.QuestNpcAcceptTradeMissionContract);
        }

        public void NpcQuestProgress() {
            input.Click(AlbionInterface.QuestNpcProgressContract);
        }

        public void StopAllActions() {
            Console.WriteLine("Stopping all actions.");
            if (rightMouseIsDown) {
                rightMouseIsDown = false;
                input.RightMouseUp();
            }
            Thread.Sleep(15);
            input.Key('s');
        }
    }
}
