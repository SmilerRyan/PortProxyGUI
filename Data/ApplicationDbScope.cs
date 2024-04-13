using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace PortProxyGUI.Data {
    public class Rule : IEquatable<Rule> {
        public string Id { get; set; }
        public string Type { get; set; }
        public string ListenOn { get; set; }
        public int ListenPort { get; set; }
        public string ConnectTo { get; set; }
        public int ConnectPort { get; set; }
        public string Comment { get; set; }
        public string Group { get; set; }
        public bool Valid => ListenPort > 0 && ConnectPort > 0;
        private string _realListenPort;
        public string RealListenPort {
            get => ListenPort > 0 ? ListenPort.ToString() : _realListenPort;
            set => _realListenPort = value;
        }
        private string _realConnectPort;
        public string RealConnectPort {
            get => ConnectPort > 0 ? ConnectPort.ToString() : _realConnectPort;
            set => _realConnectPort = value;
        }
        public bool Equals(Rule other) {
            return Id == other.Id
                && Type == other.Type
                && ListenOn == other.ListenOn
                && ListenPort == other.ListenPort
                && ConnectTo == other.ConnectTo
                && ConnectPort == other.ConnectPort
                && Comment == other.Comment
                && Group == other.Group;
        }
        public bool EqualsWithKeys(Rule other) {
            return Type == other.Type && ListenOn == other.ListenOn && ListenPort == other.ListenPort;
        }
        public static int ParsePort(string portString) {
            if (int.TryParse(portString, out var port) && 0 < port && port < 65536) return port;
            else throw new NotSupportedException($"Invalid port string. ({portString})");
        }
        public override bool Equals(object obj) {
            return Equals(obj as Rule);
        }
    }
    public class Config {
        public string Item { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class AppConfig {
        public Size MainWindowSize = new(720, 500);
        public int[] PortProxyColumnWidths = new int[] { 24, 64, 140, 100, 140, 100, 100 };
        private readonly Regex _intArrayRegex = new(@"^\[\s*(\d+)(?:\s*,\s*(\d+))*\s*\]$");
        public AppConfig() { }
        public AppConfig(Config[] rows) {
            {
                var item = rows.Where(x => x.Item == "MainWindow");
                if (int.TryParse(item.FirstOrDefault(x => x.Key == "Width")?.Value, out var width) && int.TryParse(item.FirstOrDefault(x => x.Key == "Height")?.Value, out var height)) {
                    MainWindowSize = new Size(width, height);
                } else MainWindowSize = new Size(720, 500);
            }
            {
                var item = rows.Where(x => x.Item == "PortProxy");
                var s_ColumnWidths = item.FirstOrDefault(x => x.Key == "ColumnWidths").Value;
                var match = _intArrayRegex.Match(s_ColumnWidths);
                if (match.Success) {
                    PortProxyColumnWidths = match.Groups.OfType<Group>().Skip(1).SelectMany(x => x.Captures.OfType<Capture>()).Select(x => int.Parse(x.Value)).ToArray();
                } else {
                    PortProxyColumnWidths = new int[0];
                }
            }
        }
    }
    public class ApplicationDbScope {
        public static readonly string AppDbDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PortProxyGUI");
        public static readonly string AppDbFile = Path.Combine(AppDbDirectory, "config.db");
        public static ApplicationDbScope FromFile(string file) {
            var dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var scope = new ApplicationDbScope($"Data Source=\"{file}\"");
            return scope;
        }
        public ApplicationDbScope(string connectionString) : base() {}
        public void Initialize() {}
        public IEnumerable<Rule> Rules {
            get {
                if (!File.Exists("appRules.csv")) {
                    using (StreamWriter writer = File.CreateText("appRules.csv")) {
                        writer.WriteLine("Id,Type,ListenOn,ListenPort,ConnectTo,ConnectPort,Comment,Group");
                    }
                }
                var lines = File.ReadAllLines("appRules.csv");
                var headers = lines.First().Split(',');
                var data = lines.Skip(1);
                return data.Select(line => {
                    var values = line.Split(',');
                    return new Rule
                    {
                        Id = values[Array.IndexOf(headers, "Id")],
                        Type = values[Array.IndexOf(headers, "Type")],
                        ListenOn = values[Array.IndexOf(headers, "ListenOn")],
                        ListenPort = int.Parse(values[Array.IndexOf(headers, "ListenPort")]),
                        ConnectTo = values[Array.IndexOf(headers, "ConnectTo")],
                        ConnectPort = int.Parse(values[Array.IndexOf(headers, "ConnectPort")]),
                        Comment = values[Array.IndexOf(headers, "Comment")],
                        Group = values[Array.IndexOf(headers, "Group")],
                    };
                });
            }
        }
        public Rule GetRule(string type, string listenOn, int listenPort) {
            var allRules = Rules.ToList();
            return allRules.FirstOrDefault(rule => rule.Type == type && rule.ListenOn == listenOn && rule.ListenPort == listenPort);
        }
        public void Add<T>(T obj) where T : class {
            var newid = Guid.NewGuid().ToString();
            if (obj is Rule rule) {
                rule.Id = newid;
                using (var writer = File.AppendText("appRules.csv")) {
                    writer.WriteLine($"{newid},{rule.Type},{rule.ListenOn},{rule.ListenPort},{rule.ConnectTo},{rule.ConnectPort},{rule.Comment ?? ""},{rule.Group ?? ""}");
                }
            } else throw new NotSupportedException($"Adding {obj.GetType().FullName} is not supported.");
        }
        public void AddRange<T>(IEnumerable<T> objs) where T : class {
            foreach (var obj in objs) Add(obj);
        }
        public void Update<T>(T obj) where T : class {
            if (obj is Rule rule) {
                var allLines = File.ReadAllLines("appRules.csv").ToList();
                var targetIndex = allLines.FindIndex(line => line.Split(',')[0] == rule.Id.ToString());
                if (targetIndex != -1) {
                    var updatedLine = $"{rule.Id},{rule.Type},{rule.ListenOn},{rule.ListenPort},{rule.ConnectTo},{rule.ConnectPort},{rule.Comment ?? ""},{rule.Group ?? ""}";
                    allLines[targetIndex] = updatedLine;
                } else {
                    Console.WriteLine($"Rule with Id '{rule.Id}' not found in the CSV file for update.");
                }
            } else {
                throw new NotSupportedException($"Updating {obj.GetType().FullName} is not supported.");
            }
        }
        public void UpdateRange<T>(IEnumerable<T> objs) where T : class {
            foreach (var obj in objs) Update(obj);
        }
        public void Remove<T>(T obj) where T : class {
            if (obj is Rule rule) {
                var allLines = File.ReadAllLines("appRules.csv").ToList();
                var targetIndex = allLines.FindIndex(line => { return line.Split(',')[0] == rule.Id.ToString(); });
                if (targetIndex != -1) { allLines.RemoveAt(targetIndex); File.WriteAllLines("appRules.csv", allLines.ToArray()); }
            } else throw new NotSupportedException($"Removing {obj.GetType().FullName} is not supported.");
        }
        public void RemoveRange<T>(IEnumerable<T> objs) where T : class {
            foreach (var obj in objs) Remove(obj);
        }
        public AppConfig GetAppConfig() {
            var appConfig = new AppConfig();
            try {
                string[] lines = File.ReadAllLines("AppConfig.ini");
                foreach (string line in lines) {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2) {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        switch (key) {
                            case "MW_Width":
                                appConfig.MainWindowSize.Width = int.Parse(value);
                                break;
                            case "MW_Height":
                                appConfig.MainWindowSize.Height = int.Parse(value);
                                break;
                            case "PP_ColumnWidths":
                                List<int> columnWidths = value
                                    .Replace("[", "")
                                    .Replace("]", "")
                                    .Split(',')
                                    .Select(int.Parse)
                                    .ToList();
                                appConfig.PortProxyColumnWidths = columnWidths.ToArray();
                                break;
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"An error occurred while reading AppConfig.ini: {ex.Message}");
            }
            return appConfig;
        }
        public void SaveAppConfig(AppConfig appConfig)
        {
            var s_portProxyColumnWidths = $"[{string.Join(", ", appConfig.PortProxyColumnWidths.Select(x => x.ToString()).ToList().ToArray())}]";
            File.WriteAllText("AppConfig.ini", $"MW_Width={appConfig.MainWindowSize.Width}\nMW_Height={appConfig.MainWindowSize.Height}\nPP_ColumnWidths={s_portProxyColumnWidths}\n");
        }
    }
}
