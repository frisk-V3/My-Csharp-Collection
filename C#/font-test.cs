using System;
using System.Drawing;
using System.Windows.Forms;

public class FontTester : Form
{
    ListBox fontList;
    TextBox inputBox;
    Label preview;
    NumericUpDown sizeBox;
    CheckBox boldBox;
    CheckBox italicBox;

    public FontTester()
    {
        Text = "Font Tester";
        Width = 800;
        Height = 500;

        fontList = new ListBox();
        fontList.Location = new Point(10, 10);
        fontList.Size = new Size(250, 430);
        fontList.SelectedIndexChanged += UpdatePreview;
        Controls.Add(fontList);

        foreach (FontFamily f in FontFamily.Families)
            fontList.Items.Add(f.Name);

        inputBox = new TextBox();
        inputBox.Text = "あいうえお ABC 123";
        inputBox.Font = new Font("Arial", 14);
        inputBox.Location = new Point(270, 10);
        inputBox.Size = new Size(500, 30);
        inputBox.TextChanged += UpdatePreview;
        Controls.Add(inputBox);

        sizeBox = new NumericUpDown();
        sizeBox.Minimum = 8;
        sizeBox.Maximum = 200;
        sizeBox.Value = 32;
        sizeBox.Location = new Point(270, 50);
        sizeBox.ValueChanged += UpdatePreview;
        Controls.Add(sizeBox);

        boldBox = new CheckBox();
        boldBox.Text = "Bold";
        boldBox.Location = new Point(350, 50);
        boldBox.CheckedChanged += UpdatePreview;
        Controls.Add(boldBox);

        italicBox = new CheckBox();
        italicBox.Text = "Italic";
        italicBox.Location = new Point(420, 50);
        italicBox.CheckedChanged += UpdatePreview;
        Controls.Add(italicBox);

        preview = new Label();
        preview.Location = new Point(270, 100);
        preview.Size = new Size(500, 340);
        preview.Font = new Font("Arial", 32);
        preview.Text = inputBox.Text;
        Controls.Add(preview);
    }

    void UpdatePreview(object sender, EventArgs e)
    {
        if (fontList.SelectedItem == null) return;

        string fontName = fontList.SelectedItem.ToString();
        float size = (float)sizeBox.Value;

        FontStyle style = FontStyle.Regular;
        if (boldBox.Checked) style |= FontStyle.Bold;
        if (italicBox.Checked) style |= FontStyle.Italic;

        try
        {
            preview.Font = new Font(fontName, size, style);
            preview.Text = inputBox.Text;
        }
        catch
        {
            MessageBox.Show("このフォントは選択したスタイルに対応していません。");
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.Run(new FontTester());
    }
}