using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using SimpleWix.App.Common;
using SimpleWix.App.Input;

namespace SimpleWix.App.IntermediateFileSystem
{
    public static class InputConverter
    {
        public static string CleanFilePath(this string filepath)
        {
            filepath = Regex.Replace(filepath, "\\*", "\\");
            if (filepath.First() == '\\') filepath = filepath.Remove(0, 1);
            if (filepath.EndsWith("\\")) filepath = filepath.Remove(filepath.Length - 1, 1);
            return filepath;
        }
        public static string[] CleanAndSplitFilePath(this string filepath)
        {
            filepath = filepath.CleanFilePath();
            return filepath.Split('\\');
        }
        public static bool IsDriveLetter(this string filepathChunk)
        {
            if (filepathChunk == null) return false;
            return Regex.IsMatch(filepathChunk, "^[a-zA-Z]:$");
        }
        public static TempFileSystem ConvertManifestToFileSystem(IEnumerable<Feature> features)
        {

            var fs = new TempFileSystem();
            fs.InitializeShortcutFolders(VariableConverter.VarToWixId.Keys);
            foreach (var feat in features)
            {
                var fsFeat = fs.AddFeature(feat.title, feat.description);

                foreach (var ci in feat.copyinfo)
                {
                    var src = ci.source;
                    src = src.CleanFilePath();
                    var srcParts = src.CleanAndSplitFilePath().ToList();
                    if (srcParts.None()) throw new Exception("No source found! " + src);

                    //if the first part of the file path isn't a drive letter and isn't in our special folder list, assume they mean relative to the current path.
                    if (!srcParts.First().IsDriveLetter() && !VariableConverter.VarToWixId.ContainsKey(srcParts.First()))
                    {
                        var currFolder = WixGenerator.CurrentFolder;
                        try
                        {
                            if (srcParts.First() == "..")
                            {
                                int startIndex = 0;
                                for (int i = 0; i < srcParts.Count; i++)
                                {
                                    if (srcParts[i] == "..")
                                    {
                                        currFolder = Path.GetDirectoryName(currFolder);
                                    }
                                    else
                                    {
                                        startIndex = i;
                                        break;
                                    }
                                }

                                srcParts.RemoveRange(0, startIndex);
                                src = "";
                                foreach (var part in srcParts)
                                {
                                    src = Path.Combine(src, part);
                                }
                                

                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(new Exception("There was an error navigating up folders!", e));
                        }

                        src = Path.Combine(currFolder, src);
                    }

                    var dest = ci.destination;
                    //expand vars here
                    if (File.Exists(src))
                    {
                        fs.AddFileOrUpdateFeatures(dest, src, fsFeat);
                    }
                    else if (Directory.Exists(src))
                    {
                        AddDirectoryFromSource(fs, src, dest, fsFeat, ci.includesubfolders);
                    }
                    else throw new Exception("Path does not exist: " + src);
                }

            }

            return fs;
        }

        private static void AddDirectoryFromSource(TempFileSystem fs, string src, string dest, TempFeature fsFeat, bool includeSubDirectories)
        {
            var files = Directory.GetFiles(src);
            foreach (var fi in files)
            {
                fs.AddFileOrUpdateFeatures(dest, fi, fsFeat);
            }

            if (includeSubDirectories)
            {
                var folders = Directory.GetDirectories(src);
                foreach (var fd in folders)
                {
                    var destname = dest + "\\" + Path.GetFileName(fd);
                    AddDirectoryFromSource(fs, fd, destname, fsFeat, includeSubDirectories);
                }
            }
        }
    }


    public class TempFileSystem
    {
        //private Dictionary<string, TempFolderObject> _index { get; } = new Dictionary<string, TempFolderObject>();
        public TempFolder Root { get; }
        public Dictionary<string, TempFeature> Features { get; } = new Dictionary<string, TempFeature>();

        public TempFileSystem()
        {
            this.Root = new TempFolder(this, null, "root", null);
        }

        //public void RegisterObject(TempFolderObject folderObject)
        //{
        //    if (this._index.ContainsKey(folderObject.Path)) throw new Exception("File system already contains object with the path: " + folderObject.Path);
        //    this._index.Add(folderObject.Path, folderObject);
        //}

        //public void TryRegisterObject(TempFolderObject folderObject)
        //{
        //    if (this._index.ContainsKey(folderObject.Path)) return;
        //    this._index.Add(folderObject.Path, folderObject);
        //}

        //public bool ContainsObject(string tempPath)
        //{
        //    if (this._index.ContainsKey(tempPath)) return true;
        //    return false;
        //}

        //public void UpdateRegistration(TempFolderObject tempFolderObject, string proposedPath)
        //{
        //    if (this.ContainsObject(proposedPath)) throw new Exception("Cannot move, file already exists: " + proposedPath);
        //    if (this._index.ContainsKey(tempFolderObject.Path)) this._index.Remove(tempFolderObject.Path);
        //    tempFolderObject.Path = proposedPath;
        //    this._index.Add(proposedPath, tempFolderObject);
        //}

        public string Print()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TempFileSystem");
            sb.AppendLine(this.Root.Print(WixGenerator.Ind));
            return sb.ToString();
        }

        public TempFeature AddFeature(string featTitle, string featDescription)
        {
            var feat = new TempFeature(featTitle, featDescription);
            this.Features.Add(featTitle, feat);
            return feat;
        }



        /// <summary>
        /// //////////////////////////////////////////////////////////////////><!--->-->
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="fsFeat"></param>

        public void AddFileOrUpdateFeatures(string dest, string src, TempFeature fsFeat)
        {

            var objects = dest.CleanAndSplitFilePath().ToList();

            var srcParts = src.CleanAndSplitFilePath().ToList();
            objects.Add(srcParts[srcParts.Count - 1]);


            //first let's deal with the top level, this should either be a shortcut var which will be precreated, or it's a drive letter.
            var topLevelName = objects[0];
            TempFolder topLevelFolder = null;
            if (this.Root.ContainsFolder(topLevelName)) topLevelFolder = this.Root.GetFolder(topLevelName);
            else
            {
                if (!topLevelName.IsDriveLetter()) throw new Exception("Attempted to add non drive or shortcut top level folder: " + topLevelName);
                topLevelFolder = this.Root.CreateFolder(topLevelName, fsFeat);
            }


            //after that the middle bits should all be folders.
            TempFolder prevFolder = topLevelFolder;
            TempFolder currFolder = null;
            for (int i = 1; i < objects.Count - 1; i++)
            {
                var name = objects[i];
                //if (name == "\\") continue;
                currFolder = prevFolder.AddOrGetAndRegisterFolder(name, fsFeat);
                prevFolder = currFolder;
            }

            if (currFolder == null) throw new Exception("unknown error 112");

            //finally the last object is our file name
            var finalName = objects[objects.Count - 1];

            //it may have been required here by another feature in which case it should just update the feature registration.
            currFolder.CreateFileOrRegisterFeature(finalName, src, fsFeat);
        }

        public void InitializeShortcutFolders(IEnumerable<string> values)
        {
            foreach (var s in values)
            {
                this.Root.CreateSpecialFolder(s);
            }
        }
    }

    public class TempFolderObject
    {
        public string Name { get; set; }
        public TempFileSystem FileSystem { get; set; }
        public string Path { get; set; }
        public TempFolder Parent { get; set; }
        public Dictionary<string, TempFeature> Features { get; set; } = new Dictionary<string, TempFeature>();

        public void Move(TempFolder parent)
        {
            var tempPath = parent + "\\" + this.Name;
            //this.FileSystem.UpdateRegistration(this, tempPath);
            this.Parent?.RemoveObject(this);
            this.Parent = parent;
            this.Path = tempPath;
        }



        public override string ToString()
        {
            return Name;
        }
    }

    public class TempFile : TempFolderObject
    {
        public string Source { get; set; }

        public TempFile(TempFileSystem fileSystem, TempFolder parent, string name, string src, TempFeature feat)
        {
            this.FileSystem = fileSystem;
            Name = name;
            this.Source = src;
            this.Path = parent?.Path + "\\";
            this.Move(parent);
            this.Features.Add(feat.Title, feat);
            feat.RegisterFile(this);
        }

        public string Print(string indent)
        {
            return indent + "FI:" + this.Name;
        }

        public void Update(string src, TempFeature fsFeat)
        {
            if (this.Source != src) throw new Exception("Can't add same file from multiple sources: " + this.Source + " | " + src);
            if (this.Features.ContainsKey(fsFeat.Title)) return;
            this.Features.Add(fsFeat.Title, fsFeat);
            fsFeat.RegisterFile(this);
        }
    }

    public class TempFolder : TempFolderObject
    {
        private Dictionary<string, TempFolderObject> Directory { get; } = new Dictionary<string, TempFolderObject>();
        private Dictionary<string, TempFile> FileDirectory { get; } = new Dictionary<string, TempFile>();
        private Dictionary<string, TempFolder> FolderDirectory { get; } = new Dictionary<string, TempFolder>();

        public List<TempFile> Files => new List<TempFile>(FileDirectory.Values);
        public List<TempFolder> Folders => new List<TempFolder>(FolderDirectory.Values);
        public bool Empty => !Directory.Any();


        private TempFolder(TempFileSystem fileSystem, TempFolder parent, string name)
        {
            this.FileSystem = fileSystem;
            this.Name = name;
            this.Path = parent?.Path + "\\";
        }


        public TempFolder(TempFileSystem fileSystem, TempFolder parent, string name, TempFeature feat) : this(fileSystem, parent, name)
        {
            if (feat != null)
            {
                this.Features.Add(feat.Title, feat);
                feat.RegisterFolder(this);
            }

            if (feat == null && !(name == "root" || VariableConverter.VarToWixId.ContainsKey(name))) throw new Exception(name + " must have a feature");
        }



        public void RemoveObject(TempFolderObject tempFolderObject)
        {
            if (this.Directory.ContainsKey(tempFolderObject.Name))
            {
                this.Directory.Remove(tempFolderObject.Name);
                if (tempFolderObject is TempFile file)
                    this.FileDirectory.Remove(file.Name);
                else if (tempFolderObject is TempFolder folder)
                    this.FolderDirectory.Remove(folder.Name);
            }
        }

        public TempFolder CreateFolder(string name, TempFeature feat)
        {
            if (this.Directory.ContainsKey(name)) throw new Exception("This folder already has a file or folder with the name: " + name);
            var newFolder = new TempFolder(this.FileSystem, this, name, feat);
            this.Directory.Add(name, newFolder);
            this.FolderDirectory.Add(name, newFolder);
            //this.FileSystem.TryRegisterObject(newFolder);
            return newFolder;
        }

        public TempFolder CreateSpecialFolder(string name)
        {
            if (this.Directory.ContainsKey(name)) throw new Exception("This folder already has a file or folder with the name: " + name);
            var newFolder = new TempFolder(this.FileSystem, this, name, null);
            this.Directory.Add(name, newFolder);
            this.FolderDirectory.Add(name, newFolder);
            //this.FileSystem.RegisterObject(newFolder);
            return newFolder;
        }

        public TempFile CreateFile(string name, string src, TempFeature feat)
        {
            if (this.Directory.ContainsKey(name)) throw new Exception("This folder already has a file or folder with the name: " + name);
            var newFile = new TempFile(this.FileSystem, this, name, src, feat);
            this.Directory.Add(name, newFile);
            this.FileDirectory.Add(name, newFile);
            //this.FileSystem.RegisterObject(newFile);
            return newFile;
        }

        public void CreateFileOrRegisterFeature(string finalName, string src, TempFeature fsFeat)
        {
            if (this.FileDirectory.ContainsKey(finalName))
            {
                var file = this.FileDirectory[finalName];
                file.Update(src, fsFeat);
                return;
            }
            else
            {
                var file = this.CreateFile(finalName, src, fsFeat);
            }
        }

        public string Print(string indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + "FD:" + this.Name);
            foreach (var fi in this.FileDirectory)
            {
                sb.AppendLine(fi.Value.Print(indent + WixGenerator.Ind));
            }
            foreach (var fi in this.FolderDirectory)
            {
                sb.AppendLine(fi.Value.Print(indent + WixGenerator.Ind));
            }


            return sb.ToString();
        }

        public bool ContainsFolder(string topLevel)
        {
            return this.FolderDirectory.ContainsKey(topLevel);
        }

        public TempFolder GetFolder(string topLevelName)
        {
            if (!this.FolderDirectory.ContainsKey(topLevelName)) throw new Exception("Does not contain this folder: " + topLevelName);
            return this.FolderDirectory[topLevelName];
        }

        public TempFolder AddOrGetAndRegisterFolder(string name, TempFeature feat)
        {
            if (this.ContainsFolder(name)) return this.GetAndRegisterFolder(name, feat);
            return this.CreateFolder(name, feat);
        }

        private TempFolder GetAndRegisterFolder(string name, TempFeature feat)
        {
            var fold = this.GetFolder(name);
            return fold.RegisterFeature(feat);

        }

        private TempFolder RegisterFeature(TempFeature feat)
        {
            if (this.Features.ContainsKey(feat.Title)) return this;
            this.Features.Add(feat.Title, feat);
            feat.RegisterFolder(this);
            return this;
        }



    }
}
