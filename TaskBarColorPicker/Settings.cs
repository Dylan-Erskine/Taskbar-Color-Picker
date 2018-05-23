using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskBarColorPicker
{
    public partial class Settings : Form
    {
        Microsoft.Win32.RegistryKey autorun;

        public Settings()
        {
            InitializeComponent();
            autorun = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = (autorun.GetValue("TaskplayColorPicker") != null);
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                autorun.SetValue("TaskplayColorPicker", Application.ExecutablePath);
            else
                autorun.DeleteValue("TaskplayColorPicker", false);

        }
    }
}
