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

        TextView Data, Status;
        Button ARM, Connect, Disconnect, StartLogs;
        TextInputEditText FileName;
        BTUtil BTUtility;
        Spinner PhaseMenu;

        string[] Phases = { "Heel Contact", "Loading Response", "Mid Stance", "Terminal Swing", "Pre-Swing" };
        string Phase = "Heel Contact";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            //Get the UI elements as objects
            Connect = FindViewById<Button>(Resource.Id.Connect);
            Disconnect = FindViewById<Button>(Resource.Id.Disconnect);
            StartLogs = FindViewById<Button>(Resource.Id.StartLog);
            ARM = FindViewById<Button>(Resource.Id.ARM);
            Data = FindViewById<TextView>(Resource.Id.data);
            Status = FindViewById<TextView>(Resource.Id.Status);
            FileName = FindViewById<TextInputEditText>(Resource.Id.FileName);
            PhaseMenu = FindViewById<Spinner>(Resource.Id.PhaseMenu);

            //Set event methods for button clicks
            ARM.Click += OnARMClick;
            Connect.Click += OnConnectClick;
            Disconnect.Click += OnDisconnectClick;
            StartLogs.Click += OnStartLogClick;

            //Create instances of the supporting classes
            BTUtility = new BTUtil();

            Status.Text = "Not Connected";

            //Initalize the spinner phase choices
            PhaseMenu.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(OnPhaseSelect);
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, Phases);
            PhaseMenu.Adapter = adapter;

            //Allow scrolling through the status window
            Data.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();

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
            //Establish a bluetooth connection
            if(!BTUtility.IsConnected)
            {
                if (BTUtility.Connect())
                {
                    Data.Append("\nConnected");
                    Status.Text = "Connected";
                }
                else
                {
                    Data.Append("\nCould not connect");
                }
            }
        }

        private void OnDisconnectClick(object sender, EventArgs eventArgs)
        {
            BTUtility.Disconnect();
            Data.Append("\nSlip Shoe now disconnected");
            Status.Text = "Not Connected";
        }

        private void OnStartLogClick(object sender, EventArgs eventArgs)
        {
            //Just in case, create the SlipShoeTrials directory
            FileUtil.CreateDirectory();

            //if connected check file name then start BTThread
            if(BTUtility.IsConnected)
            {
                char InvalidChar = FileUtil.CheckFileName(FileName.Text);

                if(InvalidChar == 'O')
                {
                    if(FileName.Text.EndsWith(".csv"))
                    {
                        if(FileUtil.CreateFile(FileName.Text))
                        {
                            //Set the filename and start logging
                            BTUtility.FileName = FileName.Text;
                            BTUtility.StartLogging();
                            Status.Text = "Logging";
                            Data.Append("\nNow logging to file: " + FileName.Text);
                        }
                        else
                        {
                            Data.Append("\nError: FileName already exists");
                        }
                    }
                    else
                    {
                        Data.Append("\nError: FileName must end in \".csv\"");
                    }
                }
                else
                {
                    Data.Append("\nError: FileName contains invalid character: " + InvalidChar);
                }
            }
            else
            {
                Data.Append("\nError: Connect bluetooth to start logging");
            }
        }

        private void OnARMClick(object sender, EventArgs eventArgs)
        {
            if(FileName.Text.EndsWith(".csv"))
            {
                for (int i = 0; i < Phases.Length; i++)
                {
                    if (Phase.Equals(Phases[i]))
                    {
                        BTUtility.Send(i.ToString());
                        Data.Append("\nDevice armed for phase: " + Phases[i]);
                    }
                }
            }
            else
            {
                Data.Append("\nFile Name must end in .csv!");
            }
        }

        private void OnPhaseSelect(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //Set the global string to the selected phase
            Phase = PhaseMenu.GetItemAtPosition(e.Position).ToString();
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

