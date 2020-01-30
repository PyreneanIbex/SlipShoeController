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
        BTUtil BTUtility;

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

            //Create instances of the supporting classes
            BTUtility = new BTUtil();

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
            FileUtil.CreateFile(FileName.Text);
            BTUtility.FileName = FileName.Text;

            //Establish a bluetooth connection
            if (BTUtility.Connect() && !Connected)
            {
                Data.Append("Connected\n");
                Connected = true;
            }
            else
            {
                Data.Append("Could not connect");
                Connected = false;
            }     
        }

        private void OnSendClick(object sender, EventArgs eventArgs)
        {
            //Send data through bluetooth
            BTUtility.Send(MessageToSend.Text);
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

