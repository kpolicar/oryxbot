using System.Collections.Generic;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Bot
{
    public partial class TradeMissionRecord
    {
        public class MoveStep : RecordableStep {
            public const string SerializedName = "move";
            public readonly Position Position;

            public MoveStep(Position position) =>
                Position = position;

            public override string Name => SerializedName;
            protected override string CsvFormatBody =>
                $"{Position.X},{Position.Y}";

            public static LinkedListNode<RecordableStep> From(MoveEventArgs move) =>
                new(new MoveStep(move.Position));
        }
        
        public class ChangeClusterStep : RecordableStep {
            public const string SerializedName = "cluster";
            public readonly string Location;

            public ChangeClusterStep(string location) =>
                Location = location;

            public override string Name => SerializedName;
            protected override string CsvFormatBody =>
                $"{Location}";

            public static LinkedListNode<RecordableStep> From(ChangeClusterEventArgs changeCluster) =>
                new(new ChangeClusterStep(changeCluster.Location));
        }
        
        public abstract class RecordableStep
        {
            protected RecordableStep() {
            }
            
            public abstract string Name {
                get;
            }

            public string CsvFormat => $"{Name},{CsvFormatBody}";
            protected abstract string CsvFormatBody {
                get;
            }
        }
    }
}
