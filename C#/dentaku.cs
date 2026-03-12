using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

public class Calculator : Form
{
    TextBox display;

    public Calculator()
    {
        Text = "Calculator";
        ClientSize = new Size(260, 360);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        display = new TextBox();
        display.ReadOnly = true;
        display.Text = "";
        display.Font = new Font("Arial", 24);
        display.TextAlign = HorizontalAlignment.Right;
        display.Location = new Point(10, 10);
        display.Size = new Size(230, 40);
        Controls.Add(display);

        string[] buttons =
        {
            "C", ".", "(", ")",
            "7","8","9","÷",
            "4","5","6","×",
            "1","2","3","-",
            "0","+","=",""
        };

        int index = 0;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (index >= buttons.Length) break;

                string label = buttons[index++];
                if (label == "") continue;

                Button b = new Button();
                b.Text = label;
                b.Font = new Font("Arial", 18);
                b.Size = new Size(55, 55);
                b.Location = new Point(10 + x * 60, 60 + y * 60);
                b.Click += OnClick;
                Controls.Add(b);
            }
        }
    }

    void OnClick(object sender, EventArgs e)
    {
        Button b = (Button)sender;
        string t = b.Text;

        if (t == "C")
        {
            display.Text = "";
            return;
        }

        if (t == "=")
        {
            Calculate();
            return;
        }

        if (t == "×") t = "*";
        if (t == "÷") t = "/";

        display.Text += t;
    }

    void Calculate()
    {
        try
        {
            string expr = display.Text;

            DataTable dt = new DataTable();
            var result = dt.Compute(expr, "");

            display.Text = result.ToString();
        }
        catch
        {
            MessageBox.Show("計算できません");
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.Run(new Calculator());
    }
}