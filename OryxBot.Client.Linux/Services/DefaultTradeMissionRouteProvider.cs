using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
using OryxBot.Client.Linux.Bot;
using OryxBot.Client.Linux.Bot.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;
using BotManager = OryxBot.Shared.Contracts.BotManager;

namespace OryxBot.Client.Linux.Services
{
    public class DefaultTradeMissionRouteProvider : TradeMissionRouteManager, HasDependencies
    {
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
            DefaultRoute();

        public void ToggleCustomMode() =>
            CustomRoutes = !CustomRoutes;

        private static string GetValidFileName(string fileName) {
            // remove any invalid character from the filename.
            String ret = Regex.Replace(fileName.Trim(), "[^A-Za-z0-9_. ]+", "");
            return ret.Replace(" ", String.Empty);
        }
        
        public StreamWriter SaveRouteStream() {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory+"/recordings");
                
            var path = AppDomain.CurrentDomain.BaseDirectory+"/recordings/"
                       + GetValidFileName(bot.RecordingConfig.Name)
                       + DateTime.Now.ToString("yyyyMMddHHmmssfff")
                       + ".csv";
            Console.WriteLine("Writing recording to "+path);
            return new StreamWriter(path);
        }

        public void SetDefaultRouteCity(City city) =>
            _defaultCity = city;

        public TradeMissionRoute? DefaultRoute() =>
            RouteFromResource("OryxBot.Client.Linux.Bot.Resources.route_{city}_trademission.csv") as TradeMissionRoute;

        public Route? RouteFromBankToQuest() =>
            RouteFromResource("OryxBot.Client.Linux.Bot.Resources.route_{city}_bank_to_quest.csv");

        public Route? RouteFromQuestToBank() =>
            RouteFromResource("OryxBot.Client.Linux.Bot.Resources.route_{city}_quest_to_bank.csv");

        protected Route? RouteFromResource(string resource) {
            try {
                resource = resource.Replace("{city}", DefaultRouteCityResourcePrefix());
                using var resourceStream =
                    Assembly.GetAssembly(GetType())!.GetManifestResourceStream(resource)!;

                var textStream = new StreamReader(resourceStream);
                return RouteFromStream(textStream.BaseStream);
            } catch (Exception exception) {
                FileLogger.Common.Info($"Route error for resource "+resource);
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
                    
                    var changeCluster = new ChangeClusterEventArgs(
                        fields[1], 
                        fields.Length >= 3 ? fields[2] : null);
                    steps.AddLast(TradeMissionRecord.ChangeClusterStep.From(changeCluster));
                    
                } else if (step == "quest") {
                    stepsBeforeQuest = steps;
                    steps = new Route();
                } else {
                    FileLogger.Common.Error($"Something went wrong parsing route recording, the step is "+step);
                }
            }
            
            return stepsBeforeQuest != null
                ? new TradeMissionRoute(stepsBeforeQuest, steps)
                : steps;
        }
    }
}
