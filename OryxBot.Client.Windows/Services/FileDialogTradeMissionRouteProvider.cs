using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using OryxBot.Bot.Contracts;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Windows.Services
{
    public class FileDialogTradeMissionRouteProvider : TradeMissionRouteProvider
    {
        private UIApplicationContext App = null!;
        
        public LinkedList<Position>? Route() {
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

            var waypoints = new LinkedList<Position>();
            while (!parser.EndOfData) {
                var fields = parser.ReadFields();
                
                var (x, y) = (float.Parse(fields[0]), float.Parse(fields[1]));
                var waypoint = new Position(x, y);
                
                waypoints.AddLast(waypoint);
            }

            return waypoints;
        }

        public void BindToApp(UIApplicationContext app) =>
            App = app;
    }
}
