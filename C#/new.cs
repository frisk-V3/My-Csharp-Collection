using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class SnakeGame : Form
{
    Timer timer = new Timer();
    List<Point> snake = new List<Point>();
    List<Point> obstacles = new List<Point>();

    enum ItemType { Apple, Grape, Orange, Star }
    Point itemPos;
    ItemType itemType;

    int gridSize = 20;   // 初期グリッド
    int tile = 25;       // 描画用（後で計算し直す）

    int dx = 1;
    int dy = 0;

    bool gameOver = false;
    bool paused = false;

    bool invincible = false;
    int invincibleTimer = 0;

    int score = 0;
    int level = 1;

    Random rand = new Random();

    Font font = new Font("Consolas", 14);

    const int playAreaSize = 500; // 描画領域（正方形）

    public SnakeGame()
    {
        Text = "Snake Game - Hyper Mode";
        ClientSize = new Size(playAreaSize, playAreaSize + 40);
        DoubleBuffered = true;

        InitGame();

        timer.Interval = 100;
        timer.Tick += UpdateGame;
        timer.Start();

        KeyDown += KeyInput;
    }

    void InitGame()
    {
        snake.Clear();
        snake.Add(new Point(gridSize / 2, gridSize / 2));
        snake.Add(new Point(gridSize / 2 - 1, gridSize / 2));
        snake.Add(new Point(gridSize / 2 - 2, gridSize / 2));

        dx = 1;
        dy = 0;

        score = 0;
        level = 1;
        gameOver = false;
        paused = false;

        invincible = false;
        invincibleTimer = 0;

        UpdateGridByLevel();
        GenerateObstacles();
        SpawnItem();
    }

    void UpdateGridByLevel()
    {
        // レベルに応じてグリッド拡大（最大40）
        int newGrid = 20 + (level - 1) * 5;
        if (newGrid > 40) newGrid = 40;

        gridSize = newGrid;
        tile = playAreaSize / gridSize;
    }

    void GenerateObstacles()
    {
        obstacles.Clear();
        int count = level * 3; // レベルに応じて増える

        for (int i = 0; i < count; i++)
        {
            while (true)
            {
                Point p = new Point(rand.Next(gridSize), rand.Next(gridSize));
                if (!snake.Contains(p) && p != itemPos)
                {
                    obstacles.Add(p);
                    break;
                }
            }
        }
    }

    void SpawnItem()
    {
        while (true)
        {
            Point p = new Point(rand.Next(gridSize), rand.Next(gridSize));
            if (!snake.Contains(p) && !obstacles.Contains(p))
            {
                itemPos = p;

                int r = rand.Next(100);
                if (r < 60) itemType = ItemType.Apple;       // 60%
                else if (r < 85) itemType = ItemType.Grape;  // 25%
                else if (r < 95) itemType = ItemType.Orange; // 10%
                else itemType = ItemType.Star;               // 5%

                break;
            }
        }
    }

    void KeyInput(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up && dy == 0) { dx = 0; dy = -1; }
        if (e.KeyCode == Keys.Down && dy == 0) { dx = 0; dy = 1; }
        if (e.KeyCode == Keys.Left && dx == 0) { dx = -1; dy = 0; }
        if (e.KeyCode == Keys.Right && dx == 0) { dx = 1; dy = 0; }

        if (e.KeyCode == Keys.Space)
            paused = !paused;

        if (gameOver && e.KeyCode == Keys.Enter)
        {
            InitGame();
            timer.Start();
        }
    }

    void UpdateGame(object sender, EventArgs e)
    {
        if (paused || gameOver) return;

        Point head = snake[0];
        Point newHead = new Point(head.X + dx, head.Y + dy);

        // 壁判定（無敵ならワープ）
        if (newHead.X < 0 || newHead.Y < 0 || newHead.X >= gridSize || newHead.Y >= gridSize)
        {
            if (invincible)
            {
                if (newHead.X < 0) newHead.X = gridSize - 1;
                if (newHead.X >= gridSize) newHead.X = 0;
                if (newHead.Y < 0) newHead.Y = gridSize - 1;
                if (newHead.Y >= gridSize) newHead.Y = 0;
            }
            else
            {
                EndGame();
                return;
            }
        }

        // 障害物判定（無敵なら無視）
        if (!invincible && obstacles.Contains(newHead))
        {
            EndGame();
            return;
        }

        // 自分に当たったら死亡
        if (snake.Contains(newHead))
        {
            EndGame();
            return;
        }

        snake.Insert(0, newHead);

        // アイテム取得
        if (newHead == itemPos)
        {
            switch (itemType)
            {
                case ItemType.Apple:
                    score += 10;
                    break;
                case ItemType.Grape:
                    score += 20;
                    break;
                case ItemType.Orange:
                    score += 30;
                    break;
                case ItemType.Star:
                    invincible = true;
                    invincibleTimer = 100; // 約10秒
                    break;
            }

            if (timer.Interval > 40)
                timer.Interval -= 2;

            // レベルアップ判定（100点ごとにレベルアップ）
            int newLevel = score / 100 + 1;
            if (newLevel > level)
            {
                level = newLevel;
                UpdateGridByLevel();
                GenerateObstacles();
            }

            SpawnItem();
        }
        else
        {
            snake.RemoveAt(snake.Count - 1);
        }

        // 無敵タイマー
        if (invincible)
        {
            invincibleTimer--;
            if (invincibleTimer <= 0)
                invincible = false;
        }

        Invalidate();
    }

    void EndGame()
    {
        gameOver = true;
        timer.Stop();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        g.Clear(Color.Black);

        // 壁（枠線）
        g.DrawRectangle(Pens.White, 0, 0, gridSize * tile, gridSize * tile);

        // 障害物
        foreach (var ob in obstacles)
        {
            g.FillRectangle(Brushes.DimGray, ob.X * tile, ob.Y * tile, tile - 1, tile - 1);
        }

        // スネーク描画（無敵中は黄色）
        for (int i = 0; i < snake.Count; i++)
        {
            Brush b = invincible ? Brushes.Yellow : Brushes.Lime;
            Point p = snake[i];
            g.FillRectangle(b, p.X * tile, p.Y * tile, tile - 1, tile - 1);
        }

        // アイテム描画
        Brush itemBrush = Brushes.Red;
        switch (itemType)
        {
            case ItemType.Apple: itemBrush = Brushes.Red; break;
            case ItemType.Grape: itemBrush = Brushes.Purple; break;
            case ItemType.Orange: itemBrush = Brushes.Orange; break;
            case ItemType.Star: itemBrush = Brushes.Yellow; break;
        }

        g.FillRectangle(itemBrush, itemPos.X * tile, itemPos.Y * tile, tile - 1, tile - 1);

        // スコア・レベル
        g.DrawString("Score: " + score, font, Brushes.White, 10, playAreaSize + 5);
        g.DrawString("Level: " + level, font, Brushes.Cyan, 200, playAreaSize + 5);

        if (paused)
        {
            g.DrawString("PAUSED", font, Brushes.Yellow, 180, 200);
        }

        if (gameOver)
        {
            g.DrawString("GAME OVER", font, Brushes.Red, 150, 200);
            g.DrawString("Press ENTER to Restart", new Font("Consolas", 12), Brushes.White, 130, 230);
        }
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new SnakeGame());
    }
}