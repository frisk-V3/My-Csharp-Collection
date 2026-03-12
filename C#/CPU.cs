using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

class CpuMonitorForm : Form
{
    System.Windows.Forms.Timer timer;
    PerformanceCounter cpuCounter;
    List<float> history = new List<float>();
    const int MaxPoints = 200;

    Button loadButton;
    bool loadRunning = false;
    Thread loadThread;

    public CpuMonitorForm()
    {
        Text = "CPU Monitor (csc.exe only)";
        Width = 900;
        Height = 500;
        DoubleBuffered = true;
        BackColor = Color.FromArgb(20, 20, 30);

        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 500; // 0.5秒ごとにサンプル
        timer.Tick += (s, e) => SampleCpu();
        timer.Start();

        loadButton = new Button();
        loadButton.Text = "CPU負荷 ON";
        loadButton.Width = 120;
        loadButton.Height = 30;
        loadButton.Left = 10;
        loadButton.Top = 10;
        loadButton.Click += (s, e) => ToggleLoad();
        Controls.Add(loadButton);
    }

    void SampleCpu()
    {
        float value;
        try
        {
            value = cpuCounter.NextValue();
        }
        catch
        {
            value = 0;
        }

        if (value < 0) value = 0;
        if (value > 100) value = 100;

        history.Add(value);
        if (history.Count > MaxPoints)
            history.RemoveAt(0);

        Invalidate();
    }

    void ToggleLoad()
    {
        if (!loadRunning)
        {
            loadRunning = true;
            loadButton.Text = "CPU負荷 OFF";

            loadThread = new Thread(LoadWorker);
            loadThread.IsBackground = true;
            loadThread.Start();
        }
        else
        {
            loadRunning = false;
            loadButton.Text = "CPU負荷 ON";
        }
    }

    // ★ フルC#負荷バージョン ★
    void LoadWorker()
    {
        while (loadRunning)
        {
            // 50msだけCPUを回す（純C#）
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 50)
            {
                int x = 0;
                for (int i = 0; i < 200000; i++)
                {
                    x += i; // 純粋な整数演算
                }
            }

            // 50ms休憩
            Thread.Sleep(50);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;

        int marginLeft = 50;
        int marginRight = 20;
        int marginTop = 60;
        int marginBottom = 40;

        Rectangle graphRect = new Rectangle(
            marginLeft,
            marginTop,
            ClientSize.Width - marginLeft - marginRight,
            ClientSize.Height - marginTop - marginBottom
        );

        if (graphRect.Width <= 0 || graphRect.Height <= 0)
            return;

        using (var bgBrush = new SolidBrush(Color.FromArgb(30, 30, 50)))
            g.FillRectangle(bgBrush, graphRect);

        using (var penBorder = new Pen(Color.FromArgb(80, 80, 120), 1))
            g.DrawRectangle(penBorder, graphRect);

        using (var gridPen = new Pen(Color.FromArgb(60, 60, 100), 1))
        using (var textBrush = new SolidBrush(Color.White))
        using (var font = new Font("Segoe UI", 9))
        {
            for (int i = 0; i <= 5; i++)
            {
                float yValue = i * 20;
                int y = graphRect.Bottom - (int)(graphRect.Height * (yValue / 100f));
                g.DrawLine(gridPen, graphRect.Left, y, graphRect.Right, y);
                g.DrawString(yValue.ToString("0") + " %", font, textBrush, 5, y - 8);
            }

            g.DrawString("CPU 使用率 (全体)", new Font("Segoe UI", 14),
                Brushes.White, marginLeft, 20);
        }

        if (history.Count >= 2)
        {
            using (var linePen = new Pen(Color.Cyan, 2))
            {
                float dx = (float)graphRect.Width / (MaxPoints - 1);
                PointF? prev = null;

                for (int i = 0; i < history.Count; i++)
                {
                    float v = history[i];
                    float x = graphRect.Left + dx * (MaxPoints - history.Count + i);
                    float y = graphRect.Bottom - (graphRect.Height * (v / 100f));

                    PointF p = new PointF(x, y);
                    if (prev.HasValue)
                        g.DrawLine(linePen, prev.Value, p);
                    prev = p;
                }
            }
        }

        if (history.Count > 0)
        {
            float last = history[history.Count - 1];
            string text = "現在のCPU使用率: " + last.ToString("0.0") + " %";
            using (var font = new Font("Segoe UI", 11))
            using (var brush = new SolidBrush(Color.LightGreen))
            {
                g.DrawString(text, font, brush, marginLeft + 200, 25);
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        loadRunning = false;
        base.OnFormClosing(e);
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new CpuMonitorForm());
    }
}