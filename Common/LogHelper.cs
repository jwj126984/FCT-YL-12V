using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class LogHelper
    {
        static string iniFilePath = "";
        static string dirPath = System.IO.Directory.GetCurrentDirectory() + "\\Logs\\";
        static object _lock = new object();
        static LogHelper()
        {
            Init();
        }

        public static void Init()
        {
            if (Directory.Exists(dirPath) == false)
            {
                Directory.CreateDirectory(dirPath);
            }
            iniFilePath = dirPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "log.txt";
            if (!System.IO.File.Exists(iniFilePath))
            {
                System.IO.File.Create(iniFilePath).Close();
            }
            else
            {
                System.IO.File.Delete(iniFilePath);
                System.IO.File.Create(iniFilePath).Close();
            }
            WriteLog($"软件版本：{CommonData.Vision}");
        }
        public static void WriteLog(string msg, bool isErr = false)
        {
            string data = $"{DateTime.Now}\n{msg}\n\n";
            if (isErr)
            {
                data = "Error:" + data;
            }
            lock (_lock)
            {
                System.IO.File.AppendAllText(iniFilePath, data);

            }
            //
        }
        static bool isDebug = true;
        public static void Debug(string msg)
        {
            string data = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}\n{msg}\n\n";
            if (isDebug)
            {
                lock (_lock)
                {
                    System.IO.File.AppendAllText(iniFilePath, data);

                }
            }

        }

        static object MainWindow = null;
        public static void SetMainWindow(object _MainWindow)
        {
            MainWindow = _MainWindow;
        }

        public static void ShowMsg(string msg)
        {
            if (MainWindow != null)
            {
                var m = MainWindow.GetType().GetMethod("ShowMsg");

                if (m != null)
                {
                    m?.Invoke(MainWindow, new object[] { msg });
                }
            }
        }

        static object app = null;
        public static void SetApp(object _obj)
        {
            app = _obj;
        }
        public static void MessageBoxShow(string msg)
        {
            if (app != null)
            {
                var m = app.GetType().GetMethod("MessageBoxShow");

                if (m != null)
                {
                    m?.Invoke(app, new object[] { msg });
                }
            }
        }
    }
}
