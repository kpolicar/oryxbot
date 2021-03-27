using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
using static OryxBot.Client.Windows.Native.User32;
using Region = OryxBot.Shared.Game.Region;

namespace OryxBot.Client.Windows
{
    public partial class ConfigureRecordingForm : Form
    {
        private readonly Dictionary<string, Region> _emissaryOptions = Npc.FactionEmissary.Position
            .ToDictionary(
                emissaryData => Regions.Name(emissaryData.Key),
                emissaryData => emissaryData.Key);

        private readonly Dictionary<string, City> _citiesOptions = Enum.GetValues<City>()
            .ToDictionary(Cities.Name, city => city);
        
        public City SelectedOrigin => (City) originComboBox.SelectedValue;
        public Region SelectedDestination => (Region) destinationComboBox.SelectedValue;
        public string SelectedName => nameTextBox.Text;
        
        
        public ConfigureRecordingForm() {
            InitializeComponent();
            InitializeIcons();
            
            originComboBox.DataSource = new BindingSource(_citiesOptions, null);
            originComboBox.DisplayMember = "Key";
            originComboBox.ValueMember = "Value";
            destinationComboBox.DataSource = new BindingSource(_emissaryOptions, null);
            destinationComboBox.DisplayMember = "Key";
            destinationComboBox.ValueMember = "Value";
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

