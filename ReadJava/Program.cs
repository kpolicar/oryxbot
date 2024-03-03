using System.Security.AccessControl;

namespace ReadJava;

internal class Program
{
    private static void Main() {
        var a = new Program();
        a.CreateFileWatcher("C:\\Users\\Klemen\\AppData\\Local\\Temp\\oryxbot\\memgraphics");
        Console.WriteLine("Watching file");
        while (true) {
            SendCommand();
        }
    }

    public static void SendCommand() {
        var key = Console.ReadLine().FirstOrDefault();
        //var response = new HttpClient().GetAsync($"http://localhost:8010/keyevent?key={key}").Result;
        var response1 = new HttpClient().GetAsync($"http://localhost:8010/mouse/move?x=1000&y=900").Result;
        var response2 = new HttpClient().GetAsync($"http://localhost:8010/mouse/click?x=1000&y=900&mouse=right").Result;
    }

    public void CreateFileWatcher(string path) {
        // Create a new FileSystemWatcher and set its properties.
        var watcher = new FileSystemWatcher();
        watcher.Path = path;
        /* Watch for changes in LastAccess and LastWrite times, and
           the renaming of files or directories. */
        // watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
        //                                                 | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        // Only watch text files.
        watcher.Filter = "*.png";

        // Add event handlers.
        watcher.Changed += OnChanged;
        watcher.Created += OnChanged;
        watcher.Deleted += OnChanged;
        watcher.Renamed += OnRenamed;

        // Begin watching.
        watcher.EnableRaisingEvents = true;
    }

    // Define the event handlers.
    private static void OnChanged(object source, FileSystemEventArgs e) {
        // Specify what is done when a file is changed, created, or deleted.
        //Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
    }

    private static void OnRenamed(object source, RenamedEventArgs e) {
        // Specify what is done when a file is renamed.
        //Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
    }
}
