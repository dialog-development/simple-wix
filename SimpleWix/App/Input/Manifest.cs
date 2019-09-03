using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleWix.App.Input
{

    public class Manifest
    {
        public string appname { get; set; }
        public string version { get; set; }
        public string manufacturer { get; set; }
        public string upgradecode { get; set; }
        public string productid { get; set; }
        public string outputfile { get; set; }
        public string banner { get; set; }
        public string panel { get; set; }
        public string icon { get; set; }
        public string license { get; set; }
        public bool quietmode { get; set; }
        public bool? complete { get; set; }
        public List<Feature> features { get; set; } = new List<Feature>();
        public List<UninstallInfo> uninstall { get; set; } = new List<UninstallInfo>();
        [JsonIgnore] public string filepath { get; set; }


        public string Print()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Manifest");
            sb.AppendLine(SimpleWix.Ind + "appname: " + appname ?? "none");
            sb.AppendLine(SimpleWix.Ind + "version: " + version ?? "none");
            sb.AppendLine(SimpleWix.Ind + "manufacturer: " + manufacturer ?? "none");
            sb.AppendLine(SimpleWix.Ind + "upgradecode: " + upgradecode ?? "none");
            sb.AppendLine(SimpleWix.Ind + "productid: " + productid ?? "none");
            sb.AppendLine(SimpleWix.Ind + "outputfile: " + outputfile ?? "none");
            sb.AppendLine(SimpleWix.Ind + "banner: " + banner ?? "none");
            sb.AppendLine(SimpleWix.Ind + "panel: " + panel ?? "none");
            sb.AppendLine(SimpleWix.Ind + "icon: " + icon ?? "none");
            sb.AppendLine(SimpleWix.Ind + "license: " + license ?? "none");
            sb.AppendLine(SimpleWix.Ind + "quietmode: " + quietmode ?? "none");
            sb.AppendLine(SimpleWix.Ind + "complete: " + complete ?? "none");

            sb.AppendLine(SimpleWix.Ind + "features:");
            foreach (var feat in features)
            {
                sb.AppendLine(feat?.Print(SimpleWix.Ind + SimpleWix.Ind) ?? "No features!");
            }
            sb.AppendLine(SimpleWix.Ind + "uninstall:");
            foreach (var ui in uninstall)
            {
                sb.AppendLine( ui?.Print(SimpleWix.Ind + SimpleWix.Ind) ?? "No uninstall components!");
            }

            return sb.ToString();
        }
    }

    public class Feature
    {
        public string title { get; set; } = "";
        public string description { get; set; } = "";
        public List<CopyInfo> copyinfo { get; set; } = new List<CopyInfo>();

        public string Print(string ind)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(ind + "Feature:");
            sb.AppendLine(ind + "title:" + title);
            sb.AppendLine(ind + "description:" + description);
            sb.AppendLine(ind + "copyinfo:");
            foreach (var ci in copyinfo)
            {
                sb.AppendLine(ci.Print(ind + SimpleWix.Ind ));
            }

            return sb.ToString();
        }
    }

    public class CopyInfo
    {
        public string source { get; set; }
        public string destination { get; set; }
        public bool includesubfolders { get; set; } = false;

        public string Print(string ind)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(ind + "CopyInfo:");
            sb.AppendLine(ind + "source:" + source);
            sb.AppendLine(ind + "destination:" + destination);
            sb.AppendLine(ind + "includesubfolders:" + includesubfolders);

            return sb.ToString();
        }
    }
    public class UninstallInfo
    {
        public string path { get; set; }
        public bool includesubfolders { get; set; }

        public string Print(string ind)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(ind + "UninstallInfo:");
            sb.AppendLine(ind + SimpleWix.Ind + "path:" + path);
            sb.AppendLine(ind + SimpleWix.Ind + "includesubfolders:" + includesubfolders);

            return sb.ToString();
        }
    }
}
