using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using OryxBot.Client.Windows.Bot;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Shared.Events;

namespace OryxBot.Client.Windows.Services
{
    public class FileDialogTradeMissionRouteProvider : TradeMissionRouteProvider
    {
        private UIApplicationContext App = null!;
        
        public LinkedList<TradeMissionRecord.RecordableStep>? Route() {
            // var result = App.TradeMissionRunRouteFile.ShowDialog();
            // if (result != DialogResult.OK)
            //     return null;
            //
            // var path = App.TradeMissionRunRouteFile.FileName;
            // if (path == null)
            //     return null;
            // using var textStream = new StreamReader(path);
            // return RouteFromStream(textStream.BaseStream);
            
            var resource = "Bot.Resources.route_lymhurst_bank_to_quest.csv";
            using var resourceStream =
                Assembly.GetAssembly(typeof(OryxBot.Client.Windows.Bot.BotManager))!
                    .GetManifestResourceStream(resource)!;
            
            using var textStream = new StreamReader(resourceStream);
            return RouteFromStream(textStream.BaseStream);
        }

        public LinkedList<TradeMissionRecord.RecordableStep>? RouteBack() {
            var resource = "Bot.Resources.route_lymhurst_trademission.csv";
            using var resourceStream =
                Assembly.GetAssembly(typeof(OryxBot.Client.Windows.Bot.BotManager))!
                    .GetManifestResourceStream(resource)!;
            
            using var textStream = new StreamReader(resourceStream);
            return RouteFromStream(textStream.BaseStream);
        }

        public LinkedList<TradeMissionRecord.RecordableStep>? RouteFromBankToQuest() {
            var resource = "Bot.Resources.route_lymhurst_trademission_back.csv";
            using var resourceStream =
                Assembly.GetAssembly(typeof(OryxBot.Client.Windows.Bot.BotManager))!
                    .GetManifestResourceStream(resource)!;
            
            using var textStream = new StreamReader(resourceStream);
            return RouteFromStream(textStream.BaseStream);
        }

        public LinkedList<TradeMissionRecord.RecordableStep>? RouteFromQuestToBank() {
            var resource = "Bot.Resources.route_lymhurst_quest_to_bank.csv";
            using var resourceStream =
                Assembly.GetAssembly(typeof(OryxBot.Client.Windows.Bot.BotManager))!
                    .GetManifestResourceStream(resource)!;
            
            var textStream = new StreamReader(resourceStream);
            return RouteFromStream(textStream.BaseStream);
        }

        protected LinkedList<TradeMissionRecord.RecordableStep>? RouteFromStream(Stream stream) {
            using var parser = new TextFieldParser(stream) {
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
