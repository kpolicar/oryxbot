using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Bot
{
    public partial class TradeMissionRecord
    {
        public class MoveStep : RecordableStep {
            public const string SerializedName = "move";
            public readonly Position Position;

            private MoveStep(Position position) =>
                Position = position;

            public static MoveStep From(MoveEventArgs args) {
                return new MoveStep(args.Position);
            }

            public override string Name => SerializedName;
            protected override string CsvFormatBody =>
                $"{Position.X},{Position.Y}";
        }
        
        public class ChangeClusterStep : RecordableStep {
            public const string SerializedName = "cluster";
            public readonly string Location;

            protected ChangeClusterStep(string location) =>
                Location = location;

            public static ChangeClusterStep From(ChangeClusterEventArgs args) {
                return new ChangeClusterStep(args.Location);
            }

            public override string Name => SerializedName;
            protected override string CsvFormatBody =>
                $"{Location}";
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
