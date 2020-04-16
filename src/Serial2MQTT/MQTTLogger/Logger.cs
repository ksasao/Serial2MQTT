using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BisCore
{
    public enum LogType { Output, Error, Info };
    public class Logger : IDisposable
    {
        private static Logger _logger;
        private static object _lockObj = new object();

        const string _path = @"logs\";
        StreamWriter _sw = null;
        string _processName = "";
        int    _processId = 0;
        string _oldMonth = "";

        /// <summary>
        /// ログ管理インスタンスを取得します。
        /// </summary>
        /// <returns>ログ管理インスタンス</returns>
        public static Logger GetInstance()
        {
            lock (_lockObj)
            {
                if (_logger == null)
                {
                    _logger = new Logger();
                }
            }
            return _logger;
        }

        /// <summary>
        /// Logger.GetInstance() メソッドを利用してください。
        /// </summary>
        private Logger()
        {
            string suffix = ".vshost";
            _processName = Process.GetCurrentProcess().ProcessName;
            _processId = Process.GetCurrentProcess().Id;

            if (_processName.EndsWith(suffix))
            {
                _processName = _processName.Substring(0, _processName.Length - suffix.Length);
            }
        }

        /// <summary>
        /// ログを出力します。
        /// </summary>
        /// <param name="text">メッセージ</param>
        public void Output(string text)
        {
            string date = this.Append(text);
            Console.WriteLine($"{date}\t{text}");
        }

        private string Append(string text)
        {
            lock (_lockObj)
            {
                DateTime d = DateTime.Now;
                string month = d.ToString("yyyyMM");
                string date = d.ToString("yyyy-MM-ddTHH:mm:ss.fffzzzz"); // ISO 8601

                if (month != _oldMonth)
                {
                    if (_sw != null)
                    {
                        _sw.Dispose();
                        _sw = null;
                    }
                    _oldMonth = month;
                }
                if (_sw == null)
                {
                    string filename = _path + _processName + "_" + month + "_" + _processId + ".log";
                    string path = Path.GetDirectoryName(filename);
                    Directory.CreateDirectory(path);
                    _sw = new StreamWriter(filename, true);
                }
                _sw.WriteLine($"{date}\t{text}");
                _sw.Flush();
                return date;
            }
        }

        public void Dispose()
        {
            if (_sw != null)
            {
                _sw.Dispose();
            }
            return;
        }
    }
}
