using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Subtitle_Synchronizer
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void HelpForm_Load(object sender, EventArgs e)
        {
            richTextBox_HelpText.Text = usageHelp();
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            
        }

        private void button_Close_Click_1(object sender, EventArgs e)
        {
            
        }

        private void richTextBox_HelpText_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_ClosForm_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox_HelpText.Text = speechRecognitionHelp();
        }

        private string usageHelp()
        {
            string text = @" Fuck you!";
            return text;
        }
        private string speechRecognitionHelp()
        {
            string text = 
@"This program uses two different speech recognition engines for best results. It can also run on only one of the two engines, or on none, but timing accuracy will be affected.

Speech Recognition not working? Make sure you have done this:

For System.Speech recognition:
1 - Installed the language packs for the language you want to use:
	You can get them on http://windows.microsoft.com/pt-pt/windows/language-packs#lptabs=win8
	OR
	You can get them on Control Panel -> Languages -> Add a Language -> Press Options on the new language -> Download and Install Language Pack
		Only the following languages work with speech recognition:

    en-GB. English (United Kingdom)

    en-US. English (United States)

    de-DE. German (Germany)

    es-ES. Spanish (Spain)

    fr-FR. French (France)

    ja-JP. Japanese (Japan)

    zh-CN. Chinese (China)

    zh-TW. Chinese (Taiwan)

For  Microsoft.Speech recognition:
This engine supports more languages. To use this engine:

1 - (For developers?) Install the Speech Platform Runtime V11 (x86 and x64, just to be sure) from http://www.microsoft.com/en-us/download/details.aspx?id=27225

2 - (For developers?) Install the Speech Platform SDK (x86 and x64, just to be sure) from http://www.microsoft.com/en-us/download/details.aspx?id=27226

3 - Languages can be download from http://www.microsoft.com/en-us/download/details.aspx?id=27224. The following languages should have been deployed already with the program installation:

";

            return text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox_HelpText.Text = usageHelp();
        }
    }
}
