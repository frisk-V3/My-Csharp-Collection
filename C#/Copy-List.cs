using System;
using System.IO;
using System.Windows.Forms;

public class ClipHistory : Form
{
    ListBox list = new ListBox();
    Timer timer = new Timer();
    string lastText = "";
    string saveFile = "history.txt";

    public ClipHistory()
    {
        this.Text = "Clipboard History";
        this.Width = 400;
        this.Height = 600;

        list.SetBounds(10, 10, 360, 540);
        list.DoubleClick += OnItemDoubleClick;
        this.Controls.Add(list);

        LoadHistory();

        timer.Interval = 500; // 0.5秒ごとに監視
        timer.Tick += CheckClipboard;
        timer.Start();

        this.FormClosing += OnClose;
    }

    void CheckClipboard(object sender, EventArgs e)
    {
        try
        {
            string text = Clipboard.GetText();

            if (!string.IsNullOrEmpty(text) && text != lastText)
            {
                lastText = text;
                list.Items.Insert(0, text); // 新しいものを上に追加
            }
        }
        catch { }
    }

    void OnItemDoubleClick(object sender, EventArgs e)
    {
        if (list.SelectedItem != null)
        {
            Clipboard.SetText(list.SelectedItem.ToString());
            MessageBox.Show("コピーしました");
        }
    }

    void LoadHistory()
    {
        if (File.Exists(saveFile))
        {
            string[] lines = File.ReadAllLines(saveFile);
            foreach (string s in lines)
                list.Items.Add(s);
        }
    }

    void OnClose(object sender, FormClosingEventArgs e)
    {
        File.WriteAllLines(saveFile, GetItems());
    }

    string[] GetItems()
    {
        string[] arr = new string[list.Items.Count];
        for (int i = 0; i < list.Items.Count; i++)
            arr[i] = list.Items[i].ToString();
        return arr;
    }

    [STAThread]
    public static void Main()
    {
        Application.Run(new ClipHistory());
    }
}