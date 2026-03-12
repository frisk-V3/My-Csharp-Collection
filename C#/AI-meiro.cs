using System;
using System.Collections.Generic;
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

    List<Point> path = new List<Point>(); // AIの経路
    int pathIndex = 0;
    Timer timer;

    public MazeForm()
    {
        this.DoubleBuffered = true;
        this.Width = 1000;
        this.Height = 800;
        this.Font = new Font("Consolas", 18);
        this.BackColor = Color.Black;

        timer = new Timer();
        timer.Interval = 50; // AIの移動速度
        timer.Tick += OnTick;

        StartRound();
        timer.Start();
    }

    void StartRound()
    {
        // ラウンドごとに迷路サイズ変更（人間卒業レベル）
        switch (round)
        {
            case 1: W = 41; H = 31; break;
            case 2: W = 51; H = 41; break;
            case 3: W = 61; H = 51; break;
            case 4: W = 81; H = 61; break;
            case 5: W = 101; H = 71; break;
            default: W = 41; H = 31; break;
        }

        GenerateMaze();
        FindPath();   // AIが最短経路を計算
        pathIndex = 0;
        Invalidate();
    }

    void GenerateMaze()
    {
        maze = new char[H, W];

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

        // 方向シャッフル（C#5対応）
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

    // BFSで最短経路を探す
    void FindPath()
    {
        Queue<Point> q = new Queue<Point>();
        bool[,] visited = new bool[H, W];
        Point[,] parent = new Point[H, W];

        q.Enqueue(new Point(1, 1));
        visited[1, 1] = true;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            Point p = q.Dequeue();

            if (p.X == gx && p.Y == gy)
                break;

            for (int i = 0; i < 4; i++)
            {
                int nx = p.X + dx[i];
                int ny = p.Y + dy[i];

                if (nx >= 0 && nx < W && ny >= 0 && ny < H &&
                    maze[ny, nx] != '#' && !visited[ny, nx])
                {
                    visited[ny, nx] = true;
                    parent[ny, nx] = p;
                    q.Enqueue(new Point(nx, ny));
                }
            }
        }

        // ゴールから逆順にたどって経路を作る
        path.Clear();
        Point cur = new Point(gx, gy);

        while (!(cur.X == 1 && cur.Y == 1))
        {
            path.Add(cur);
            cur = parent[cur.Y, cur.X];
        }

        path.Add(new Point(1, 1));
        path.Reverse();
    }

    void OnTick(object sender, EventArgs e)
    {
        if (pathIndex < path.Count)
        {
            px = path[pathIndex].X;
            py = path[pathIndex].Y;
            pathIndex++;

            if (px == gx && py == gy)
            {
                // ラウンドクリア
                round++;
                if (round > 5)
                {
                    timer.Stop();
                    MessageBox.Show("AIが5ステージのニンゲンソツギョウメイロを完全制覇しました。");
                    round = 1;
                    StartRound();
                    timer.Start();
                }
                else
                {
                    StartRound();
                }
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

                // Round2以降は視界3マス制限（AIは全体を知ってるけど、表示だけ制限）
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

        // 画面左上にラウンド表示
        g.DrawString("ROUND: " + round, this.Font, Brushes.Yellow, 20, 20 + H * 20 + 10);
    }

    [STAThread]
    public static void Main()
    {
        Application.Run(new MazeForm());
    }
}