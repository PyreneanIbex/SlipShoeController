using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Java.Util;
using System.Threading;
using System.IO;

namespace SlipShoeController
{
    class BTUtil
    {
        public string FileName;

        private readonly string DeviceName = "DSD TECH HC-05";

        private BluetoothAdapter BTAdapter = BluetoothAdapter.DefaultAdapter;
        private BluetoothDevice BTDevice;
        private BluetoothSocket BTSocket;
        private Thread BTThread;

        /// <summary>
        /// Finds the dvice with the name defined in the private string. Opens a socket
        /// connection and starts a thread listening for data
        /// </summary>
        /// <returns>true for success, false for failure</returns>
        public bool Connect()
        {
            try
            {
                //Find the desired module from the list
                BTDevice = (from bd in BTAdapter.BondedDevices where bd.Name == DeviceName select bd).FirstOrDefault();

                //Create a socket to communicate through
                BTSocket = BTDevice.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                BTSocket.Connect();

                //Start the background thread listening for data incoming from bluetooth
                BTThread = new Thread(() => { BluetoothListener(); });
                BTThread.Start();

                //indicate success
                return true;
            }
            catch
            {
                //indicate failure
                return false;
            }

        }

        /// <summary>
        /// Ends the listener thread and closes the bluetooth socket
        /// </summary>
        public void Disconnect()
        {
            if(BTThread != null)
            {
                BTThread.Abort();
                BTThread = null;
                BTSocket.Close();
            }            
        }

        /// <summary>
        /// Thread method. Loops infinitely checking for incoming data and appending it to a file
        /// </summary>
        private void BluetoothListener()
        {
            try
            {
                var InputStream = (BTSocket.InputStream as InputStreamInvoker).BaseInputStream;

                while (true)
                {

                    if (InputStream.Available() > 0)
                    {
                        //Read in from the input stream
                        byte[] buf = new byte[InputStream.Available()];
                        InputStream.Read(buf);

                        //Convert the bytes to a string
                        string data = Encoding.UTF8.GetString(buf);

                        FileUtil.AppendToFile(data, FileName);
                    }
                }
            }
            catch (ThreadAbortException) { }
        }

        /// <summary>
        /// Writes a string to the bluetooth output stream
        /// </summary>
        /// <param name="message">A string containg the data to send over bluetooth</param>
        public void Send(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            try
            {
                BTSocket.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch { }

        }
    }
}