using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace PortProxyGUI.Data
{
    public class MigrationUtil
    {
        public ApplicationDbScope DbScope { get; private set; }

        public MigrationUtil(ApplicationDbScope context)
        {
            DbScope = context;
            EnsureHistoryTable();
        }

        public void EnsureHistoryTable()
        {
        }


        public Dictionary<MigrationKey, string[]> History = new Dictionary<MigrationKey, string[]>
        {
            [new MigrationKey { MigrationId = "202103021542", ProductVersion = "1.1.0" }] = new[]
            {
                @"CREATE TABLE rules
(
    Id text PRIMARY KEY,
    Type text,
    ListenOn text,
    ListenPort integer,
    ConnectTo text,
    ConnectPort integer
);",
                "CREATE UNIQUE INDEX IX_Rules_Type_ListenOn_ListenPort ON Rules(Type, ListenOn, ListenPort);",
            },

            [new MigrationKey { MigrationId = "202201172103", ProductVersion = "1.2.0" }] = new[]
            {
                "ALTER TABLE rules ADD Note text;",
                "ALTER TABLE rules ADD `Group` text;",
            },

            [new MigrationKey { MigrationId = "202202221635", ProductVersion = "1.3.0" }] = new[]
            {
                "ALTER TABLE rules RENAME TO rulesOld;",
                "DROP INDEX IX_Rules_Type_ListenOn_ListenPort;",

                @"CREATE TABLE rules (
	Id text PRIMARY KEY,
	Type text,
	ListenOn text,
	ListenPort integer,
	ConnectTo text,
	ConnectPort integer,
	Comment text,
	`Group` text 
);",
                "CREATE UNIQUE INDEX IX_Rules_Type_ListenOn_ListenPort ON Rules ( Type, ListenOn, ListenPort );",

                "INSERT INTO rules SELECT Id, Type, ListenOn, ListenPort, ConnectTo, ConnectPort, Note, `Group` FROM rulesOld;",
                "DROP TABLE rulesOld;",
            },

            [new MigrationKey { MigrationId = "202303092024", ProductVersion = "1.4.0" }] = new[]
            {
                @"", "", "", "", "",
            },
        };
    }
}
