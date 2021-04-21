using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
    public class FileDialogTradeMissionRouteManager : TradeMissionRouteManager, HasDependencies
    {
        private MainForm app = null!;
        private BotManager bot = null!;
        
        private bool _customRoutes;
        private City? _defaultCity;

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


        public TradeMissionRoute? Route() =>
            !CustomRoutes
                ? DefaultRoute()
                : RouteFromFileSelector() as TradeMissionRoute;

        private Route? RouteFromFileSelector() {
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

        public StreamWriter SaveRouteStream() {
            var result = app.TradeMissionSaveRouteFile.ShowDialog(app);
            if (result != DialogResult.OK) {
                var confirmation = MessageBox.Show(
                    "If you do not save the route, it will be discarded!"+
                    "Are you sure you want to delete the recorded route?",
                    "Are you sure?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (confirmation != DialogResult.Yes) {
                    return SaveRouteStream();
                }
                return StreamWriter.Null;
            }

            return new StreamWriter(app.TradeMissionSaveRouteFile.OpenFile());
        }

        public void SetDefaultRouteCity(City city) =>
            _defaultCity = city;

        public TradeMissionRoute? DefaultRoute() =>
            RouteFromResource("OryxBot.Client.Windows.Bot.Resources.route_{city}_trademission.csv") as TradeMissionRoute;

        public Route? RouteFromBankToQuest() =>
            RouteFromResource("OryxBot.Client.Windows.Bot.Resources.route_{city}_bank_to_quest.csv");

        public Route? RouteFromQuestToBank() =>
            RouteFromResource("OryxBot.Client.Windows.Bot.Resources.route_{city}_quest_to_bank.csv");

        protected Route? RouteFromResource(string resource) {
            try {
                resource = resource.Replace("{city}", DefaultRouteCityResourcePrefix());
                using var resourceStream =
                    Assembly.GetAssembly(GetType())!.GetManifestResourceStream(resource)!;

                var textStream = new StreamReader(resourceStream);
                return RouteFromStream(textStream.BaseStream);
            } catch (Exception) {
                Debug.WriteLine("failed "+resource);
                return new Route();
            }
        }

        private string DefaultRouteCityResourcePrefix() => _defaultCity switch {
            City.Caerleon => "caerleon",
            City.Thetford => "thetford",
            City.FortSterling => "fortsterling",
            City.Lymhurst => "lymhurst",
            City.Bridgewatch => "bridgewatch",
            City.Martlock => "martlock",
            _ => throw new ArgumentOutOfRangeException()
        };

        protected Route RouteFromStream(Stream stream) {
            using var parser = new TextFieldParser(stream) {
                TextFieldType = FieldType.Delimited,
                Delimiters = new []{ "," }
            };

            var metadata = new Dictionary<string, string>();

            var steps = new Route();
            Route? stepsBeforeQuest = null;
            
            
            while (!parser.EndOfData) {
                var fields = parser.ReadFields();
                
                if (fields[0] == "metadata") {
                    metadata = fields
                        .Skip(1)
                        .ToDictionary(
                            s => s.Split(':')[0], 
                            s => s.Split(':')[1]);
                    
                    if (metadata.ContainsKey("name"))
                        steps.Name = metadata["name"];
                    if (metadata.ContainsKey("origin"))
                        steps.Origin = Regions.Region(metadata["origin"]);
                    if (metadata.ContainsKey("destination"))
                        steps.Destination = Regions.Region(metadata["destination"]);
                    
                    continue;
                }

                var step = fields[0];
                
                if (step == TradeMissionRecord.MoveStep.SerializedName) {
                    
                    var move = new MoveEventArgs(
                        float.Parse(fields[1], CultureInfo.InvariantCulture.NumberFormat), 
                        float.Parse(fields[2], CultureInfo.InvariantCulture.NumberFormat));
                    steps.AddLast(TradeMissionRecord.MoveStep.From(move));
                } else if (step == TradeMissionRecord.ChangeClusterStep.SerializedName) {
                    
                    var changeCluster = new ChangeClusterEventArgs(fields[1]);
                    steps.AddLast(TradeMissionRecord.ChangeClusterStep.From(changeCluster));
                    
                } else if (step == "quest") {
                    stepsBeforeQuest = steps;
                    steps = new Route();
                } else {
                    Debug.Fail("Something went wrong");
                }
            }
            
            return stepsBeforeQuest != null
                ? new TradeMissionRoute(stepsBeforeQuest, steps)
                : steps;
        }

        public void BindToApp(MainForm app) =>
            this.app = app;
    }
}
