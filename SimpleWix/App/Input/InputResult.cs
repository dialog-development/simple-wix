using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;
using CommandLine;
using Newtonsoft.Json;

namespace SimpleWix.App.Input
{
    static class InputResult
    {

        public static Manifest GetManifest(string[] input)
        {
            try
            {
                ParserResult<Options> result = Parser.Default.ParseArguments<Options>(input);

                if (result.Tag == ParserResultType.Parsed)
                {
                    Options options = ((Parsed<Options>)result).Value;
                    var manifest = GetManifest(options);
                    CombineOptionsAndManifest(options, manifest);
                    PopulateManifestWithDefaults(manifest);
                    return manifest;
                }
                else
                    throw new Exception("Unable to parse command line options. ");
            }
            catch (Exception e)
            {
                throw new Exception("There was an error gathering input.", e);
            }

        }

        private static void PopulateManifestWithDefaults(Manifest man)
        {
            if (man.appname.IsNullOrEmpty()) throw new Exception("Appname must be set either through the manifest of command options");
            if (man.version.IsNullOrEmpty()) throw new Exception("Version must be set either through the manifest of command options");
            if (man.upgradecode.IsNullOrEmpty()) throw new Exception("Upgradecode must be set either through the manifest of command options");
            if (man.outputfile.IsNullOrEmpty()) man.outputfile = Path.Combine(SimpleWix.CurrentFolder, man.appname + " - " + man.version + ".wxs");
            if (man.features == null || man.features.None()) throw new Exception("Error, no features set in manifest!");
            if (man.complete == null) man.complete = false;
        }

        private static Manifest GetManifest(Options options)
        {
            string manifestPath = null;
            if (options.Manifest.IsNullOrEmpty()) manifestPath = Path.Combine(SimpleWix.CurrentFolder, "manifest.json");
            else manifestPath = options.Manifest;
            if (!File.Exists(manifestPath)) throw new Exception("Manifest path does not exist: " + manifestPath);
            var str = File.ReadAllText(manifestPath);
            var man = JsonConvert.DeserializeObject<Manifest>(str);
            man.filepath = manifestPath;
            return man;
        }

        private static void CombineOptionsAndManifest(Options opt, Manifest man)
        {
            bool valid = true;
            if (!opt.Name.IsNullOrEmpty())
                man.appname = opt.Name;

            if (!opt.Version.IsNullOrEmpty())
                man.version = opt.Version;

            if (!opt.OutputFile.IsNullOrEmpty())
                man.outputfile = opt.OutputFile;
        }
        static string[] parseVersions(string input)
        {
            string[] parts2 = Regex.Split(input, @"-");
            return parts2;
        }
    }

    public static class IEnumerableExtensions
    {
        public static bool None<T>(this IEnumerable<T> enumer)
        {
            return !enumer.Any();
        }
    }
}