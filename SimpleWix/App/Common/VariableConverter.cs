using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWix.App.Common
{
    public static class VariableConverter
    {
        public static Dictionary<string, Func<string>> varToCurrentPath { get; } = new Dictionary<string, Func<string>>()
        {
            {"%appdata%", () => { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); }},
            {"%desktop%", () => { return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); }},
            {"%documents%", () => { return Environment.GetFolderPath(Environment.SpecialFolder.Personal); }},
            {"%localappdata%", () => { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }},
            {"%programfiles%", () => { return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86); }},
            {"%programfiles64%", () => { return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles); }},
            {"%programmenu%", () => { return Environment.GetFolderPath(Environment.SpecialFolder.Programs); }},
            {"%startup%", () => { return Environment.GetFolderPath(Environment.SpecialFolder.Startup); }},
            {"%windowsvolume%", () => { return Environment.GetFolderPath(Environment.SpecialFolder.System); }}


        };

        public static string ExpandPath(string path)
        {
            foreach (var kvp in varToCurrentPath)
            {
                path = path.Replace(kvp.Key, kvp.Value.Invoke());
            }

            return path;
        }

        public static Dictionary<string, string> VarToWixId { get; } = new Dictionary<string, string>()
        {
            {"SourceDir", "TARGETDIR"},
            {"%appdata%", "AppDataFolder"},
            {"%desktop%", "DesktopFolder"},
            {"%documents%","PersonalFolder"},
            {"%localappdata%","LocalAppDataFolder" },
            {"%programfiles%", "ProgramFilesFolder"},
            {"%programfiles64%","ProgramFiles64Folder"},
            {"%programmenu%", "ProgramMenuFolder"},
            {"%startup%", "StartupFolder"},
            {"%windowsvolume%", "WindowsVolume"}

        };



    };
        
    
}
