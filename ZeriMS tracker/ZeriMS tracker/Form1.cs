namespace ZeriMS_tracker
{
    using System.Net.Http;
    using System.Text.Json;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        private readonly HttpClient _http;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly System.Windows.Forms.Timer _BaseTimer;

        private bool _GameStart;
        private double _peakMoveSpeed;
        private double _lastgold;
        private bool _InBase = false;
        private bool _DevMode = true; // true = the dev labels are showing

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
            _timer.Tick += Timer_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            label5.Text = $"S: {_timer.Interval}";
            await UpdateMoveSpeedAsync();
        }
        private async Task UpdateMoveSpeedAsync()
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

                // FIXED: save the gold straight away so an error further down
                // can't leave _lastgold stuck on an old value
                double previousGold = _lastgold;
                _lastgold = gold;

                if (previousGold > gold)
                {

                }
                else
                {
                    //_InBase = false;
                }

                _GameStart = true; // we got data, so a game is running

                // FIXED: don't record a peak on the tick where you just bought something
                if (moveSpeed > _peakMoveSpeed)
                {
                    _peakMoveSpeed = moveSpeed;
                    label1.Text = $"PMS: {moveSpeed:F1}";
                }

                label2.Text = $"MS: {moveSpeed:F1}";

                label3.Text = $"LG:{previousGold:F1}";
                label4.Text = $"CG:{gold:F1}";
                label6.Text = $"Base: {_InBase}";
            }
            catch (Exception ex)
            {
                _GameStart = false;
                _InBase = false;          // FIXED: don't stay stuck on a long timer
                _timer.Interval = 500;    // between games
                label2.Text = "Waiting"; // or ex.Message while debugging
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
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
