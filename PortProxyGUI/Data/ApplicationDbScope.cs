﻿using NStandard;
using SQLib.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PortProxyGUI.Data
{
    public class ApplicationDbScope : SqliteScope<ApplicationDbScope>
    {
        public static readonly string AppDbDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PortProxyGUI");
        public static readonly string AppDbFile = Path.Combine(AppDbDirectory, "config.db");

        public static ApplicationDbScope FromFile(string file)
        {
            var dir = Path.GetDirectoryName(file);

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(file))
            {
#if NETCOREAPP3_0_OR_GREATER
#else
                System.Data.SQLite.SQLiteConnection.CreateFile(file);
#endif
            }

            var scope = new ApplicationDbScope($"Data Source=\"{file}\"");
            scope.Migrate();
            return scope;
        }

        public ApplicationDbScope(string connectionString) : base(connectionString)
        {
        }

        public override void Initialize()
        {
        }

        public void Migrate() => new MigrationUtil(this).MigrateToLast();

        public Migration GetLastMigration()
        {
            return SqlQuery<Migration>($"SELECT * FROM __history ORDER BY MigrationId DESC LIMIT 1;").First();
        }

        public IEnumerable<Rule> Rules
{
  get
  {
    // Replace "path/to/your/rules.csv" with the actual path to your CSV file
    var lines = File.ReadAllLines("appRules.csv");

    // Assuming the first line contains column headers
    var headers = lines.First().Split(',');

    // Skip the header row for data processing
    var data = lines.Skip(1);

    return data.Select(line =>
    {
      var values = line.Split(',');
      // Map CSV values to Rule properties (assuming matching order)
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

public Rule GetRule(string type, string listenOn, int listenPort)
{
  // Load rules using the Rules property getter
  var allRules = Rules.ToList(); // Load all rules into memory for searching

  // Search for the rule based on your criteria
  return allRules.FirstOrDefault(rule => rule.Type == type && rule.ListenOn == listenOn && rule.ListenPort == listenPort);
}



        public void Add<T>(T obj) where T : class
        {
            var newid = Guid.NewGuid().ToString();
            if (obj is Rule rule)
            {
                Sql($"INSERT INTO Rules (Id, Type, ListenOn, ListenPort, ConnectTo, ConnectPort, Comment, `Group`) VALUES ({newid}, {rule.Type}, {rule.ListenOn}, {rule.ListenPort}, {rule.ConnectTo}, {rule.ConnectPort}, {rule.Comment ?? ""}, {rule.Group ?? ""});");
                rule.Id = newid;
            }
            else throw new NotSupportedException($"Adding {obj.GetType().FullName} is not supported.");
        }
        public void AddRange<T>(IEnumerable<T> objs) where T : class
        {
            foreach (var obj in objs) Add(obj);
        }

        public void Update<T>(T obj) where T : class
        {
            if (obj is Rule rule)
            {
                Sql($"UPDATE Rules SET Type={rule.Type}, ListenOn={rule.ListenOn}, ListenPort={rule.ListenPort}, ConnectTo={rule.ConnectTo}, ConnectPort={rule.ConnectPort} WHERE Id={rule.Id};");
            }
            else throw new NotSupportedException($"Updating {obj.GetType().FullName} is not supported.");
        }
        public void UpdateRange<T>(IEnumerable<T> objs) where T : class
        {
            foreach (var obj in objs) Update(obj);
        }

        public void Remove<T>(T obj) where T : class
        {
            if (obj is Rule rule)
            {
                Sql($"DELETE FROM Rules WHERE Id={rule.Id};");
            }
            else throw new NotSupportedException($"Removing {obj.GetType().FullName} is not supported.");
        }
        public void RemoveRange<T>(IEnumerable<T> objs) where T : class
        {
            foreach (var obj in objs) Remove(obj);
        }

        public AppConfig GetAppConfig()
        {
            var appConfig = new AppConfig();
            try
            {
                string[] lines = File.ReadAllLines("AppConfig.ini");
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        switch (key)
                        {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading AppConfig.ini: {ex.Message}");
            }
            return appConfig;
        }

        public void SaveAppConfig(AppConfig appConfig)
        {
            var s_portProxyColumnWidths = $"[{appConfig.PortProxyColumnWidths.Select(x => x.ToString()).Join(", ")}]";
            File.WriteAllText("AppConfig.ini", $"MW_Width={appConfig.MainWindowSize.Width}\nMW_Height={appConfig.MainWindowSize.Height}\nPP_ColumnWidths={s_portProxyColumnWidths}\n");
        }

    }
}
