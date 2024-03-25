using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static NitroxModel.DisplayStatusCodes;
public class CustomMessageBox : Form
{
    
    Label message = new();
    Button helpBtn = new();
    Button closeBtn = new();

    public CustomMessageBox(StatusCode statusCode, string exception)
    {
            ClientSize = new Size(690, 250);
            Text = "Nitrox has encountered an error!";

            helpBtn.Location = new Point(611, 212);
            helpBtn.Size = new Size(75, 23);
            helpBtn.Text = "Help";
            helpBtn.BackColor = Control.DefaultBackColor;
            helpBtn.Click += HelpButtonOnClick;

            closeBtn.Location = new Point(511, 212);
            closeBtn.Size = new Size(75, 23);
            closeBtn.Text = "Close";
            closeBtn.BackColor = Control.DefaultBackColor;
            closeBtn.Click += CloseButtonOnClick;
            message.Location = new Point(10, 10);
        // Need to add function to wrap text to fit inside messageBox
            message.Text = "Nitrox has run into an error with the status code " + statusCode.ToString("D") + "! " + Environment.NewLine +
                "Look up this status code on the nitrox website(https://www.nitrox.rux.gg/) using the help button below for more information. " + Environment.NewLine +
                "Nitrox may still be running and it is possible that Nitrox has recovered from this error. " + Environment.NewLine + Environment.NewLine + WrapText(exception);
            message.Font = Control.DefaultFont;
            message.AutoSize = true;

            BackColor = Color.White;
            ShowIcon = false;
            Controls.Add(helpBtn);
            Controls.Add(closeBtn);
            Controls.Add(message);
    }
    private string WrapText(string text)
    {
        int lastSpace = 0;
        int lastBreak = 0;
        string textAfterBreak = "";
        // Iterate over each character in the string
        for(int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ')
            {
                lastSpace = i;
            }
            if((i - lastBreak) > 140)
            {
                 textAfterBreak = text.Substring(lastSpace);
                 text = text.Replace(textAfterBreak, "");
                 text = string.Concat(text, "\r\n" + textAfterBreak);
                lastBreak = lastSpace;
            }
        }
        return text;
    }
    private void HelpButtonOnClick(object sender, EventArgs e)
    {
        Process.Start("https://nitrox.rux.gg/");
    }
    private void HelpButtonOnClickPirate(object sender, EventArgs e)
    {
        Process.Start("https://discord.com/invite/subnautica-nitrox-official-525437013403631617");
    }
    private void CloseButtonOnClick(object sender, EventArgs e)
    {
        Close();
    }
}
