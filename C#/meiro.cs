using System;
using System.Drawing;
using System.Windows.Forms;

public class MazeForm : Form
{
    int round = 1;
    int W, H;
    char[,] maze;
    int px, py;
    int gx, gy;
    Random rnd = new Random();

    public MazeForm()
    {
        this.DoubleBuffered = true;
        this.Width = 900;
        this.Height = 700;
        this.Font = new Font("Consolas", 18);
        this.BackColor = Color.Black;
        this.KeyDown += OnKeyDown;

        StartRound();
    }

    void StartRound()
    {
        // ラウンドごとにサイズ変更
        switch (round)
        {
            case 1: W = 41; H = 31; break;
            case 2: W = 51; H = 41; break;
            case 3: W = 61; H = 51; break;
            case 4: W = 81; H = 61; break;
            case 5: W = 101; H = 71; break;
        }

        maze = new char[H, W];
        GenerateMaze();
    }

    void GenerateMaze()
    {
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                maze[y, x] = '#';

        DFS(1, 1);

        px = 1; py = 1;
        gx = W - 2; gy = H - 2;
        maze[py, px] = 'S';
        maze[gy, gx] = 'G';
    }

    void DFS(int y, int x)
    {
        maze[y, x] = ' ';

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { -1, 0, 1, 0 };

        // シャッフル（C#5対応）
        for (int i = 0; i < 4; i++)
        {
            int r = rnd.Next(4);

            int tx = dx[i];
            dx[i] = dx[r];
            dx[r] = tx;

            int ty = dy[i];
            dy[i] = dy[r];
            dy[r] = ty;
        }

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i] * 2;
            int ny = y + dy[i] * 2;

            if (ny > 0 && ny < H - 1 && nx > 0 && nx < W - 1 && maze[ny, nx] == '#')
            {
                maze[y + dy[i], x + dx[i]] = ' ';
                DFS(ny, nx);
            }
        }
    }

    void OnKeyDown(object sender, KeyEventArgs e)
    {
        int nx = px, ny = py;

        if (e.KeyCode == Keys.Up) ny--;
        if (e.KeyCode == Keys.Down) ny++;
        if (e.KeyCode == Keys.Left) nx--;
        if (e.KeyCode == Keys.Right) nx++;

        if (maze[ny, nx] != '#')
        {
            px = nx;
            py = ny;

            if (px == gx && py == gy)
            {
                round++;
                if (round > 5)
                {
                    MessageBox.Show("全ラウンドクリア！人間卒業おめでとう！");
                    round = 1;
                }
                StartRound();
            }
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;

        for (int y = 0; y < H; y++)
        {
            string line = "";
            for (int x = 0; x < W; x++)
            {
                bool visible = true;

                // Round2以降は視界3マス制限
                if (round >= 2)
                {
                    if (Math.Abs(x - px) > 3 || Math.Abs(y - py) > 3)
                        visible = false;
                }

                if (!visible)
                {
                    line += "■";
                }
                else
                {
                    if (x == px && y == py)
                        line += "P";
                    else
                        line += maze[y, x];
                }
            }
            g.DrawString(line, this.Font, Brushes.White, 20, 20 + y * 20);
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.Run(new MazeForm());
    }
}