using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android.Bluetooth;
using Java.Lang.Reflect;
using Java.Util;
using LoginApp.Models;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using static LoginApp.Helpers.Constants;

namespace LoginApp.Helpers
{
    public class Base
    {
        public static readonly string APP_FOLDER_PATH = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "LoginApp");
        public static readonly string USER_DATABASE_FILE = "userdatabase.json";
        private static readonly UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private static Base _instance;

        private Stream _outputStream;
        private Stream _inputStream;

        public User User { get; set; }
        public Status BluetoothStatus { get; set; }
        private BluetoothDevice SelectedBluetooth { get; set; }
        private BluetoothSocket BluetoothSocket { get; set; }

        public static Base Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Base();
                }

                return _instance;

            }
        }

        public static void TrackEvent(string eventName, Dictionary<string, string> properties = null)
        {
            Analytics.TrackEvent(eventName, properties);
        }

        public static void TrackError(Exception ex, Dictionary<string, string> properties = null)
        {
            Crashes.TrackError(ex, properties);
        }

        public static void CreateSetupFolders()
        {
            try
            {
                Directory.CreateDirectory(APP_FOLDER_PATH);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool RegisterUser(List<User> users)
        {
            try
            {
                string json = JsonConvert.SerializeObject(users);
                string filename = Path.Combine(APP_FOLDER_PATH, USER_DATABASE_FILE);

                using (var streamWriter = new StreamWriter(filename, false))
                {
                    streamWriter.WriteLine(json);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<User> GetUsers()
        {
            try
            {
                string file = Path.Combine(APP_FOLDER_PATH, USER_DATABASE_FILE);
                string json = null;

                if (!File.Exists(file))
                {
                    return null;
                }

                using (var streamReader = new StreamReader(file))
                {
                    json = streamReader.ReadToEnd();
                }

                return JsonConvert.DeserializeObject<List<User>>(json);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Ativa o bluetooth
        /// </summary>
        /// <param name="enable">Ativar (true) | Desativar (false)</param>
        /// <returns></returns>
        public static bool TurnOnBluetooth(bool enable)
        {
            try
            {
                BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

                if (bluetoothAdapter == null)
                {
                    return false;
                }

                if (enable && !bluetoothAdapter.IsEnabled)
                {
                    return bluetoothAdapter.Enable();
                }
                else if (!enable && bluetoothAdapter.IsEnabled)
                {
                    return bluetoothAdapter.Disable();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Conecta com um dispositivo bluetooth
        /// </summary>
        /// <param name="device">Nome do dispositivo</param>
        /// <returns></returns>
        public async Task<bool> ConnectToDevice(string device)
        {
            try
            {
                if (BluetoothStatus == Status.Connected)
                {
                    return true;
                }

                BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

                if (adapter == null)
                {
                    return false;
                }

                if (!TurnOnBluetooth(true))
                {
                    return false;
                }

                await Task.Delay(3000);

                adapter.CancelDiscovery();

                var pairedDevices = adapter.BondedDevices;

                foreach (var paired in pairedDevices)
                {
                    if (paired.Name.Equals(device))
                    {
                        SelectedBluetooth = paired;
                    }
                }

                if (SelectedBluetooth == null)
                {
                    return false;
                }

                // Buscamos o socket bluetooth para realizar a conexão com a placa bluetooth
                BluetoothSocket = SelectedBluetooth.CreateRfcommSocketToServiceRecord(MY_UUID);

                // Conectamos ao socket
                BluetoothSocket.Connect();

                // Atribui os streams do socket bluetooth
                _outputStream = BluetoothSocket.OutputStream;
                _inputStream = BluetoothSocket.InputStream;

                // Armazena que o bluetooth está conectado
                BluetoothStatus = Status.Connected;

                return BluetoothSocket.IsConnected;
            }
            catch (Exception)
            {
                // Fallback
                try
                {
                    BluetoothSocket = GetBluetoothSocketByInvoke();

                    BluetoothSocket.Connect();

                    _outputStream = BluetoothSocket.OutputStream;
                    _inputStream = BluetoothSocket.InputStream;

                    BluetoothStatus = Status.Connected;

                    return BluetoothSocket.IsConnected;
                }
                catch (Exception)
                {
                    // Fallback 2
                    try
                    {
                        BluetoothSocket = GetBluetoothInsecureSocketByInvoke();

                        BluetoothSocket.Connect();

                        _outputStream = BluetoothSocket.OutputStream;
                        _inputStream = BluetoothSocket.InputStream;

                        BluetoothStatus = Status.Connected;

                        return BluetoothSocket.IsConnected;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            DisconnectFromDevice();
                        }
                        catch (Exception)
                        {
                            // Não implementado
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Desconecta da placa bluetooth
        /// </summary>
        /// <returns></returns>
        public bool DisconnectFromDevice()
        {
            try
            {
                if (BluetoothSocket != null)
                {
                    SelectedBluetooth = null;

                    BluetoothSocket.Close();
                    BluetoothSocket.Dispose();

                    if (_outputStream != null)
                    {
                        _outputStream.Close();
                        _outputStream.Dispose();
                    }

                    if (_inputStream != null)
                    {
                        _inputStream.Close();
                        _inputStream.Dispose();
                    }

                    // Altera o status para desconectado
                    BluetoothStatus = Status.Disconnected;
                }

                // Desativa o bluetooth
                if (!TurnOnBluetooth(false))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                // Não implementado
            }

            return false;
        }

        /// <summary>
        /// Envia uma mensagem via bluetooth
        /// </summary>
        /// <param name="message">Mensagem para ser enviada</param>
        /// <returns></returns>
        public bool SendData(string message)
        {
            try
            {
                // Converte a mensagem para um array de bytes
                var buffer = Encoding.ASCII.GetBytes(message);

                // Envia a mensagem para a placa bluetooth
                _outputStream.Write(buffer, 0, buffer.Length);

                return true;
            }
            catch (Exception)
            {
                // Não implementado
            }

            return false;
        }

        /// <summary>
        /// Recebe os dados enviados via bluetooth
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReceiveData()
        {
            byte[] buffer = new byte[30];

            try
            {
                if (_inputStream.IsDataAvailable())
                {
                    int read = await _inputStream.ReadAsync(buffer, 0, buffer.Length);

                    if (read <= 0)
                    {
                        return null;
                    }

                    char[] result = Encoding.ASCII.GetChars(buffer);

                    if (!result[0].Equals("\0"))
                    {
                        return result[0].ToString();
                    }
                }
            }
            catch (Exception)
            {
                // Não implementado
            }

            return null;
        }

        /// <summary>
        /// Busca o socket bluetooth através do invoke direto no método
        /// </summary>
        /// <returns></returns>
        private BluetoothSocket GetBluetoothSocketByInvoke()
        {
            try
            {
                Method[] methods = SelectedBluetooth.Class.GetMethods();

                foreach (var method in methods)
                {
                    if (method.Name.Equals("createRfcommSocket"))
                    {
                        try
                        {
                            return (BluetoothSocket)method.Invoke(SelectedBluetooth, 1);
                        }
                        catch
                        {
                        }

                        return null;
                    }
                }
            }
            catch (Exception)
            {
                // Não implementado
            }

            return null;
        }

        /// <summary>
        /// Busca o socket inseguro bluetooth através do invoke direto no método
        /// </summary>
        /// <returns></returns>
        private BluetoothSocket GetBluetoothInsecureSocketByInvoke()
        {
            try
            {
                Method[] methods = SelectedBluetooth.Class.GetMethods();

                foreach (var method in methods)
                {
                    if (method.Name.Equals("createInsecureRfcommSocketToServiceRecord"))
                    {
                        try
                        {
                            return (BluetoothSocket)method.Invoke(SelectedBluetooth, MY_UUID);
                        }
                        catch
                        {
                        }

                        return null;
                    }
                }
            }
            catch (Exception)
            {
                // Não implementado
            }

            return null;
        }
    }
}
