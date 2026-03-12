using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace NetCardBattleWin
{
    enum CpuType
    {
        Aggro = 1,
        Control = 2,
        Midrange = 3
    }

    enum CardType
    {
        Unit,
        Spell
    }

    class Card
    {
        public string Name { get; private set; }
        public CardType Type { get; private set; }
        public int Cost { get; private set; }

        public int Attack { get; private set; }
        public int Health { get; set; }

        public int Damage { get; private set; }
        public bool TargetUnitFirst { get; private set; }

        public Card(string name, int cost, int attack, int health)
        {
            Name = name;
            Cost = cost;
            Type = CardType.Unit;
            Attack = attack;
            Health = health;
        }

        public Card(string name, int cost, int damage, bool targetUnitFirst)
        {
            Name = name;
            Cost = cost;
            Type = CardType.Spell;
            Damage = damage;
            TargetUnitFirst = targetUnitFirst;
        }

        public Card Clone()
        {
            if (Type == CardType.Unit)
                return new Card(Name, Cost, Attack, Health);
            else
                return new Card(Name, Cost, Damage, TargetUnitFirst);
        }

        public override string ToString()
        {
            if (Type == CardType.Unit)
                return "[U] " + Name + " (C" + Cost + ") ATK " + Attack + " / HP " + Health;
            else
                return "[S] " + Name + " (C" + Cost + ") DMG " + Damage;
        }
    }

    class Player
    {
        public string Name { get; private set; }
        public int HP { get; set; }
        public int Energy { get; set; }
        public List<Card> Deck { get; private set; }
        public List<Card> Hand { get; private set; }
        public List<Card> Board { get; private set; }

        public Player(string name)
        {
            Name = name;
            HP = 500;
            Energy = 0;
            Deck = new List<Card>();
            Hand = new List<Card>();
            Board = new List<Card>();
        }

        public void Draw(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (Deck.Count == 0) break;
                Card top = Deck[0];
                Deck.RemoveAt(0);
                Hand.Add(top);
            }
        }

        public void Draw()
        {
            Draw(1);
        }

        public void TakeDamage(int dmg)
        {
            HP -= dmg;
            if (HP < 0) HP = 0;
        }

        public void CleanupDeadUnits()
        {
            List<Card> alive = new List<Card>();
            for (int i = 0; i < Board.Count; i++)
            {
                if (Board[i].Health > 0) alive.Add(Board[i]);
            }
            Board = alive;
        }
    }

    class CpuPlayer : Player
    {
        public CpuType Type { get; private set; }
        private Action<string> log;

        public CpuPlayer(CpuType type, Action<string> logger) : base(type.ToString() + " CPU")
        {
            Type = type;
            log = logger;
        }

        public void TakeTurn(Player opponent, int turnNumber)
        {
            Energy++;
            Draw();

            log("");
            log("[" + Name + " のターン] HP:" + HP + " EN:" + Energy);
            log("相手 " + opponent.Name + " HP:" + opponent.HP);

            bool playedSomething;
            do
            {
                playedSomething = TryPlayBestCard(opponent);
            } while (playedSomething && opponent.HP > 0);

            CpuAttack(opponent);

            CleanupDeadUnits();
            opponent.CleanupDeadUnits();
        }

        private bool TryPlayBestCard(Player opponent)
        {
            List<Card> playable = new List<Card>();
            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand[i].Cost <= Energy) playable.Add(Hand[i]);
            }
            if (playable.Count == 0) return false;

            Card best = null;
            int bestScore = int.MinValue;

            for (int i = 0; i < playable.Count; i++)
            {
                Card card = playable[i];
                int score = EvaluateCard(card, opponent);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = card;
                }
            }

            if (best == null) return false;

            PlayCard(best, opponent);
            return true;
        }

        private int EvaluateCard(Card card, Player opponent)
        {
            int score = 0;

            if (Type == CpuType.Aggro)
            {
                if (card.Type == CardType.Unit)
                    score = card.Attack * 2 + card.Health;
                else
                    score = card.Damage * 2;
            }
            else if (Type == CpuType.Control)
            {
                if (card.Type == CardType.Spell)
                {
                    int unitCount = opponent.Board.Count;
                    score = card.Damage * 2 + unitCount * 10;
                }
                else
                {
                    score = card.Health + card.Attack;
                }
            }
            else
            {
                if (card.Type == CardType.Unit)
                    score = card.Attack + card.Health;
                else
                    score = card.Damage + opponent.Board.Count * 5;
            }

            score -= card.Cost;
            return score;
        }

        private void PlayCard(Card card, Player opponent)
        {
            Energy -= card.Cost;
            Hand.Remove(card);

            if (card.Type == CardType.Unit)
            {
                log(Name + " はユニット [" + card.Name + "] (ATK " + card.Attack + " / HP " + card.Health + ") を召喚！");
                Board.Add(card);
            }
            else
            {
                log(Name + " は呪文 [" + card.Name + "] を発動！");
                ResolveSpell(card, this, opponent, log);
            }
        }

        private void CpuAttack(Player opponent)
        {
            if (Board.Count == 0)
            {
                log(Name + " の場にはユニットがいない。攻撃できない。");
                return;
            }

            log(Name + " のユニットが攻撃を開始！");

            for (int i = 0; i < Board.Count; i++)
            {
                Card unit = Board[i];
                if (opponent.Board.Count > 0)
                {
                    Card target = null;
                    int minHp = int.MaxValue;
                    for (int j = 0; j < opponent.Board.Count; j++)
                    {
                        if (opponent.Board[j].Health < minHp)
                        {
                            minHp = opponent.Board[j].Health;
                            target = opponent.Board[j];
                        }
                    }

                    log("[" + unit.Name + "] が 相手ユニット [" + target.Name + "] を攻撃！ " + unit.Attack + " ダメージ");
                    target.Health -= unit.Attack;
                    unit.Health -= target.Attack;
                }
                else
                {
                    log("[" + unit.Name + "] が 本体 " + opponent.Name + " を攻撃！ " + unit.Attack + " ダメージ");
                    opponent.TakeDamage(unit.Attack);
                }
            }
        }

        public static void ResolveSpell(Card spell, Player self, Player opponent, Action<string> log)
        {
            int dmg = spell.Damage;

            if (spell.TargetUnitFirst && opponent.Board.Count > 0)
            {
                Card target = null;
                int maxAtk = int.MinValue;
                for (int i = 0; i < opponent.Board.Count; i++)
                {
                    if (opponent.Board[i].Attack > maxAtk)
                    {
                        maxAtk = opponent.Board[i].Attack;
                        target = opponent.Board[i];
                    }
                }

                log("呪文が相手ユニット [" + target.Name + "] に " + dmg + " ダメージ！");
                target.Health -= dmg;
            }
            else
            {
                log("呪文が本体 " + opponent.Name + " に " + dmg + " ダメージ！");
                opponent.TakeDamage(dmg);
            }
        }
    }

    class MainForm : Form
    {
        private Player human;
        private CpuPlayer cpu;
        private int turn = 1;

        private Label lblHuman;
        private Label lblCpu;
        private ListBox lstHand;
        private ListBox lstBoardHuman;
        private ListBox lstBoardCpu;
        private Button btnPlayCard;
        private Button btnEndTurn;
        private TextBox txtLog;
        private ComboBox cmbCpuType;

        private bool gameOver = false;

        public MainForm()
        {
            Text = "ネットカードバトル - WinForms";
            ClientSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;

            InitControls();
            InitGame();
        }

        private void InitControls()
        {
            Label lblCpuSel = new Label();
            lblCpuSel.Text = "CPUタイプ:";
            lblCpuSel.Location = new Point(10, 10);
            lblCpuSel.AutoSize = true;
            Controls.Add(lblCpuSel);

            cmbCpuType = new ComboBox();
            cmbCpuType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCpuType.Items.Add("Aggro");
            cmbCpuType.Items.Add("Control");
            cmbCpuType.Items.Add("Midrange");
            cmbCpuType.SelectedIndex = 2;
            cmbCpuType.Location = new Point(80, 8);
            Controls.Add(cmbCpuType);

            Button btnRestart = new Button();
            btnRestart.Text = "リスタート";
            btnRestart.Location = new Point(200, 6);
            btnRestart.Click += delegate { InitGame(); };
            Controls.Add(btnRestart);

            lblHuman = new Label();
            lblHuman.Location = new Point(10, 40);
            lblHuman.AutoSize = true;
            Controls.Add(lblHuman);

            lblCpu = new Label();
            lblCpu.Location = new Point(10, 60);
            lblCpu.AutoSize = true;
            Controls.Add(lblCpu);

            Label lblHand = new Label();
            lblHand.Text = "手札";
            lblHand.Location = new Point(10, 90);
            lblHand.AutoSize = true;
            Controls.Add(lblHand);

            lstHand = new ListBox();
            lstHand.Location = new Point(10, 110);
            lstHand.Size = new Size(400, 150);
            Controls.Add(lstHand);

            btnPlayCard = new Button();
            btnPlayCard.Text = "選択カードを使用";
            btnPlayCard.Location = new Point(10, 270);
            btnPlayCard.Click += BtnPlayCard_Click;
            Controls.Add(btnPlayCard);

            btnEndTurn = new Button();
            btnEndTurn.Text = "ターン終了 / 攻撃";
            btnEndTurn.Location = new Point(150, 270);
            btnEndTurn.Click += BtnEndTurn_Click;
            Controls.Add(btnEndTurn);

            Label lblBoardHuman = new Label();
            lblBoardHuman.Text = "自分の場";
            lblBoardHuman.Location = new Point(10, 310);
            lblBoardHuman.AutoSize = true;
            Controls.Add(lblBoardHuman);

            lstBoardHuman = new ListBox();
            lstBoardHuman.Location = new Point(10, 330);
            lstBoardHuman.Size = new Size(400, 100);
            Controls.Add(lstBoardHuman);

            Label lblBoardCpu = new Label();
            lblBoardCpu.Text = "CPUの場";
            lblBoardCpu.Location = new Point(10, 440);
            lblBoardCpu.AutoSize = true;
            Controls.Add(lblBoardCpu);

            lstBoardCpu = new ListBox();
            lstBoardCpu.Location = new Point(10, 460);
            lstBoardCpu.Size = new Size(400, 100);
            Controls.Add(lstBoardCpu);

            txtLog = new TextBox();
            txtLog.Location = new Point(430, 10);
            txtLog.Size = new Size(450, 550);
            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.ReadOnly = true;
            Controls.Add(txtLog);
        }

        private void InitGame()
        {
            gameOver = false;
            turn = 1;
            txtLog.Clear();

            string playerName = "Player";
            human = new Player(playerName);

            CpuType type = CpuType.Midrange;
            if (cmbCpuType.SelectedIndex == 0) type = CpuType.Aggro;
            else if (cmbCpuType.SelectedIndex == 1) type = CpuType.Control;

            cpu = new CpuPlayer(type, Log);

            BuildBasicDeck(human);
            BuildBasicDeck(cpu);

            Shuffle(human.Deck);
            Shuffle(cpu.Deck);

            human.Draw(5);
            cpu.Draw(5);

            Log("=== ゲーム開始 ===");
            Log(human.Name + " vs " + cpu.Name);

            UpdateUI();
        }

        private void BtnPlayCard_Click(object sender, EventArgs e)
        {
            if (gameOver) return;
            if (lstHand.SelectedIndex < 0) return;

            Card card = human.Hand[lstHand.SelectedIndex];
            if (card.Cost > human.Energy)
            {
                Log("エネルギーが足りない！");
                return;
            }

            human.Energy -= card.Cost;
            human.Hand.RemoveAt(lstHand.SelectedIndex);

            if (card.Type == CardType.Unit)
            {
                Log("ユニット [" + card.Name + "] を召喚！");
                human.Board.Add(card);
            }
            else
            {
                Log("呪文 [" + card.Name + "] を発動！");
                CpuPlayer.ResolveSpell(card, human, cpu, Log);
            }

            CheckGameOver();
            UpdateUI();
        }

        private void BtnEndTurn_Click(object sender, EventArgs e)
        {
            if (gameOver) return;

            HumanAttackPhase();
            CheckGameOver();
            if (gameOver) { UpdateUI(); return; }

            CpuTurn();
            CheckGameOver();
            turn++;
            UpdateUI();
        }

        private void HumanAttackPhase()
        {
            if (human.Board.Count == 0)
            {
                Log("場にユニットがいない。攻撃できない。");
            }
            else
            {
                Log("攻撃フェイズ：自分の全ユニットで攻撃！");
                for (int i = 0; i < human.Board.Count; i++)
                {
                    Card unit = human.Board[i];
                    if (cpu.Board.Count > 0)
                    {
                        Card target = null;
                        int minHp = int.MaxValue;
                        for (int j = 0; j < cpu.Board.Count; j++)
                        {
                            if (cpu.Board[j].Health < minHp)
                            {
                                minHp = cpu.Board[j].Health;
                                target = cpu.Board[j];
                            }
                        }

                        Log("[" + unit.Name + "] が 相手ユニット [" + target.Name + "] を攻撃！ " + unit.Attack + " ダメージ");
                        target.Health -= unit.Attack;
                        unit.Health -= target.Attack;
                    }
                    else
                    {
                        Log("[" + unit.Name + "] が 本体 " + cpu.Name + " を攻撃！ " + unit.Attack + " ダメージ");
                        cpu.TakeDamage(unit.Attack);
                    }
                }
            }

            human.CleanupDeadUnits();
            cpu.CleanupDeadUnits();
        }

        private void CpuTurn()
        {
            cpu.TakeTurn(human, turn);
        }

        private void CheckGameOver()
        {
            if (human.HP <= 0 && cpu.HP <= 0)
            {
                Log("=== 引き分け！ ===");
                gameOver = true;
            }
            else if (human.HP <= 0)
            {
                Log("=== " + cpu.Name + " の勝ち！ ===");
                gameOver = true;
            }
            else if (cpu.HP <= 0)
            {
                Log("=== " + human.Name + " の勝ち！ ===");
                gameOver = true;
            }
        }

        private void UpdateUI()
        {
            lblHuman.Text = "自分: HP " + human.HP + " / EN " + human.Energy;
            lblCpu.Text = "CPU: " + cpu.Name + "  HP " + cpu.HP + " / EN " + cpu.Energy;

            lstHand.Items.Clear();
            for (int i = 0; i < human.Hand.Count; i++)
                lstHand.Items.Add(human.Hand[i].ToString());

            lstBoardHuman.Items.Clear();
            for (int i = 0; i < human.Board.Count; i++)
            {
                Card c = human.Board[i];
                lstBoardHuman.Items.Add(c.Name + " ATK " + c.Attack + " / HP " + c.Health);
            }

            lstBoardCpu.Items.Clear();
            for (int i = 0; i < cpu.Board.Count; i++)
            {
                Card c = cpu.Board[i];
                lstBoardCpu.Items.Add(c.Name + " ATK " + c.Attack + " / HP " + c.Health);
            }

            if (!gameOver)
            {
                human.Energy++; // ターン開始時エネルギー増加（人間側）
                human.Draw();
                Log("");
                Log("--- ターン " + turn + " 開始（自分） ---");
            }
        }

        private void Log(string msg)
        {
            txtLog.AppendText(msg + Environment.NewLine);
        }

        private static void BuildBasicDeck(Player p)
        {
            p.Deck.Clear();
            p.Deck.Add(new Card("HelloWorld プロセス", 1, 20, 20));
            p.Deck.Add(new Card("軽量スレッド", 2, 30, 30));
            p.Deck.Add(new Card("ガベージコレクタ", 3, 20, 60));
            p.Deck.Add(new Card("最適化コンパイラ", 4, 50, 50));
            p.Deck.Add(new Card("OSカーネル", 5, 70, 80));

            p.Deck.Add(new Card("BANコマンド", 3, 40, true));
            p.Deck.Add(new Card("物理削除バッチ", 4, 60, true));
            p.Deck.Add(new Card("DDoSアタック", 5, 80, false));
            p.Deck.Add(new Card("パッチ適用", 2, 30, true));
            p.Deck.Add(new Card("メモリリーク", 3, 50, false));

            List<Card> copy = new List<Card>();
            for (int i = 0; i < p.Deck.Count; i++)
                copy.Add(p.Deck[i].Clone());
            p.Deck.AddRange(copy);
        }

        private static void Shuffle<T>(IList<T> list)
        {
            Random rnd = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}