using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using System.Text.RegularExpressions;

namespace SmartBar
{
    public partial class SmartBar : Form
    {

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 1;
        private const uint MOD_ALT = 0x0001;  // Alt key
        private const uint VK_S = 0x53;       // 'S' key

        private const int WM_HOTKEY = 0x0312;

        private bool _isCacheInitialized = false;

        private List<Item> _cachedApps = new List<Item>();

        private readonly ChatGPT _chatGpt = new ChatGPT();
        private Calculator calculator = new Calculator();

        private bool isMath = false;
        private bool wasModified = false;
        private bool isVisible = true;

        public SmartBar()
        {
            InitializeComponent();
            bool registered = RegisterHotKey(this.Handle, HOTKEY_ID, MOD_ALT, VK_S);
            if (!registered)
            {
                MessageBox.Show("Hotkey could not be registered.");
            }
            listBox1.Visible = false;
            this.Size = new Size(this.Size.Width, 39);
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.DrawItem += listBox1_DrawItem;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                if (isVisible)
                {
                    this.Visible = true;
                    Point newLocation = new Point(MousePosition.X - 80, MousePosition.Y + 15);
                    this.Location = newLocation;
                    this.TopMost = true;
                    isVisible = !isVisible;
                }
                else
                {
                    this.TopMost = false;
                    this.Visible = false;
                    isVisible = !isVisible;
                }
            }

            base.WndProc(ref m);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            base.OnFormClosed(e);
        }

        private static void SearchIndexedFiles(string keyword, ListBox list)
        {
            string connectionString = "Provider=Search.CollatorDSO;Extended Properties='Application=Windows'";

            string safeKeyword = EscapeLikePattern(keyword.Replace("'", "''"));
            string pattern = $"%{safeKeyword}%";
            string query = $"SELECT System.ItemPathDisplay FROM SYSTEMINDEX " +
                           $"WHERE System.ItemNameDisplay LIKE '{pattern}'";

            try
            {
                HashSet<string> seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string path = reader.GetString(0);
                            if (seenPaths.Add(path))
                            {
                                list.Items.Add(new Item(Path.GetFileName(path), "file", path));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search failed: " + ex.Message);
            }
        }

        private static string EscapeLikePattern(string input)
        {
            return input.Replace("[", "[[]")
                        .Replace("%", "[%]")
                        .Replace("_", "[_]")
                        .Replace("]", "[]]");
        }

        private void CacheInstalledApps()
        {
            var apps = new HashSet<Item>();

            string[] paths = {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)))
    };

            foreach (string path in paths)
            {
                try
                {
                    string fullPath = Path.Combine(path, @"Microsoft\Windows\Start Menu\Programs");
                    if (!Directory.Exists(fullPath)) continue;

                    foreach (string file in Directory.EnumerateFiles(fullPath, "*.lnk", SearchOption.AllDirectories))
                    {
                        string appName = Path.GetFileNameWithoutExtension(file);
                        if (!string.IsNullOrWhiteSpace(appName))
                        {
                            apps.Add(new Item(appName, "app", file));
                        }
                    }
                }
                catch { }
            }
            _cachedApps = apps.ToList();
        }

        private void searchBar_TextChanged(object sender, EventArgs e)
        {
            string text = searchBar.Text;

            if (!_isCacheInitialized)
            {
                CacheInstalledApps();
                _isCacheInitialized = true;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                listBox1.Items.Clear();
                listBox1.Visible = false;
                this.Size = new Size(this.Size.Width, 39);
                wasModified = true;
                return;
            }
            else if (wasModified)
            {
                listBox1.Visible = true;
                this.Size = new Size(this.Size.Width, 139);
                wasModified = false;
            }


            if (
                (text.Contains("+") || text.Contains("-") ||
                 text.Contains("*") || text.Contains("/") ||
                 text.Contains("%") || text.Contains("^")))
            {
                listBox1.Items.Clear();
                isMath = true;
            }
            else
            {
                isMath = false;
                listBox1.Items.Clear();
                var matches = new List<Item>();

                foreach (Item app in _cachedApps)
                {
                    if (app.getName().ToLower().Contains(text.ToLower()))
                    {
                        matches.Add(app);
                    }
                }

                if (matches.Count > 0)
                {
                    listBox1.Items.AddRange(matches.ToArray());
                }
                SearchIndexedFiles(text, listBox1);
            }
        }

        private async void searchBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                listBox1.Items.Clear();
                if (searchBar.Text.ToLower() == "exit")
                {
                    System.Environment.Exit(0);
                }
                if (!isMath)
                {
                    listBox1.Items.Add("Loading...");

                    try
                    {
                        string response = await _chatGpt.AskChatGPT(searchBar.Text);
                        listBox1.Items.Clear();
                        listBox1.Items.Add(response);
                    }
                    catch (Exception ex)
                    {
                        listBox1.Items.Clear();
                        listBox1.Items.Add($"Error: {ex.Message}");
                    }
                }
                else
                {
                    listBox1.Items.Add(calculator.Compute(searchBar.Text));
                }
            }

            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;

                string query = searchBar.Text.Trim();

                if (!string.IsNullOrEmpty(query))
                {
                    string url = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";
                    try
                    {
                        System.Diagnostics.Process.Start(url);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error opening browser: " + ex.Message);
                    }
                }
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            var item = listBox1.Items[e.Index];

            string text = item.ToString();
            Brush brush = Brushes.Black;

            if (item is Item items)
            {
                if (items.getType() == "app")
                {
                    brush = Brushes.Blue;
                }
                if (items.getType() == "file")
                {
                    brush = Brushes.YellowGreen;
                }

                text = items.getName();
            }

            e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
            e.DrawFocusRectangle();
        }

        private static string ResolveShortcut(string shortcutPath)
        {
            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            return shortcut.TargetPath;
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            Item selectedItem = (Item)listBox1.SelectedItem;

            if (selectedItem.getType() == "app")
            {
                string appPath = selectedItem.getPath();
                string actualPath = ResolveShortcut(appPath);
                try
                {
                    System.Diagnostics.Process.Start(actualPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening app: " + ex.Message);
                }
            }
            else if (selectedItem.getType() == "file")
            {
                string filePath = selectedItem.getPath();
                try
                {
                    System.Diagnostics.Process.Start(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening file: " + ex.Message);
                }
            }
        }
    }
}
