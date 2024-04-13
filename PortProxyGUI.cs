using PortProxyGUI.Data;
using PortProxyGUI.UI;
using PortProxyGUI.Utils;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;
namespace PortProxyGUI {
    public partial class PortProxyGUI : Form {
        private readonly ListViewColumnSorter lvwColumnSorter = new ListViewColumnSorter();
        public SetProxy SetProxyForm;
        public About AboutForm;
        private AppConfig AppConfig;
        public PortProxyGUI() {
            InitializeComponent();
            Icon = Properties.Resources.icon;
            Font = new(new FontFamily("Microsoft Sans Serif"), 8f);
            listViewProxies.ListViewItemSorter = lvwColumnSorter;
        }
        private void PortProxyGUI_Load(object sender, EventArgs e) {
            AppConfig = Program.Database.GetAppConfig();
            var size = AppConfig.MainWindowSize;
            Left -= (size.Width - Width) / 2;
            Top -= (size.Height - Height) / 2;
            ResetWindowSize();
        }
        private void PortProxyGUI_Shown(object sender, EventArgs e) {
            RefreshProxyList();
        }

        private void ResetWindowSize()
        {
            Size = AppConfig.MainWindowSize;

            // Check if column count mismatch exists
            if (AppConfig.PortProxyColumnWidths.Length != listViewProxies.Columns.Count)
            {
                // Resize AppConfig.PortProxyColumnWidths to match listViewProxies.Columns.Count
                Array.Resize(ref AppConfig.PortProxyColumnWidths, listViewProxies.Columns.Count);
            }

            // Loop through columns and set widths
            for (int i = 0; i < AppConfig.PortProxyColumnWidths.Length; i++)
            {
                if (i < listViewProxies.Columns.Count)
                {
                    listViewProxies.Columns[i].Width = AppConfig.PortProxyColumnWidths[i];
                }
            }
        }

        private Data.Rule ParseRule(ListViewItem item) {
            var subItems = item.SubItems.OfType<ListViewSubItem>().ToArray();
            int listenPort, connectPort;
            listenPort = Data.Rule.ParsePort(subItems[3].Text);
            connectPort = Data.Rule.ParsePort(subItems[5].Text);
            var rule = new Data.Rule{
                Type = subItems[1].Text.Trim(),
                ListenOn = subItems[2].Text.Trim(),
                ListenPort = listenPort,
                ConnectTo = subItems[4].Text.Trim(),
                ConnectPort = connectPort,
                Comment = subItems[6].Text.Trim(),
                Group = item.Group?.Header.Trim(),
            };
            return rule;
        }
        private void EnableSelectedProxies() {
            var items = listViewProxies.SelectedItems.OfType<ListViewItem>();
            foreach (var item in items) {
                item.ImageIndex = 1;
                try { var rule = ParseRule(item); PortPorxyUtil.AddOrUpdateProxy(rule); } catch (NotSupportedException ex) {
                    MessageBox.Show(ex.Message, "Exclamation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            PortPorxyUtil.ParamChange();
        }
        private void DisableSelectedProxies() {
            var items = listViewProxies.SelectedItems.OfType<ListViewItem>();
            foreach (var item in items) {
                item.ImageIndex = 0;
                try { var rule = ParseRule(item); PortPorxyUtil.DeleteProxy(rule); } catch (NotSupportedException ex) {
                    MessageBox.Show(ex.Message, "Exclamation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            PortPorxyUtil.ParamChange();
        }
        private void DeleteSelectedProxies() {
            var items = listViewProxies.SelectedItems.OfType<ListViewItem>();
            DisableSelectedProxies();
            Program.Database.RemoveRange(items.Select(x => new Data.Rule { Id = x.Tag.ToString() }));
            foreach (var item in items) listViewProxies.Items.Remove(item);
        }
        private void SetProxyForUpdate(SetProxy form) {
            var item = listViewProxies.SelectedItems.OfType<ListViewItem>().FirstOrDefault();
            try { var rule = ParseRule(item); form.UseUpdateMode(item, rule); } catch (NotSupportedException ex) {
                MessageBox.Show(ex.Message, "Exclamation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }
        private void InitProxyGroups(Data.Rule[] rules) {
            listViewProxies.Groups.Clear();
            var groups = (
                from g in rules.GroupBy(x => x.Group)
                let name = g.Key
                where !IsNullOrWhiteSpace(name)
                orderby name
                select new ListViewGroup(name)
            ).ToArray();
            listViewProxies.Groups.AddRange(groups);
        }
        private void InitProxyItems(Data.Rule[] rules, Data.Rule[] proxies) {
            listViewProxies.Items.Clear();
            foreach (var rule in rules) {
                var imageIndex = proxies.Any(p => p.EqualsWithKeys(rule)) ? 1 : 0;
                var group = listViewProxies.Groups.OfType<ListViewGroup>().FirstOrDefault(x => x.Header == rule.Group);
                var item = new ListViewItem();
                UpdateListViewItem(item, rule, imageIndex);
                listViewProxies.Items.Add(item);
            }
        }
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            return value.Trim().Length == 0;
        }

        public void UpdateListViewItem(ListViewItem item, Data.Rule rule, int imageIndex) {
            item.ImageIndex = imageIndex;
            item.Tag = rule.Id;
            item.SubItems.Clear();
            item.SubItems.AddRange(new[]{
                new ListViewSubItem(item, rule.Type),
                new ListViewSubItem(item, rule.ListenOn),
                new ListViewSubItem(item, rule.ListenPort.ToString()) { Tag = "Number" },
                new ListViewSubItem(item, rule.ConnectTo),
                new ListViewSubItem(item, rule.ConnectPort.ToString ()) { Tag = "Number" },
                new ListViewSubItem(item, rule.Comment ?? ""),
            });
            if (IsNullOrWhiteSpace(rule.Group)) item.Group = null;
            else {
                var group = listViewProxies.Groups.OfType<ListViewGroup>().FirstOrDefault(x => x.Header == rule.Group);
                if (group == null) { group = new ListViewGroup(rule.Group); listViewProxies.Groups.Add(group); }
                item.Group = group;
            }
        }
        public void RefreshProxyList() {
            var proxies = PortPorxyUtil.GetProxies();
            var rules = Program.Database.Rules.ToArray();
            foreach (var proxy in proxies) {
                var matchedRule = rules.FirstOrDefault(r => r.EqualsWithKeys(proxy));
                proxy.Id = matchedRule?.Id;
            }
            var pendingAdds = proxies.Where(x => x.Valid && x.Id == null);
            var pendingUpdates =
                from proxy in proxies
                let exsist = rules.FirstOrDefault(r => r.Id == proxy.Id)
                where exsist is not null
                where proxy.Valid && proxy.Id is not null
                select proxy;
            Program.Database.AddRange(pendingAdds);
            Program.Database.UpdateRange(pendingUpdates);
            rules = Program.Database.Rules.ToArray();
            InitProxyGroups(rules);
            InitProxyItems(rules, proxies);
            toolStripStatusLabel_ServiceNotRunning.Visible = !PortPorxyUtil.IsServiceRunning();
        }
        private void contextMenuStrip_RightClick_MouseClick(object sender, MouseEventArgs e) {
            if (sender is ContextMenuStrip strip) {
                var selected = strip.Items.OfType<ToolStripMenuItem>().Where(x => x.Selected).FirstOrDefault();
                if (selected is null || !selected.Enabled) return;
                switch (selected) {
                    case ToolStripMenuItem item when item == toolStripMenuItem_Enable: EnableSelectedProxies(); break;
                    case ToolStripMenuItem item when item == toolStripMenuItem_Disable: DisableSelectedProxies(); break;
                    case ToolStripMenuItem item when item == toolStripMenuItem_New:
                        SetProxyForm ??= new SetProxy(this);
                        SetProxyForm.UseNormalMode();
                        SetProxyForm.ShowDialog();
                        break;
                    case ToolStripMenuItem item when item == toolStripMenuItem_Modify:
                        SetProxyForm ??= new SetProxy(this);
                        SetProxyForUpdate(SetProxyForm);
                        SetProxyForm.ShowDialog();
                        break;
                    case ToolStripMenuItem item when item == toolStripMenuItem_Refresh:
                        RefreshProxyList();
                        toolStripStatusLabel_Status.Text = $"{DateTime.Now} : Refreshed.";
                        break;
                    case ToolStripMenuItem item when item == toolStripMenuItem_FlushDnsCache:
                        var status = Native.NativeMethods.DnsFlushResolverCache();
                        if (status == 0) throw new InvalidOperationException("Flush DNS Cache failed.");
                        toolStripStatusLabel_Status.Text = $"{DateTime.Now} : DNS cache cleared.";
                        break;
                    case ToolStripMenuItem item when item == toolStripMenuItem_Delete:
                        DeleteSelectedProxies();
                        break;
                    case ToolStripMenuItem item when item == toolStripMenuItem_About:
                        if (AboutForm == null) { AboutForm = new About(this); AboutForm.Show(); }
                        else AboutForm.Show();
                        break;
                }
            }
        }
        private void listView1_MouseUp(object sender, MouseEventArgs e) {
            if (sender is ListView listView) {
                toolStripMenuItem_Enable.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Any(x => x.ImageIndex == 0);
                toolStripMenuItem_Disable.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Any(x => x.ImageIndex == 1);
                toolStripMenuItem_Delete.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Any();
                toolStripMenuItem_Modify.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;
            }
        }
        private void listView1_DoubleClick(object sender, EventArgs e) {
            if (sender is ListView listView) {
                var selectAny = listView.SelectedItems.OfType<ListViewItem>().Any();
                if (selectAny) {
                    SetProxyForm ??= new SetProxy(this);
                    SetProxyForUpdate(SetProxyForm);
                    SetProxyForm.ShowDialog();
                }
            }
        }
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e) {
            if (e.Column == lvwColumnSorter.SortColumn) {
                if (lvwColumnSorter.Order == SortOrder.Ascending) {
                    lvwColumnSorter.Order = SortOrder.Descending;
                } else {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            } else {
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }
            listViewProxies.Sort();
        }
        private void listViewProxies_KeyUp(object sender, KeyEventArgs e) {
            if (sender is ListView) {
                if (e.KeyCode == Keys.Delete) DeleteSelectedProxies();
            }
        }
        private void listViewProxies_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e) {
            if (AppConfig is not null && sender is ListView listView) {
                AppConfig.PortProxyColumnWidths[e.ColumnIndex] = listView.Columns[e.ColumnIndex].Width;
            }
        }
        private void PortProxyGUI_FormClosing(object sender, FormClosingEventArgs e) {
            Program.Database.SaveAppConfig(AppConfig);
        }
        private void PortProxyGUI_Resize(object sender, EventArgs e) {
            if (AppConfig is not null && sender is Form form) {
                AppConfig.MainWindowSize = form.Size;
            }
        }
        private void toolStripMenuItem_Export_Click(object sender, EventArgs e) {
            using var dialog = saveFileDialog_Export;
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK) {
                var fileName = dialog.FileName;
                try { File.Copy("appRules.csv", fileName, true); } catch { }
            }
        }
        private void toolStripMenuItem_Import_Click(object sender, EventArgs e) {
            using var dialog = openFileDialog_Import;
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK) {
                var fileName = dialog.FileName;
                MessageBox.Show("Importing is not available yet. You selected: " + fileName);
                RefreshProxyList();
            }
        }
        private void toolStripMenuItem_ResetWindowSize_Click(object sender, EventArgs e) {
            AppConfig = new AppConfig();
            ResetWindowSize();
        }
        private void toolStripStatusLabel_ServiceNotRunning_Click(object sender, EventArgs e) {
            PortPorxyUtil.StartService();
            toolStripStatusLabel_ServiceNotRunning.Visible = false;
        }
    }
}
