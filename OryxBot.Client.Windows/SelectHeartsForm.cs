using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OryxBot.Shared;
using OryxBot.Shared.Game;
using static OryxBot.Client.Windows.Native.User32;

namespace OryxBot.Client.Windows
{
    public partial class SelectHeartsForm : Form
    {
        private readonly Dictionary<string, RunConfiguration.ContractType> _heartOptions = Enum.GetValues<RunConfiguration.ContractType>()
            .ToDictionary(type => ((int)type).ToString(), type => type);
        
        public RunConfiguration.ContractType SelectedContractType => (RunConfiguration.ContractType) heartSelectorComboBox.SelectedValue;
        
        
        public SelectHeartsForm() {
            InitializeComponent();
            InitializeIcons();
            heartSelectorComboBox.DataSource = new BindingSource(_heartOptions, null);
            heartSelectorComboBox.DisplayMember = "Key";
            heartSelectorComboBox.ValueMember = "Value";
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

