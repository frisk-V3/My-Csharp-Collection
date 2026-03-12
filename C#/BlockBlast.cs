using System;
using System.Drawing;
using System.Windows.Forms;

public class BreakoutRetro : Form
{
    const int PaddleWidth = 90;
    const int PaddleHeight = 12;
    const int BallSize = 10;
    const int BrickRows = 6;
    const int BrickCols = 10;
    const int BrickWidth = 60;
    const int BrickHeight = 20;

    Rectangle paddle;
    Rectangle ball;
    int ballDx = 4;
    int ballDy = -4;

    bool[,] bricks = new bool[BrickRows, BrickCols];

    Timer timer;
    int score = 0;
    int stage = 1;

    Random rand = new Random();

    public BreakoutRetro()
    {
        Text = "Retro Breakout";
        DoubleBuffered = true;
        ClientSize = new Size(BrickCols * BrickWidth + 40, 480);
        BackColor = Color.Black;

        InitStage();

        timer = new Timer();
        timer.Interval = 16;
        timer.Tick += UpdateGame;
        timer.Start();

        KeyDown += OnKeyDown;
        MouseMove += OnMouseMove;
        Paint += OnPaint;
    }

    void InitStage()
    {
        int paddleX = (ClientSize.Width - PaddleWidth) / 2;
        int paddleY = ClientSize.Height - 60;
        paddle = new Rectangle(paddleX, paddleY, PaddleWidth, PaddleHeight);

        int ballX = paddle.X + PaddleWidth / 2 - BallSize / 2;
        int ballY = paddle.Y - BallSize - 2;
        ball = new Rectangle(ballX, ballY, BallSize, BallSize);

        for (int r = 0; r < BrickRows; r++)
            for (int c = 0; c < BrickCols; c++)
                bricks[r, c] = true;

        ballDx = rand.Next(0, 2) == 0 ? -4 : 4;
        ballDy = -4 - (stage - 1);
    }

    void UpdateGame(object sender, EventArgs e)
    {
        ball.X += ballDx;
        ball.Y += ballDy;

        if (ball.Left <= 0 || ball.Right >= ClientSize.Width)
            ballDx = -ballDx;

        if (ball.Top <= 0)
            ballDy = -ballDy;

        if (ball.Bottom >= ClientSize.Height)
        {
            timer.Stop();
            Application.Restart(); // タブ閉じてもう一回開くイメージ
            return;
        }

        if (ball.IntersectsWith(paddle))
        {
            int hitPos = ball.X + BallSize / 2 - paddle.X;
            float ratio = (float)hitPos / PaddleWidth - 0.5f;
            ballDx = (int)(ratio * 10);
            if (ballDx == 0) ballDx = rand.Next(0, 2) == 0 ? -3 : 3;
            ballDy = -Math.Abs(ballDy);
        }

        for (int r = 0; r < BrickRows; r++)
        {
            for (int c = 0; c < BrickCols; c++)
            {
                if (!bricks[r, c]) continue;

                Rectangle brickRect = new Rectangle(
                    20 + c * BrickWidth,
                    40 + r * BrickHeight,
                    BrickWidth - 2,
                    BrickHeight - 2
                );

                if (ball.IntersectsWith(brickRect))
                {
                    bricks[r, c] = false;
                    score += 100;

                    Rectangle overlap = Rectangle.Intersect(ball, brickRect);
                    if (overlap.Width > overlap.Height)
                        ballDy = -ballDy;
                    else
                        ballDx = -ballDx;

                    goto EndCollision;
                }
            }
        }

    EndCollision:

        if (IsAllBricksCleared())
        {
            stage++;
            InitStage();
        }

        Invalidate();
    }

    bool IsAllBricksCleared()
    {
        for (int r = 0; r < BrickRows; r++)
            for (int c = 0; c < BrickCols; c++)
                if (bricks[r, c]) return false;
        return true;
    }

    void OnMouseMove(object sender, MouseEventArgs e)
    {
        int x = e.X - PaddleWidth / 2;
        if (x < 0) x = 0;
        if (x + PaddleWidth > ClientSize.Width) x = ClientSize.Width - PaddleWidth;
        paddle.X = x;
        Invalidate();
    }

    void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Left)
        {
            paddle.X -= 15;
            if (paddle.X < 0) paddle.X = 0;
        }
        else if (e.KeyCode == Keys.Right)
        {
            paddle.X += 15;
            if (paddle.X + PaddleWidth > ClientSize.Width)
                paddle.X = ClientSize.Width - PaddleWidth;
        }
        Invalidate();
    }

    void OnPaint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        using (Brush b = new SolidBrush(Color.White))
            g.FillRectangle(b, paddle);

        using (Brush b = new SolidBrush(Color.White))
            g.FillEllipse(b, ball);

        using (Brush b = new SolidBrush(Color.FromArgb(0x70, 0xE0, 0x70)))
        {
            for (int r = 0; r < BrickRows; r++)
                for (int c = 0; c < BrickCols; c++)
                    if (bricks[r, c])
                        g.FillRectangle(b, 20 + c * BrickWidth, 40 + r * BrickHeight, BrickWidth - 2, BrickHeight - 2);
        }

        using (Brush b = new SolidBrush(Color.White))
        {
            g.DrawString("SCORE: " + score, new Font("Consolas", 12), b, 10, 10);
            g.DrawString("STAGE: " + stage, new Font("Consolas", 12), b, 140, 10);
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.Run(new BreakoutRetro());
    }
}