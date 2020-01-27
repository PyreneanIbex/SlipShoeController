using System;
using System.Linq;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Java.Util;
using System.Text;
using System.IO;
using Android;
using Android.Content.PM;

namespace SlipShoeController
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        TextView Data;
        Button Send, Connect, CreateFile;
        TextInputEditText MessageToSend;
        TextInputEditText FileName;
        BluetoothAdapter BTAdapter = BluetoothAdapter.DefaultAdapter;
        BluetoothDevice BTDevice;
        BluetoothSocket BTSocket;
        Thread BTThread;

        bool Connected = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            //Get the UI elements as objects
            MessageToSend = FindViewById<TextInputEditText>(Resource.Id.MessageToSend);
            Send = FindViewById<Button>(Resource.Id.Send);
            Connect = FindViewById<Button>(Resource.Id.Connect);
            CreateFile = FindViewById<Button>(Resource.Id.CreateFile);
            Data = FindViewById<TextView>(Resource.Id.data);
            FileName = FindViewById<TextInputEditText>(Resource.Id.FileName);

            //Set event methods for button clicks
            Send.Click += OnSendClick;
            Connect.Click += OnConnectClick;
            CreateFile.Click += OnCreateFileClick;

            //Ask for permission to work with files
            if (PackageManager.CheckPermission(Manifest.Permission.ReadExternalStorage, PackageName) != Permission.Granted
                && PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, PackageName) != Permission.Granted)
            {
                var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage };
                RequestPermissions(permissions, 1);
            }
        }

        //Open the connection to the bluetooth device
        private void OnConnectClick(object sender, EventArgs eventArgs)
        {
            Data.Append("\nConnecting\n");

            //Find the desired module from the list
            BTDevice = (from bd in BTAdapter.BondedDevices where bd.Name == "DSD TECH HC-05" select bd).FirstOrDefault();

            if(!Connected)
            {
                //Create a socket to communicate through
                BTSocket = BTDevice.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                BTSocket.Connect();

                //Start the background thread listening for data incoming from bluetooth
                BTThread = new Thread(() => { BluetoothListener(); });
                BTThread.Start();
                Connected = true;
            }
            
        }

        private void BluetoothListener()
        {
            try
            {
                var InputStream = (BTSocket.InputStream as InputStreamInvoker).BaseInputStream;

                while (true)
                {                

                    if(InputStream.Available() > 0)
                    {
                        byte[] buf = new byte[InputStream.Available()];

                        InputStream.Read(buf);

                        string data = Encoding.UTF8.GetString(buf);

                        if (data.Length > 0)
                        {
                            this.RunOnUiThread(() =>
                            {
                                Data.Text += data;
                            });
                        }
                        Thread.Sleep(100);
                    }
                }
            }
            catch (ThreadAbortException) { }
        }

        private void OnSendClick(object sender, EventArgs eventArgs)
        {
            Data.Append("\nSending: " + MessageToSend.Text + "\n");

            byte[] buffer = Encoding.UTF8.GetBytes(MessageToSend.Text);

            BTSocket.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private void OnCreateFileClick(object sender, EventArgs eventArgs)
        {
            string DirPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

            Directory.CreateDirectory(Path.Combine(DirPath, "SlipShoeTrials"));

            var fs = File.Create(Path.Combine(DirPath, "SlipShoeTrials", FileName.Text));
            fs.Close();

            File.WriteAllText(Path.Combine(DirPath, "SlipShoeTrials", FileName.Text), "wut");
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        //private void FabOnClick(object sender, EventArgs eventArgs)
        //{
        //    View view = (View) sender;
        //    Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
        //        .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        //}
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}

