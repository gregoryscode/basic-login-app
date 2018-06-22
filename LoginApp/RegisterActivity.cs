using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Clans.Fab;
using LoginApp.Helpers;
using LoginApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace LoginApp
{
    [Activity(Label = "Cadastre-se", Theme = "@style/MyTheme")]
    public class RegisterActivity : AppCompatActivity
    {
        private SupportToolbar _toolbar;
        private EditText _txtName;
        private EditText _txtUsername;
        private EditText _txtPassword;
        private EditText _txtConfirmPassword;
        private FloatingActionButton _fabSave;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register_layout);

            Base.TrackEvent("Register activity opened");

            SetupFields();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            switch (id)
            {
                case Android.Resource.Id.Home:
                    {
                        Finish();
                        break;
                    }
            }

            return true;
        }

        private void SetupFields()
        {
            _toolbar = FindViewById<SupportToolbar>(Resource.Id.tbRegister);
            _txtName = FindViewById<EditText>(Resource.Id.txtRegisterName);
            _txtUsername = FindViewById<EditText>(Resource.Id.txtRegisterUsername);
            _txtPassword = FindViewById<EditText>(Resource.Id.txtRegisterPassword);
            _txtConfirmPassword = FindViewById<EditText>(Resource.Id.txtRegisterConfirmPassword);
            _fabSave = FindViewById<FloatingActionButton>(Resource.Id.fabRegister);
            _fabSave.Click += Save_Click;

            SetSupportActionBar(_toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        private void Save_Click(object sender, System.EventArgs e)
        {
            try
            {
                List<User> users = new List<User>();
                int id = 1;
                string name = _txtName.Text;
                string username = _txtUsername.Text;
                string password = _txtPassword.Text;
                string confirmPassword = _txtConfirmPassword.Text;

                if(string.IsNullOrEmpty(name))
                {
                    ShowMessage("O campo 'Nome' é obrigatório.");
                    return;
                }

                if(string.IsNullOrEmpty(username))
                {
                    ShowMessage("O campo 'Usuário' é obrigatório.");
                    return;
                }

                if(string.IsNullOrEmpty(password))
                {
                    ShowMessage("O campo 'Senha' é obrigatório.");
                    return;
                }

                if(string.IsNullOrEmpty(confirmPassword))
                {
                    ShowMessage("O campo 'Confirmar senha' é obrigatório.");
                    return;
                }

                if(!password.Equals(confirmPassword))
                {
                    ShowMessage("A confirmação de senha é diferente da senha.");
                    return;
                }

                users = Base.GetUsers();

                if(users != null)
                {
                    id = users.Max(g => g.ID) + 1;                    
                    users.Add(new User() { ID = id, Name = name, Username = username, Password = password });
                }            
                else
                {
                    users = new List<User>();
                    users.Add(new User() { ID = id, Name = name, Username = username, Password = password });
                }

                if(Base.RegisterUser(users))
                {
                    ShowMessage("Usuário cadastrado com sucesso!");
                    Finish();
                    return;
                }

                ShowMessage("Não foi possível cadastrar o seu usuário. Por favor, tente novamente. Se o prolema persistir, entre em contato com os desenvolvedores.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Ocorreu um erro ao cadastrar o usuário: {ex.Message}");
                Base.TrackError(ex);
            }
        }

        private void ShowMessage(string message)
        {
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }
    }
}