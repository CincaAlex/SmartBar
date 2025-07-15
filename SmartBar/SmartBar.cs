using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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


        private bool isAnApp = false;
        private bool isMath = false;
        private bool isFile = false;
        private bool _isCacheInitialized = false;

        private List<string> _cachedApps = new List<string>();


        public SmartBar()
        {
            InitializeComponent();
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_ALT, VK_S);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;

            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                Point newLocation = new Point(MousePosition.X / 2, MousePosition.Y + 15);
                this.Location = newLocation;
            }

            base.WndProc(ref m);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            base.OnFormClosed(e);
        }

        public static void SearchIndexedFiles(string keyword, ListBox list)
        {
            list.Items.Clear();
            string connectionString = "Provider=Search.CollatorDSO;Extended Properties='Application=Windows'";

            string safeKeyword = EscapeLikePattern(keyword.Replace("'", "''"));
            string pattern = $"%{safeKeyword}%";
            string query = $"SELECT System.ItemPathDisplay FROM SYSTEMINDEX " +
                           $"WHERE System.ItemNameDisplay LIKE '{pattern}'";

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string path = reader.GetString(0);
                            list.Items.Add(path);
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
            var apps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                            apps.Add(appName);
                        }
                    }
                }
                catch {   }
            }
            _cachedApps = apps.ToList();
        }

        private void searchBar_TextChanged(object sender, EventArgs e)
        {
            string text = searchBar.Text;
            isAnApp = false;
            isMath = false;
            isFile = false;

            if (!_isCacheInitialized)
            {
                CacheInstalledApps();
                _isCacheInitialized = true;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                listBox1.Items.Clear();
                return;
            }

            if (text.Contains("+") || text.Contains("-") ||
                text.Contains("*") || text.Contains("/"))
            {
                isMath = true;
                listBox1.Items.Clear();
            }
            else
            {
                listBox1.Items.Clear();
                var matches = new List<string>();

                foreach (string app in _cachedApps)
                {
                    if (app.ToLower().Contains(text.ToLower()))
                    {
                        matches.Add(app);
                    }
                }

                if (matches.Count > 0)
                {
                    isAnApp = true;
                    listBox1.Items.AddRange(matches.ToArray());
                }
            }

            if (!isAnApp && !isMath)
            {
                SearchIndexedFiles(text, listBox1);
                isFile = listBox1.Items.Count > 0;
            }
        }

        private void searchBar_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Tab)
            {
                //Chat GPT
            }

            if(!isMath && !isAnApp && !isFile && e.KeyCode == Keys.Enter)
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
    }
}
