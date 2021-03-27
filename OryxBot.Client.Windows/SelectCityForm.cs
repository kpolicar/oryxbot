using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OryxBot.Shared.Game;
using static OryxBot.Client.Windows.Native.User32;

namespace OryxBot.Client.Windows
{
    public partial class SelectCityForm : Form
    {
        private readonly Dictionary<string, City> _citiesOptions = Enum.GetValues<City>()
            .ToDictionary(Cities.Name, city => city);
        
        public City SelectedCity => (City) citySelectorComboBox.SelectedValue;
        
        
        public SelectCityForm() {
            InitializeComponent();
            InitializeIcons();
            citySelectorComboBox.DataSource = new BindingSource(_citiesOptions, null);
            citySelectorComboBox.DisplayMember = "Key";
            citySelectorComboBox.ValueMember = "Value";
        }
        
        private void InitializeIcons() {
            // var titlebarIcon = (Icon) resources.GetObject("$this.Icon")!;
            // var taskbarIcon = (Icon) resources.GetObject("$this.IconTaskbar")!;
            // SendMessage(Handle, WM_SETICON, ICON_SMALL, titlebarIcon.Handle);
            // SendMessage(Handle, WM_SETICON, ICON_BIG, taskbarIcon.Handle);
        }

        private void confirmButton_Click(object sender, EventArgs e) =>
            DialogResult = DialogResult.OK;

        private void cancelButton_Click(object sender, EventArgs e) =>
            DialogResult = DialogResult.Cancel;
    }
}

