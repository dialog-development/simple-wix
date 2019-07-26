using System.Text;
using CommandLine;

namespace SimpleWix.App.Input
{
    internal class Options
    {
        [Option('c', "complete", HelpText ="If this flag is set, the tool will also attempt to find and launch light.exe and candle.exe to compile the full msi. ")]
        public bool? Complete { get; set; }

        [Option('m', "manifest", Required = false, HelpText = "The source manifest file, if left blank it will check next to itself for manifest.json")]
        public string Manifest { get; set; }

        [Option('n', "appname", Required = false, HelpText = "The name of your tool, i.e. Interior Partition Tools. This is optional if set in the manifest, if set in both this will take precedent.")]
        public string Name { get; set; }

        [Option('v', "version", Required = false, HelpText = "The version number of your addin in the #.#.#.# format (though note that Windows ignores the patch number). This is optional if set in the manifest, if set in both this will take precedent.")]
        public string Version { get; set; }

        [Option('f', "outputFile", HelpText = "The full file path & name of the output file (defaults to Title - Version.wxs at the location of the command prompt executing it. This is optional if set in the manifest, if set in both this will take precedent.")]
        public string OutputFile { get; set; }

        [Option('q', "quiet", HelpText =
            "Operates the tool with no user prompts.  This is optional if set in the manifest, if set in both this will take precedent. ")]
        public bool QuietOperation { get; set; } = false;

        //[HelpOption]
        public string GetUsage()
        {
            // this without using CommandLine.Text
            //  or using HelpText.AutoBuild
            var usage = new StringBuilder();
            usage.AppendLine("Quickstart Application 1.0");
            usage.AppendLine("Read user manual for usage instructions...");
            return usage.ToString();
        }

    }
}