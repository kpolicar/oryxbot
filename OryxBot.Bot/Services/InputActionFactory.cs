using System;
using System.Numerics;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;


namespace OryxBot.Bot.Services
{
    public class InputActionFactory : ActionFactory, HasDependencies
    {
        private Input input = null!;
        private AlbionDataProvider dataProvider = null!;
        private BotManagerContract bot = null!;
        private Position currentPosition;


        public void BindDependencies(ServiceContainer serviceContainer) {
            input = serviceContainer.GetService<Input>();
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            bot = serviceContainer.GetService<BotManagerContract>();
            
            dataProvider.Move += (_, e) => currentPosition = e.Position;
        }

        public void MoveTowards(Position origin, Position target) {
            var direction = new Vector2(target.X - origin.X, target.Y - origin.Y);
            direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float)Math.PI/4));
            direction = Vector2.Normalize(direction);

            input.MoveCursorRelativeToCenter(direction);
            
            //input.RightMouseDown();
            //input.RightMouseUp();
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

        public void StopAllActions() {
            input.Key('s');
        }
    }
}
