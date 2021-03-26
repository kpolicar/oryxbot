using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using OryxBot.Client.Windows.Bot;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;
using BotManager = OryxBot.Shared.Contracts.BotManager;

namespace OryxBot.Client.Windows.Services
{
    public class FileDialogTradeMissionRouteProvider : TradeMissionRouteProvider, HasDependencies
    {
        private MainForm app = null!;
        private BotManager bot = null!;
        
        private bool _customRoutes;
        public bool CustomRoutes {
            private set {
                _customRoutes = value;
                ModeChanged?.Invoke(this, EventArgs.Empty);
            }
            get => _customRoutes;
        }
        public event EventHandler? ModeChanged;
        
        public void BindDependencies(ServiceContainer serviceContainer) =>
            bot = serviceContainer.GetService<BotManager>();


        public LinkedList<TradeMissionRecord.RecordableStep>? Route() =>
            !CustomRoutes
                ? DefaultRoute()
                : RouteFromFileSelector();
        
        public LinkedList<TradeMissionRecord.RecordableStep>? RouteBack() =>
            !CustomRoutes
                ? DefaultRouteBack()
                : RouteFromFileSelector();

        private LinkedList<TradeMissionRecord.RecordableStep>? RouteFromFileSelector() {
            var result = app.TradeMissionRunRouteFile.ShowDialog();
            if (result != DialogResult.OK)
                return null;
            
            var path = app.TradeMissionRunRouteFile.FileName;
            if (path == null)
                return null;
            using var textStream = new StreamReader(path);
            return RouteFromStream(textStream.BaseStream);
        }
        
        public void ToggleCustomMode() =>
            CustomRoutes = !CustomRoutes;

        public LinkedList<TradeMissionRecord.RecordableStep>? DefaultRoute() =>
            RouteFromResource("OryxBot.Client.Windows.Bot.Resources.route_{city}_trademission.csv");

        public LinkedList<TradeMissionRecord.RecordableStep>? DefaultRouteBack() =>
            RouteFromResource("OryxBot.Client.Windows.Bot.Resources.route_{city}_trademission_back.csv");

        public LinkedList<TradeMissionRecord.RecordableStep>? RouteFromBankToQuest() =>
            RouteFromResource("OryxBot.Client.Windows.Bot.Resources.route_{city}_bank_to_quest.csv");

        public LinkedList<TradeMissionRecord.RecordableStep>? RouteFromQuestToBank() =>
            RouteFromResource("OryxBot.Client.Windows.Bot.Resources.route_{city}_quest_to_bank.csv");

        protected LinkedList<TradeMissionRecord.RecordableStep>? RouteFromResource(string resource) {
            resource = resource.Replace("{city}", CityResourcePrefix);
            using var resourceStream =
                Assembly.GetAssembly(GetType())!.GetManifestResourceStream(resource)!;
            
            var textStream = new StreamReader(resourceStream);
            return RouteFromStream(textStream.BaseStream);
        }

        public string CityResourcePrefix => bot.ActiveCity switch {
            City.Caerleon => "caerleon",
            City.Thetford => "thetford",
            City.FortSterling => "fortsterling",
            City.Lymhurst => "lymhurst",
            City.Bridgewatch => "bridgewatch",
            City.Martlock => "martlock",
            _ => throw new ArgumentOutOfRangeException()
        };

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
                    
                    var move = new MoveEventArgs(
                        float.Parse(fields[1], CultureInfo.InvariantCulture.NumberFormat), 
                        float.Parse(fields[2], CultureInfo.InvariantCulture.NumberFormat));
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

        public void BindToApp(MainForm app) =>
            this.app = app;
    }
}
