using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using static NitroxModel.DisplayStatusCodes;
public class CustomMessageBox : Form
{
    
    Label message = new();
    Button helpBtn = new();
    Button closeBtn = new();

    public CustomMessageBox(StatusCode statusCode, bool isPirate)
    {
        if (!isPirate)
        {
            ClientSize = new Size(690, 150);
            Text = "Nitrox has encountered an error!";

            helpBtn.Location = new Point(611, 112);
            helpBtn.Size = new Size(75, 23);
            helpBtn.Text = "Help";
            helpBtn.BackColor = Control.DefaultBackColor;
            helpBtn.Click += HelpButtonOnClick;

            closeBtn.Location = new Point(511, 112);
            closeBtn.Size = new Size(75, 23);
            closeBtn.Text = "Close";
            closeBtn.BackColor = Control.DefaultBackColor;
            closeBtn.Click += CloseButtonOnClick;

            message.Location = new Point(10, 10);
            message.Text = "Nitrox has run into an error with the status code " + statusCode.ToString("D") + "! " + Environment.NewLine +
                "Look up this status code on the nitrox website(https://www.nitrox.rux.gg/) using the help button below for more information. " + Environment.NewLine +
                "Nitrox may still be running and it is possible that Nitrox has recovered from this error. ";
            message.Font = Control.DefaultFont;
            message.AutoSize = true;

            BackColor = Color.White;
            ShowIcon = false;
            Controls.Add(helpBtn);
            Controls.Add(closeBtn);
            Controls.Add(message);
        }
        else
        {
            // Get the pirate to report themselves in the discord to be banned
            ClientSize = new System.Drawing.Size(900, 150);
            Text = "Nitrox has encountered an error!";

            helpBtn.Location = new Point(0, 112);
            helpBtn.Size = new Size(125, 23);
            helpBtn.Text = "Join discord";
            helpBtn.BackColor = Control.DefaultBackColor;
            helpBtn.Click += HelpButtonOnClickPirate;

            message.Location = new Point(10, 10);
            message.Text = "Nitrox has run into an error with the status code " + statusCode.ToString("D") + "! " + Environment.NewLine +
                "Please ask for support in the nitrox discord using the join button. A member of the support team would be glad to assist you if you provide them with this status code.";
            message.Font = Control.DefaultFont;
            message.AutoSize = true;

            BackColor = Color.White;
            ShowIcon = false;
            Controls.Add(helpBtn);
            Controls.Add(message);
        }
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
