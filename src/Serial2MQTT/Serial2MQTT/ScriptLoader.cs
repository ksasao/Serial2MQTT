using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using uPLibrary.Networking.M2Mqtt;

namespace Serial2MQTT
{
    public class ScriptLoader
    {
        const string Template = "Template";
        public string TemplateFile { get; } = Template + ".cs";
        public void RunScript(string comPort, string host, string path)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            string[] template = File.ReadAllLines(TemplateFile);
            string[] script = File.ReadAllLines(path);
            List<string> builder = new List<string>();
            bool skip = false;
            int offset = 0;
            int count = 0;
            foreach(var line in template)
            {
                string trimed = line.Trim().ToUpper();
                if (trimed.StartsWith("//[*"))
                {
                    skip = true;
                }
                else if(trimed.StartsWith("//*]")){
                    skip = false;
                }
                else if (trimed.StartsWith("//[SCRIPT]")){
                    offset = count;
                    builder.AddRange(script);
                }
                else
                {
                    if (!skip)
                    {
                        count++;
                        builder.Add(line);
                    }
                }
            }
            string text = string.Join("\r\n",builder) + $"\r\nnew {Template}(\"{comPort}\",\"{host}\")";
            try
            {
                ScriptOptions options = ScriptOptions.Default
                    .WithImports("System", "System.Text", "System.Net.Http", "uPLibrary.Networking.M2Mqtt", "Codeplex.Data", "System.IO.Ports")
                    .WithReferences(
                        "System.IO.Ports.dll",
                        Path.GetFullPath(@"Serial2MQTT.dll"),
                        Path.GetFullPath(@"M2Mqtt.dll"));
                var task = CSharpScript.RunAsync(text, options);
                task.Wait();
            }catch(Exception ex)
            {
                string message = ex.Message;
                var regex = new Regex(@"\((\d+),(\d+)\)(.*)");
                var match = regex.Match(message);
                if (match.Success)
                {
                    int line = Convert.ToInt32(match.Groups[1].Value);
                    int col = Convert.ToInt32(match.Groups[2].Value);
                    string body = match.Groups[3].Value;
                    string cursor = (new string(' ', col-1)) + "^";
                    int scriptLine = line - offset;
                    message = $"{path} ({line-offset},{col}){body}\r\n{builder[line-1]}\r\n{cursor}";
                }
                Console.WriteLine(message);
                Environment.Exit(-1);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Environment.Exit(-1);
            }
        }
    }
}
