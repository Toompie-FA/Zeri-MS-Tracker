namespace ZeriMS_tracker
{
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public partial class Form1 : Form
    {
        private readonly HttpClient _http;
        private readonly System.Windows.Forms.Timer _timer;
        private double _lastHealth;
        private bool _GameStart;
        private double _peakMoveSpeed;
        private double _lastgold;
        private string PlayerRiotID;
        private bool _InBase = false;
        private bool _HealthInfluxed;
        private bool _DevMode = true;
        private double respawnTimer;
        // true = the dev labels are showing

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
            _timer = new System.Windows.Forms.Timer { Interval = 500 };
            _timer.Tick += Timer_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            await UpdateMoveSpeedAsync();
        }
        private async Task UpdateMoveSpeedAsync()
        {
            bool isDead = false;            
            //try
            {
                //get player stats
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

                try
                {
                    PlayerRiotID = doc.RootElement
                    .GetProperty("riotId")
                    .GetString();
                }
                catch
                {
                }

                //checking if dead and then respawn timer
                string listJson = await _http.GetStringAsync("https://127.0.0.1:2999/liveclientdata/playerlist");
                using JsonDocument listDoc = JsonDocument.Parse(listJson);

                foreach (JsonElement player in listDoc.RootElement.EnumerateArray())
                {
                    if (player.GetProperty("riotId").GetString() == PlayerRiotID)
                    {
                        isDead = !player.GetProperty("isDead").GetBoolean();
                        respawnTimer = player.GetProperty("respawnTimer").GetDouble();
                    }
                }

                //get game stats
                string statsJson = await _http.GetStringAsync("https://127.0.0.1:2999/liveclientdata/gamestats");
                using JsonDocument statsDoc = JsonDocument.Parse(statsJson);

                double GameTime = statsDoc.RootElement
                    .GetProperty("gameTime")
                    .GetDouble();
                // FIXED: save the gold straight away so an error further down
                // can't leave _lastgold stuck on an old value
                double previousGold = _lastgold;
                _lastgold = gold;

                try
                {
                    _HealthInfluxed = (CurrentHealth - _lastHealth) > MaxHealth * 0.03;
                }
                catch
                {

                }
                _lastHealth = CurrentHealth;

                switch (true)
                {
                    case true when previousGold > gold:
                        _InBase = true;
                        label5.Text = "Gold";
                        return;
                    case true when _HealthInfluxed:
                        _InBase = true;
                        label5.Text = "Health";
                        return;
                    case true when GameTime <= 35:
                        _InBase = true;
                        label5.Text = "GameTime";
                        return;
                    case true when respawnTimer > 0:
                        label5.Text = $"{respawnTimer:F1}";
                        //Thread.Sleep((int)(respawnTimer * 1000));
                        return;
                    default:
                        label5.Text = "nothing";
                        break;
                }

                if (_InBase == true)
                {
                    if(GameTime > 840)
                    {
                        Thread.Sleep(7000);
                    }
                    else
                    {
                        Thread.Sleep(13000);
                    }
                    _InBase = false;
                }


                // FIXED: don't record a peak on the tick where you just bought something
                if (moveSpeed > _peakMoveSpeed)
                {
                    _peakMoveSpeed = moveSpeed;
                    label1.Text = $"PMS: {moveSpeed:F1}";
                }

                label2.Text = $"MS: {moveSpeed:F1}";

                label3.Text = $"LG:{previousGold:F1}";
                label4.Text = $"CG:{gold:F1}";
                label6.Text = $"{GameTime}";
            }
            //catch (Exception ex)
            {
                //_GameStart = false;
                //_InBase = false;    // between games
                //label2.Text = "Waiting"; // or ex.Message while debugging
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.TopMost = true;
            // FIXED: use this.Width, because Form1.ActiveForm is null
            // when the form isn't the active window
            if (_DevMode == true)
            {
                label3.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
                this.Width = this.Width / 2;
                _DevMode = false;
            }
            else
            {
                label3.Visible = true;
                label4.Visible = true;
                label5.Visible = true;
                label6.Visible = true;
                this.Width = this.Width * 2;
                _DevMode = true;
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            _timer.Start();
        }
    }

}
