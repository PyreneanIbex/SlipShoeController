using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SlipShoeController
{
    class FileUtil
    {
        private static char[] InvalidChars = { '\\', '/', '@' };
        public static string DirPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "SlipShoeTrials");
       
        /// <summary>
        /// Creates a Directory called "SlipShoeTrials" if it doesn't already exist
        /// </summary>
        public static void CreateDirectory()
        {
            Directory.CreateDirectory(DirPath);
        }

        /// <summary>
        /// If the file does not already exist, creates the file in the SlipShoeTrials folder
        /// </summary>
        /// <param name="FileName">Name of the file to create</param>
        /// <returns>True if it didn't already exist, false if it did</returns>
        public static bool CreateFile(string FileName)
        {
            if(!File.Exists(Path.Combine(DirPath, FileName)))
            {
                var fs = File.Create(Path.Combine(DirPath, FileName));
                fs.Close();

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Checks for invalid characters in the file name
        /// </summary>
        /// <param name="FileName">Name of the file to check</param>
        /// <returns>An invalid character in the name or O for OK</returns>
        public static char CheckFileName(string FileName)
        {
            foreach(char c in Path.GetInvalidPathChars())
            {
                if(FileName.Contains(c))
                {
                    return c;
                }
            }

            foreach (char c in InvalidChars)
            {
                if(FileName.Contains(c))
                {
                    return c;
                }
            }

            return 'O';
        }

        /// <summary>
        /// Appends string to file with specified name
        /// </summary>
        /// <param name="DataToAppend">String to add to file</param>
        /// <param name="FileName">Name of the file to append to</param>
        public static void AppendToFile(string DataToAppend, string FileName)
        {
            File.AppendAllText(Path.Combine(DirPath, FileName), DataToAppend);
        }
    }
}