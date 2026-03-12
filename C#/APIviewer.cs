using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

public class ApiTesterForm : Form
{
    TextBox urlBox;
    TextBox resultBox;
    Button sendButton;

    public ApiTesterForm()
    {
        this.Text = "Winfoam API Tester (C#5)";
        this.Width = 600;
        this.Height = 400;

        urlBox = new TextBox();
        urlBox.Top = 10;
        urlBox.Left = 10;
        urlBox.Width = 560;
        urlBox.Text = "https://api.example.com/v1/endpoint";
        this.Controls.Add(urlBox);

        sendButton = new Button();
        sendButton.Top = 40;
        sendButton.Left = 10;
        sendButton.Width = 100;
        sendButton.Text = "Send";
        sendButton.Click += new EventHandler(OnSendClick);
        this.Controls.Add(sendButton);

        resultBox = new TextBox();
        resultBox.Top = 80;
        resultBox.Left = 10;
        resultBox.Width = 560;
        resultBox.Height = 260;
        resultBox.Multiline = true;
        resultBox.ScrollBars = ScrollBars.Vertical;
        this.Controls.Add(resultBox);
    }

    void OnSendClick(object sender, EventArgs e)
    {
        string url = urlBox.Text;

        try
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Accept = "application/json";

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    string body = reader.ReadToEnd();
                    resultBox.Text = "Status: " + (int)res.StatusCode + "\r\n\r\n" + body;
                }
            }
        }
        catch (WebException ex)
        {
            resultBox.Text = "Error: " + ex.Message;

            if (ex.Response != null)
            {
                using (var err = new StreamReader(ex.Response.GetResponseStream()))
                {
                    resultBox.Text += "\r\n\r\n" + err.ReadToEnd();
                }
            }
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new ApiTesterForm());
    }
}