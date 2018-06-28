using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Clans.Fab;
using LoginApp.Helpers;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace LoginApp
{
    [Activity(Label = "Home", Theme = "@style/MyTheme")]
    public class HomeActivity : AppCompatActivity
    {
        private SupportToolbar _toolbar;
        private FloatingActionButton _fabConnect;
        private FloatingActionButton _fabSend;
        private FloatingActionButton _fabLogout;
        private EditText _txtDevice;
        private EditText _txtCommand;
        private TextView _txtWelcome;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.home_layout);

            Base.TrackEvent("Logged in");

            SetupFields();
        }

        private void SetupFields()
        {
            _toolbar = FindViewById<SupportToolbar>(Resource.Id.tbHome);
            _txtWelcome = FindViewById<TextView>(Resource.Id.txtWelcome);
            _txtDevice = FindViewById<EditText>(Resource.Id.txtDevice);
            _txtCommand = FindViewById<EditText>(Resource.Id.txtCommand);
            _fabConnect = FindViewById<FloatingActionButton>(Resource.Id.fabConnect);
            _fabSend = FindViewById<FloatingActionButton>(Resource.Id.fabSend);
            _fabLogout = FindViewById<FloatingActionButton>(Resource.Id.fabLogout);

            _txtWelcome.Text = $"Olá, {Base.Instance.User.Name}";
            _fabConnect.Click += Connect_Click;
            _fabSend.Click += Send_Click;
            _fabLogout.Click += Logout_Click;

            SetSupportActionBar(_toolbar);
        }

        private void Connect_Click(object sender, System.EventArgs e)
        {

        }

        private void Send_Click(object sender, System.EventArgs e)
        {

        }

        private void Logout_Click(object sender, System.EventArgs e)
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }
    }
}

