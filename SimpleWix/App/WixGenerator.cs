using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SimpleWix.App.Input;
using SimpleWix.App.IntermediateFileSystem;
using SimpleWix.App.WiXAbstraction;

//using CommandLine.Text;

namespace SimpleWix.App
{
    class WixGenerator
    {

        public static Assembly ExecutingAssembly { get; } = Assembly.GetExecutingAssembly();
        public static string CurrentPath = Assembly.GetEntryAssembly().Location;
        public static string CurrentFolder { get; } = Path.GetDirectoryName(CurrentPath);
        public static FileVersionInfo FileVersionInfo = FileVersionInfo.GetVersionInfo(ExecutingAssembly.Location);
        public static string Version = FileVersionInfo.FileVersion;
        public static bool QuietMode = false;
        public static string Ind = "    ";




        /// <summary>
        /// Generates a wxs file for use by wix. 
        /// </summary>
        /// <param name="args"> 
        ///  CMD.EXE: WixGenerator.exe [SourceFolder] [AddInName] [AddInVersion] [RevitVersions] [UpgradeCode]
        /// 
        /// SourceFolder:   The folder to build from. This will be the equivalent of the Autodesk\Revit\Addins\YYYY\ folder. e.g. "C:\repos\Interior Partition Tools\bin"
        /// 
        /// AddInName:      The name of your addin e.g. "Interior Partition Tools" 
        /// 
        /// AddInVersion:   The version of your addin e.g. "0.0.0.1"
        /// 
        /// RevitVersions:  The revit versions to build for, separated by a "-", e.g. "2017-2018-2019"
        /// 
        /// UpgradeCode:    A GUID for your product. This essentially identifies your product as what it is, and let's the
        ///                 installer know to uninstall previous versions. If this changes your installer *will not* uninstall the previous 
        ///                 version before installing this one. e.g. "ANY-VALID-GUID"
        /// </param>
        static void Main(string[] args)
        {
            try
            {
                Manifest inputs = InputResult.GetManifest(args);
                QuietMode = inputs?.quietmode ?? false;

                Console.WriteLine(inputs.Print());
                //Console.ReadKey();
                ValidateManifest(inputs);


                printTitle(inputs);


                var fs = InputConverter.ConvertManifestToFileSystem(inputs.features);
                Console.WriteLine(fs.Print());
                var wix = WixConverter.ConvertFileSystem(fs, inputs.productid, inputs.upgradecode, inputs.appname, inputs.version, inputs.icon,
                    inputs.manufacturer, inputs.panel, inputs.banner, inputs.license);

                List<string> outPut = wix.Print(0);
                outPut.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

                using (StreamWriter wr = new StreamWriter(inputs.outputfile))
                {
                    foreach (string s in outPut)
                        wr.WriteLine(s);
                    // xs.Serialize(wr, targetDir);
                }

                using (StreamReader sr = new StreamReader(inputs.outputfile))
                {
                    while (sr.Peek() >= 0)
                    {
                        sr.ReadLine();
                        //Console.WriteLine(sr.ReadLine());
                    }
                }
                int x = 0;

                
                Console.WriteLine("XML WixFile Succesfully Saved To: " + inputs.outputfile);

                Console.WriteLine("");
                Console.WriteLine("");
                if (inputs.complete ?? false)
                {
                    var dir = Path.GetDirectoryName(inputs.outputfile);
                    PrintDir(dir);
                    var candlePath = FindCandlePath();
                    var wixobj = "\"" + inputs.outputfile.Replace(".wxs", ".wixobj") + "\"";
                    Console.WriteLine(RunProcessAndWaitForOutput(candlePath, "\"" + inputs.outputfile + "\"" + " -out "+wixobj));
                    PrintDir(dir);

                    Console.WriteLine("");
                    Console.WriteLine("");
                    var msi = wixobj.Replace(".wixobj", ".msi");
                    var lightPath = FindLightPath();
                    Console.Write(RunProcessAndWaitForOutput(lightPath, "-ext WixUIExtension " + wixobj + " -sice:ICE91" + " -out " + msi));
                    PrintDir(dir);

                }
            }
            catch (Exception e)
            {
                
                Console.WriteLine("An unexpected error occurred:" + Environment.NewLine + e.ToString());
                if (!QuietMode)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit ...");
                    Console.ReadKey();
                }
                Environment.Exit(1);
            }

            if (!QuietMode)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit ...");
                Console.ReadKey();
            }

            Console.WriteLine("Now exiting, goodbye.");
        }

        private static void PrintDir(string dir)
        {
            Console.WriteLine(dir);
            foreach (var fi in Directory.GetFiles(dir))
            {
                Console.WriteLine("    " + fi);
            }
            foreach (var fd in Directory.GetDirectories(dir))
            {
                Console.WriteLine("    " + fd);
            }
        }
        private static string RunProcessAndWaitForOutput(string filepath, string args)
        {

            System.Diagnostics.Process cProcess = new System.Diagnostics.Process();
            cProcess.StartInfo.FileName = filepath;
            cProcess.StartInfo.Arguments = args; //argument
            cProcess.StartInfo.UseShellExecute = false;
            cProcess.StartInfo.RedirectStandardOutput = true;
            cProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            cProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
            cProcess.Start();
            string output = cProcess.StandardOutput.ReadToEnd(); //The output result
            cProcess.WaitForExit();
            if (cProcess.ExitCode > 0)
            {
                Console.WriteLine(output);
                throw new Exception("The process at " + filepath + " exited with code " + cProcess.ExitCode);
            }

            return output;
        }
        private static string FindLightPath()
        {
            try
            {
                var wixFolder = FindWixInstallPath();
                if (wixFolder == null) throw new Exception("Couldn't determine WiX installation folder!");
                var lightPath = Path.Combine(wixFolder, "bin", "light.exe");
                if (!File.Exists(lightPath)) throw new Exception("Couldn't find light.exe at: " + lightPath);
                Console.WriteLine("Light found at: " + lightPath);
                return lightPath;
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't find light.exe path from the Wix toolset: ", e);
            }
        }
        private static string FindCandlePath()
        {
            try
            {
                var wixFolder = FindWixInstallPath();
                if (wixFolder == null) throw new Exception("Couldn't determine WiX installation folder!");
                var candlePath = Path.Combine(wixFolder, "bin", "candle.exe");
                if (!File.Exists(candlePath)) throw new Exception("Couldn't find candle.exe at: " + candlePath);
                Console.WriteLine("Candle found at: " + candlePath);
                return candlePath;
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't find candle.exe path from the Wix toolset: ", e);
            }
        }

        private static string FindWixInstallPath()
        {
            var progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var wixMatches = new List<string>();
            string wixFolder = null;
            foreach (var fd in Directory.GetDirectories(progFiles))
            {
                if (Regex.IsMatch(fd, "WiX Toolset v\\d*.\\d*")) wixMatches.Add(fd);
            }

            if (wixMatches.Count == 1) wixFolder = wixMatches.First();

            return wixFolder;
        }
        private static void ValidateManifest(Manifest inputs)
        {
            if (inputs.appname.IsNullOrEmpty()) throw new Exception("name cannot be empty.");
        }


        static void printTitle(Manifest inputs)
        {
            List<string> title = new List<string>();


            title.Add(""); ;
            title.Add(""); ;
            title.Add(""); ;
            title.Add(" ˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉˉ ");
            title.Add("<                                                                                       >");
            title.Add("<                                                                                       >");
            title.Add("<                                                                                       >");
            title.Add("<<                                                                                     >>");
            title.Add("<<<<                                                                                 >>>>");
            title.Add("<<<<<<<<<<             Simple Wix " + Version + " | Copyright DIALOG 2019             >>>>>>>>>>>");
            title.Add("<<<<                                                                                 >>>>");
            title.Add("<<                                                                                     >>");
            title.Add("<                                                                                       >");
            title.Add("<                                                                                       >");
            title.Add("<                                                                                       >");
            title.Add(" ˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍˍ ");
            title.Add(""); ;
            title.Add(""); ;

            title.Add("About To Generate Wix XML WixFile:  " + inputs.appname);
            title.Add("Manifest:     " + inputs.filepath);
            title.Add(inputs.Print());
            title.Add("");
            title.Add("");
            printListWithDelay(title, 20);
        }
        static void printListWithDelay(List<string> strings, int delay)
        {
            foreach (string s in strings)
            {
                Console.WriteLine(s);
                System.Threading.Thread.Sleep(delay);
            }
        }






    }

    public class GuidManager
    {
        public static string GetGuid(string id)
        {
            return Guid.NewGuid().ToString();
        }
    }



}
