using System.Collections.Generic;
using System.Globalization;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Client.Windows.Bot
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
                $"{Position.X.ToString(CultureInfo.InvariantCulture.NumberFormat)},{Position.Y.ToString(CultureInfo.InvariantCulture.NumberFormat)}";

            public static LinkedListNode<RecordableStep> From(MoveEventArgs move) =>
                new(new MoveStep(move.Position));
        }

        public class ProgressQuestStep : RecordableStep
        {
            public const string SerializedName = "quest";
            public override string Name => SerializedName;
            public override string CsvFormat => Name;
            protected override string CsvFormatBody => "";
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

            public virtual string CsvFormat => $"{Name},{CsvFormatBody}";
            protected abstract string CsvFormatBody {
                get;
            }
        }
    }
}
