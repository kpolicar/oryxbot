using System.Windows.Forms;

namespace OryxBot.Client.Windows.Services
{
    public class Win32Input
    {
        private Control? relativeToControl;


        public void BindToControl(Control formGameWindowPanel) =>
            relativeToControl = formGameWindowPanel;
    }
}
