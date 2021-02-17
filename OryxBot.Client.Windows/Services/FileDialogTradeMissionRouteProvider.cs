using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using OryxBot.Bot;
using OryxBot.Bot.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Client.Windows.Services
{
    public class FileDialogTradeMissionRouteProvider : TradeMissionRouteProvider
    {
        private UIApplicationContext App = null!;
        
        public LinkedList<TradeMissionRecord.RecordableStep>? Route() {
            var result = App.TradeMissionRunRouteFile.ShowDialog();
            if (result != DialogResult.OK)
                return null;
            
            var path = App.TradeMissionRunRouteFile.FileName;
            if (path == null)
                return null;

            using var parser = new TextFieldParser(path) {
                TextFieldType = FieldType.Delimited,
                Delimiters = new []{ "," }
            };

            var steps = new LinkedList<TradeMissionRecord.RecordableStep>();
            while (!parser.EndOfData) {
                var fields = parser.ReadFields();

                var step = fields[0];
                
                if (step == TradeMissionRecord.MoveStep.SerializedName) {
                    
                    var move = new MoveEventArgs(float.Parse(fields[1]), float.Parse(fields[2]));
                    steps.AddLast(TradeMissionRecord.MoveStep.From(move));
                } else if (step == TradeMissionRecord.ChangeClusterStep.SerializedName) {
                    
                    var changeCluster = new ChangeClusterEventArgs(fields[1]);
                    steps.AddLast(TradeMissionRecord.ChangeClusterStep.From(changeCluster));
                    
                } else {
                    Debug.Fail("Something went wrong");
                }
            }

            return steps;
        }

        public void BindToApp(UIApplicationContext app) =>
            App = app;
    }
}
