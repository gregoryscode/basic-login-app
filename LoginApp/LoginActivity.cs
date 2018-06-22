using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Widget;
using LoginApp.Helpers;
using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using System.Collections.Generic;

namespace LoginApp
{
    [Activity(Label = "LoginApp", Theme = "@style/AppThemeLogin", MainLauncher = true, Icon = "@drawable/icon_app")]
    public class LoginActivity : AppCompatActivity
    {

        private const int REQUEST_PERMISSIONS = 1;
        readonly string[] PERMISSIONS_REQUIRED =
        {
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.Internet,
        };

        private Button _btnLogin;
        private EditText _txtUsername;
        private EditText _txtPassword;
        private TextView _txtRegister;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.login_layout);

            StartAppCenter();

            Base.TrackEvent("App launched");

            try
            {
                SetupFields();
                CheckPermissions();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ocorreu um erro: {ex.Message}");
                Base.TrackError(ex);
            }
        }

        private void StartAppCenter()
        {
            Push.SetSenderId("1022787026315");
            AppCenter.Start("793b0912-c637-49fd-9592-3f19b613057c", typeof(Analytics), typeof(Crashes), typeof(Push));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            try
            {
                CheckPermissions();
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void SetupFields()
        {
            _txtUsername = FindViewById<EditText>(Resource.Id.txtUsername);
            _txtPassword = FindViewById<EditText>(Resource.Id.txtPassword);
            _txtRegister = FindViewById<TextView>(Resource.Id.txtRegister);
            _btnLogin = FindViewById<Button>(Resource.Id.btnLogin);

            _txtRegister.Click += Register_Click;
        }

        private void CheckPermissions()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    SetupApplication();
                    return;
                }

                if (!HasPermissions(PERMISSIONS_REQUIRED))
                {
                    // Requisitamos as permissões
                    ActivityCompat.RequestPermissions(this, PERMISSIONS_REQUIRED, REQUEST_PERMISSIONS);
                }
                // Permissões foram autorizadas. Pode continuar.
                else
                {
                    SetupApplication();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool HasPermissions(string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (ContextCompat.CheckSelfPermission(this, permission) != Permission.Granted)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetupApplication()
        {
            try
            {
                Base.CreateSetupFolders();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Register_Click(object sender, System.EventArgs e)
        {
            StartActivity(typeof(RegisterActivity));
        }

        private void ShowMessage(string message)
        {
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }
    }
}