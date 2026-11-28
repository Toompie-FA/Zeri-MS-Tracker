namespace ZeriMS_tracker
{
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public partial class Form1 : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        private readonly HttpClient _http;
        private readonly System.Windows.Forms.Timer _timer;
        private double _lastHealth;
        private bool _GameStart;
        private double _peakMoveSpeed;
        private double _lastgold;
        private string _PlayerRiotID;
        private bool _InBase = false;
        private bool _HealthInfluxed;
        private bool _DevMode = true;
        // true = the dev labels are showing
        private double _respawnTimer;
        private int _StartTime = 35;
        private double _FountainHealing = 0.03;
        private int _HomeguardUpgradeTime = 840;
        private int _WalkBackTime = 17;
        private bool isDead;
        private int _WalkBackTimeLate = 8;
        private double _previousGold;
        private Size _NormalSize;
        private double _previousResource;
        private record GameSnapshot(
             double _MoveSpeed, double _Gold, double _CurrentHealth,
             double _MaxHealth, double _GameTime, double _RespawnTimer, double Resource, double _MaxMana, double _CurrentMana);
        private DateTime _ignoreUntil = DateTime.MinValue;

        private void StartHomeguardIgnore(double gameTime)
        {
            label5.Text = "Active";
            _ignoreUntil = DateTime.Now.AddSeconds(gameTime > _HomeguardUpgradeTime ? _WalkBackTimeLate : _WalkBackTime);
        }

        private bool ShouldIgnoreReadings()
        {
            return DateTime.Now < _ignoreUntil;
        }
        public Form1()
        {
            InitializeComponent();

            // The game's local server uses a self-signed certificate, so accept it
            // only when the request goes to 127.0.0.1.
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) =>
                    msg.RequestUri?.Host == "127.0.0.1"
            };
            _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(2) };
            _timer = new System.Windows.Forms.Timer { Interval = 3000 };
            _timer.Tick += Timer_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShrinkForm();
            _timer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            this.TopMost = true;
            await UpdateMoveSpeedAsync();
            bool pHeld = (GetAsyncKeyState((int)Keys.P) & 0x8000) != 0;
            if (pHeld == true)
            {
                await Task.Delay(1000);
                ReturnForm();
            }
            else
            {
                ShrinkForm();
            }
        }

        private void ShrinkForm()
        {
            _NormalSize = this.ClientSize;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            label6.Visible = false;
            label1.AutoSize = true;
            label1.Location = new Point(0, 0);
            this.ClientSize = label1.PreferredSize;
        }

        private void ReturnForm()
        {
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            label5.Visible = true;
            label6.Visible = true;
            this.ClientSize = _NormalSize;
        }

        private bool CheckInBase(GameSnapshot data)
        {
            _previousGold = _lastgold;                       // last tick's gold
            bool boughtSomething = _previousGold > data._Gold; // did gold go down?
            _lastgold = data._Gold;
            _HealthInfluxed = (data._CurrentHealth - _lastHealth) > data._MaxHealth * _FountainHealing;
            _lastHealth = data._CurrentHealth;
            bool earlyGame = data._GameTime <= 35; //only way to detect starting homeguard
            bool ResourceUsed = data.Resource < _previousResource; //if used mana
            _previousResource = data.Resource;
            return boughtSomething || _HealthInfluxed || earlyGame || _respawnTimer > 0 || !ResourceUsed;
        }

        private bool InFight(GameSnapshot data)
        {
            bool ManaUsed = data._MaxMana > data._CurrentMana;
            return ManaUsed;
        }

        private async Task<GameSnapshot> GetGameSnapshotAsync()
        {
            try
            {
                string json = await _http.GetStringAsync("https://127.0.0.1:2999/liveclientdata/activeplayer");
                using JsonDocument doc = JsonDocument.Parse(json);

                double moveSpeed = doc.RootElement
                    .GetProperty("championStats")
                    .GetProperty("moveSpeed")
                    .GetDouble();
                double gold = doc.RootElement
                    .GetProperty("currentGold")
                    .GetDouble();
                double CurrentHealth = doc.RootElement
                    .GetProperty("championStats")
                    .GetProperty("currentHealth")
                    .GetDouble();
                double MaxHealth = doc.RootElement
                    .GetProperty("championStats")
                    .GetProperty("maxHealth")
                    .GetDouble();
                double MaxMana = doc.RootElement
                    .GetProperty("championStats")
                    .GetProperty("maxMana")
                    .GetDouble();
                double CurrentMana = doc.RootElement
                    .GetProperty("championStats")
                    .GetProperty("currentMana")
                    .GetDouble();
                _PlayerRiotID = doc.RootElement
                        .GetProperty("riotId")
                        .GetString();
                double Resource = doc.RootElement
                    .GetProperty("championStats")
                    .GetProperty("resourceValue")
                    .GetDouble();

                //get game time through game stats endpoint, because the activeplayer endpoint doesn't have it
                string statsJson = await _http.GetStringAsync("https://127.0.0.1:2999/liveclientdata/gamestats");
                using JsonDocument statsDoc = JsonDocument.Parse(statsJson);

                //checking if dead and then respawn timer
                string listJson = await _http.GetStringAsync("https://127.0.0.1:2999/liveclientdata/playerlist");
                using JsonDocument listDoc = JsonDocument.Parse(listJson);

                double _GameTime = statsDoc.RootElement
                    .GetProperty("gameTime")
                    .GetDouble();

                foreach (JsonElement player in listDoc.RootElement.EnumerateArray())
                {
                    if (player.GetProperty("riotId").GetString() == _PlayerRiotID)
                    {
                        isDead = !player.GetProperty("isDead").GetBoolean();
                        _respawnTimer = player.GetProperty("respawnTimer").GetDouble();
                    }
                }

                return new GameSnapshot(moveSpeed, gold, CurrentHealth, MaxHealth, _GameTime, _respawnTimer, Resource, MaxMana, CurrentMana);
            }
            catch (HttpRequestException)
            {
                // League isn't running (or the game just ended): reset
                _peakMoveSpeed = 0;
                label1.Text = "PMS: 0.0";
                return new GameSnapshot(0, 0, 0, 0, 0, 0, 0, 0, 0);
            }
            catch (Exception)
            {
                // timeout or odd data mid-game: keep the peak
                return new GameSnapshot(0, 0, 0, 0, 0, 0, 0, 0, 0);
            }
        }
        private async Task UpdateMoveSpeedAsync()
        {
            GameSnapshot data = await GetGameSnapshotAsync();
            UpdateLabels(data);

            if (CheckInBase(data))
            {
                _InBase = true;
                return;
            }

            if (_InBase) // you just left base
            {
                StartHomeguardIgnore(data._GameTime);
                _InBase = false;
            }

            if (ShouldIgnoreReadings() || data._RespawnTimer > 0) return;

            if (data._MoveSpeed > _peakMoveSpeed && InFight(data))
            {
                _peakMoveSpeed = data._MoveSpeed;
                label1.Text = $"PMS: {data._MoveSpeed:F1}";
            }
        }

        private void UpdateLabels(GameSnapshot data)
        {
            label2.Text = $"MS: {data._MoveSpeed:F1}";
            label3.Text = $"LG:{_previousGold:F1}";
            label4.Text = $"CG:{data._Gold:F1}";
            label6.Text = $"{data._GameTime}";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }
    }
}