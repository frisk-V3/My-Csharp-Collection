using System;
using System.Drawing;
using System.Windows.Forms;

public class Tetromino
{
    public int[,] Shape;
    public Color Color;

    public Tetromino(int[,] shape, Color color)
    {
        Shape = shape;
        Color = color;
    }
}

public class Tetris : Form
{
    const int CanvasWidth = 10;
    const int CanvasHeight = 20;
    const int TileSize = 30;

    int[,] grid = new int[CanvasWidth, CanvasHeight];
    Color[,] gridColors = new Color[CanvasWidth, CanvasHeight];

    int score = 0;
    Timer timer = new Timer();

    Point currentPos;
    int[,] currentPiece, nextPiece;
    Color currentColor, nextColor;

    Random rand = new Random();
    bool isFullScreen = false;

    Tetromino[] shapes =
    {
        new Tetromino(new int[,] { {1,1,1,1} }, Color.Cyan),          // I
        new Tetromino(new int[,] { {1,1},{1,1} }, Color.Yellow),      // O
        new Tetromino(new int[,] { {0,1,0},{1,1,1} }, Color.Purple),  // T
        new Tetromino(new int[,] { {0,1,1},{1,1,0} }, Color.Green),   // S
        new Tetromino(new int[,] { {1,1,0},{0,1,1} }, Color.Red),     // Z
        new Tetromino(new int[,] { {1,0,0},{1,1,1} }, Color.Orange),  // L
        new Tetromino(new int[,] { {0,0,1},{1,1,1} }, Color.Blue),    // J
        new Tetromino(new int[,] { {1} }, Color.White),               // Dot
        new Tetromino(new int[,] { {1,1} }, Color.Pink),              // Mini-I
        new Tetromino(new int[,] { {1,0},{1,1} }, Color.Lime)         // Mini-L
    };

    public Tetris()
    {
        Text = "Super Tetris 10";
        ClientSize = new Size(CanvasWidth * TileSize + 150, CanvasHeight * TileSize + 50);
        DoubleBuffered = true;
        BackColor = Color.FromArgb(20, 20, 20);

        KeyDown += OnKeyDown;

        timer.Interval = 500;
        timer.Tick += (s, e) =>
        {
            MovePiece(0, 1);
            Invalidate();
        };

        Tetromino first = shapes[rand.Next(shapes.Length)];
        nextPiece = first.Shape;
        nextColor = first.Color;

        SpawnPiece();
        timer.Start();
    }

    void SpawnPiece()
    {
        currentPiece = nextPiece;
        currentColor = nextColor;

        Tetromino next = shapes[rand.Next(shapes.Length)];
        nextPiece = next.Shape;
        nextColor = next.Color;

        currentPos = new Point(CanvasWidth / 2 - currentPiece.GetLength(1) / 2, 0);

        if (CheckCollision(0, 0))
        {
            timer.Stop();
            MessageBox.Show("Game Over! Score: " + score);
            Application.Restart();
        }
    }

    bool CheckCollision(int dx, int dy, int[,] piece = null)
    {
        int[,] p = piece ?? currentPiece;

        for (int y = 0; y < p.GetLength(0); y++)
        {
            for (int x = 0; x < p.GetLength(1); x++)
            {
                if (p[y, x] == 0) continue;

                int nx = currentPos.X + x + dx;
                int ny = currentPos.Y + y + dy;

                if (nx < 0 || nx >= CanvasWidth || ny >= CanvasHeight)
                    return true;

                if (ny >= 0 && grid[nx, ny] != 0)
                    return true;
            }
        }

        return false;
    }

    void MovePiece(int dx, int dy)
    {
        if (!CheckCollision(dx, dy))
        {
            currentPos.X += dx;
            currentPos.Y += dy;
        }
        else if (dy > 0)
        {
            LockPiece();
            ClearLines();
            SpawnPiece();
        }
    }

    void LockPiece()
    {
        for (int y = 0; y < currentPiece.GetLength(0); y++)
        {
            for (int x = 0; x < currentPiece.GetLength(1); x++)
            {
                if (currentPiece[y, x] == 0) continue;

                grid[currentPos.X + x, currentPos.Y + y] = 1;
                gridColors[currentPos.X + x, currentPos.Y + y] = currentColor;
            }
        }
    }

    void ClearLines()
    {
        for (int y = CanvasHeight - 1; y >= 0; y--)
        {
            bool full = true;

            for (int x = 0; x < CanvasWidth; x++)
            {
                if (grid[x, y] == 0)
                {
                    full = false;
                    break;
                }
            }

            if (!full) continue;

            for (int ty = y; ty > 0; ty--)
            {
                for (int tx = 0; tx < CanvasWidth; tx++)
                {
                    grid[tx, ty] = grid[tx, ty - 1];
                    gridColors[tx, ty] = gridColors[tx, ty - 1];
                }
            }

            score += 100;
            y++;
        }
    }

    void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Left) MovePiece(-1, 0);
        if (e.KeyCode == Keys.Right) MovePiece(1, 0);
        if (e.KeyCode == Keys.Down) MovePiece(0, 1);

        if (e.KeyCode == Keys.Q) RotateLeft();
        if (e.KeyCode == Keys.W) RotateRight();

        if (e.KeyCode == Keys.Space) HardDrop();

        if (e.KeyCode == Keys.F11) ToggleFullScreen();

        Invalidate();
    }

    void HardDrop()
    {
        while (!CheckCollision(0, 1))
            currentPos.Y++;

        MovePiece(0, 1);
    }

    void ToggleFullScreen()
    {
        if (!isFullScreen)
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
        }
        else
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Normal;
        }

        isFullScreen = !isFullScreen;
    }

    void RotateRight()
    {
        int r = currentPiece.GetLength(0);
        int c = currentPiece.GetLength(1);

        int[,] rotated = new int[c, r];

        for (int y = 0; y < r; y++)
            for (int x = 0; x < c; x++)
                rotated[x, r - 1 - y] = currentPiece[y, x];

        int[] kicks = { 0, 1, -1, 2, -2 };

        foreach (int k in kicks)
        {
            if (!CheckCollision(k, 0, rotated))
            {
                currentPos.X += k;
                currentPiece = rotated;
                return;
            }
        }
    }

    void RotateLeft()
    {
        int r = currentPiece.GetLength(0);
        int c = currentPiece.GetLength(1);

        int[,] rotated = new int[c, r];

        for (int y = 0; y < r; y++)
            for (int x = 0; x < c; x++)
                rotated[c - 1 - x, y] = currentPiece[y, x];

        int[] kicks = { 0, 1, -1, 2, -2 };

        foreach (int k in kicks)
        {
            if (!CheckCollision(k, 0, rotated))
            {
                currentPos.X += k;
                currentPiece = rotated;
                return;
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        int offsetX = (ClientSize.Width - (CanvasWidth * TileSize + 120)) / 2;
        int offsetY = (ClientSize.Height - (CanvasHeight * TileSize)) / 2;

        g.FillRectangle(Brushes.Black, offsetX, offsetY, CanvasWidth * TileSize, CanvasHeight * TileSize);
        g.DrawRectangle(Pens.Gray, offsetX, offsetY, CanvasWidth * TileSize, CanvasHeight * TileSize);

        for (int x = 0; x < CanvasWidth; x++)
            for (int y = 0; y < CanvasHeight; y++)
                if (grid[x, y] != 0)
                    DrawTile(g, gridColors[x, y], offsetX + x * TileSize, offsetY + y * TileSize);

        for (int y = 0; y < currentPiece.GetLength(0); y++)
            for (int x = 0; x < currentPiece.GetLength(1); x++)
                if (currentPiece[y, x] != 0)
                    DrawTile(g, currentColor, offsetX + (currentPos.X + x) * TileSize, offsetY + (currentPos.Y + y) * TileSize);

        int nextX = offsetX + CanvasWidth * TileSize + 30;

        g.DrawString("NEXT", new Font("Arial", 12, FontStyle.Bold), Brushes.White, nextX, offsetY);

        for (int y = 0; y < nextPiece.GetLength(0); y++)
            for (int x = 0; x < nextPiece.GetLength(1); x++)
                if (nextPiece[y, x] != 0)
                    DrawTile(g, nextColor, nextX + x * TileSize, offsetY + 30 + y * TileSize);

        g.DrawString("Score: " + score, new Font("Arial", 12), Brushes.White, nextX, offsetY + 150);
        g.DrawString("Q: LeftRotate / W: RightRotate\nSpace: HardDrop\nF11: FullScreen", new Font("Arial", 9), Brushes.Gray, nextX, offsetY + 200);
    }

    void DrawTile(Graphics g, Color color, int px, int py)
    {
        g.FillRectangle(new SolidBrush(color), px, py, TileSize - 1, TileSize - 1);
        g.DrawRectangle(new Pen(Color.FromArgb(50, 255, 255, 255)), px, py, TileSize - 1, TileSize - 1);
    }

    [STAThread]
    public static void Main()
    {
        Application.Run(new Tetris());
    }
}