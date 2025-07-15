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

        private void searchBar_TextChanged(object sender, EventArgs e)
        {
            string text = searchBar.Text;
            bool isAnApp = false;
            bool isMath = false;
            bool isFile = false;

            if (string.IsNullOrWhiteSpace(text))
            {
                listBox1.Items.Clear();
                return;
            }

            if (text.Contains("+") || text.Contains("-") ||
                text.Contains("*") || text.Contains("/"))
            {
                isMath = true;
            }
            else
            {
                string userStartMenuPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Start Menu\Programs");

                foreach (var file in new DirectoryInfo(userStartMenuPath).GetFiles())
                {
                    if (file.Name.Contains(text))
                    {
                        isAnApp = true;
                        break;
                    }
                }
            }

            if (!isAnApp && !isMath)
            {
                SearchIndexedFiles(text, listBox1);
                isFile = listBox1.Items.Count > 0;
            }

            if (!isFile && !isMath && !isAnApp)
            {
                // Your logic here
            }
        }


    }
}
