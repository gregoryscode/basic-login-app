using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Clans.Fab;
using LoginApp.Helpers;
using System;
using System.Threading.Tasks;
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

        private async void Connect_Click(object sender, EventArgs e)
        {
            ProgressDialog progressDialog = null;

            try
            {
                progressDialog = ProgressDialog.Show(this, "Aguarde", "Estabelecendo conexão...", true);

                if (Base.TurnOnBluetooth(true))
                {
                    string device = _txtDevice.Text;

                    if (string.IsNullOrEmpty(device))
                    {
                        Toast.MakeText(this, $"Dispositivo inválido.", ToastLength.Short).Show();
                        return;
                    }

                    await Task.Factory.StartNew(async () =>
                    {
                        if (await Base.Instance.ConnectToDevice(device.ToUpper()))
                        {
                            RunOnUiThread(() => {
                                // _txtBluetoothStatus.Text = GetString(Resource.String.app_connected);
                                // _txtBluetoothStatus.SetTextColor(Color.ParseColor("#008B45"));

                                progressDialog.Hide();
                                Toast.MakeText(this, $"Conexão realizada com sucesso.", ToastLength.Short).Show();
                            });
                        }
                        else
                        {
                            progressDialog.Hide();
                            Toast.MakeText(this, $"Não foi possível realizar a conexão com o dispositivo.", ToastLength.Short).Show();
                        }
                    });
                }
                else
                {
                    progressDialog.Hide();
                    Toast.MakeText(this, $"Não foi possível ativar o bluetooth.", ToastLength.Short).Show();
                }

            }
            catch (Exception ex)
            {
                progressDialog.Hide();
                Toast.MakeText(this, $"Não foi possível conectar com o dispositivo. Erro: {ex.Message}", ToastLength.Short).Show();
            }
        }

        private void Send_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    string message = _txtCommand.Text;

                    if (string.IsNullOrEmpty(message))
                    {
                        RunOnUiThread(() => { Toast.MakeText(this, $"Escreva uma mensagem para ser enviada.", ToastLength.Short).Show(); });
                    }

                    if (Base.Instance.SendData(message))
                    {
                        RunOnUiThread(() => { Toast.MakeText(this, $"Mensagem enviada com sucesso.", ToastLength.Short).Show(); });
                    }
                    else
                    {
                        RunOnUiThread(() => { Toast.MakeText(this, $"A mensagem não foi enviada.", ToastLength.Short).Show(); });
                    }

                    // Aguarda para verificar os dados retornados pelo bluetooth
                    await Task.Delay(1000);

                    string result = await Base.Instance.ReceiveData();

                    RunOnUiThread(() => { Toast.MakeText(this, $"Recebido: {result ?? "Nenhum dado recebido."}", ToastLength.Short).Show(); });
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() => { Toast.MakeText(this, $"Erro ao enviar mensagem. Erro: {ex.Message}", ToastLength.Short).Show(); });
                }
            });
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }
    }
}

