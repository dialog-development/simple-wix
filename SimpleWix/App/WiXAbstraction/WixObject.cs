using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SimpleWix.App.Common;

namespace SimpleWix.App.WiXAbstraction
{

    public class WixObject
    {
        public string Id { get; private set; }
        public List<WixObject> Objects = new List<WixObject>();
        private string _printName;

        public WixObject(string printName)
        {
            _printName = printName;
        }



        public void SetId(string input)
        {
            this.Id = input;
        }

        public void SanitizeAndSetId(string inputId)
        {

            this.Id = "_" + inputId.GetMD5Hash();
        }
        public static string GetRestOfXMLTag(WixObject obj)
        {
            Type type = obj.GetType();
            List<PropertyInfo> properties = type.GetProperties().ToList();

            StringBuilder sb = new StringBuilder();

            PropertyInfo idP = null;
            foreach (PropertyInfo p in properties)
            {
                if (p.Name == "Id")
                {
                    idP = p;
                }
            }
            properties.Remove(idP);
            properties.Insert(0, idP);
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj, null);
                if (value != null)
                {
                    bool skip = false;
                    if (value is string strVal)
                    {
                        if (strVal.IsNullOrWhitespace()) skip = true;
                    }
                    //if(!skip)
                    sb.Append(property.Name + "=\"" + value + "\" ");

                }
            }
            sb.Append(">");
            return sb.ToString();
        }

        public void AppendSubDirIds(string toAppend)
        {
            foreach (WixObject obj in this.Objects)
            {
                if (obj.Id != null)
                    obj.Id = obj.Id + toAppend;
                obj.AppendSubDirIds(toAppend);
            }
        }

        public static string SanitizeXMLProp(string input)
        {
            input = input?.Replace("&", "&amp;");
            input = input?.Replace(">", "&gt;");
            input = input?.Replace("<", "&lt;");
            input = input?.Replace("'", "&apos;");
            input = input?.Replace("\"", "&quot;");
            return input;
        }

        public new string GetPrintName()
        {
            return this._printName;
        }

        public virtual List<string> Print(int indent)
        {
            /*if(this is WixDirectory)
            {
                if(((WixDirectory)this).GetRemovalComponent() != null)
                    ((WixDirectory)this).AddComponent(((WixDirectory)this).GetRemovalComponent());
            }*/
            string currIndent = String.Concat(Enumerable.Repeat("	", indent));
            List<string> output = new List<string>();

            string xmlStartTag = currIndent + "<" + this.GetPrintName() + " " + GetRestOfXMLTag(this);
            if (this.Objects.Count == 0)
            {
                xmlStartTag = xmlStartTag.Replace(">", "/>");
                output.Add(xmlStartTag);
            }
            else
            {
                output.Add(xmlStartTag);
                foreach (WixObject s in this.Objects)
                    output.AddRange(s.Print(indent + 1));
                output.Add(currIndent + @"</" + this.GetPrintName()
                           + ">");
            }
            return output;
        }

        public List<WixComponentRef> GetComponentRefs()
        {
            List<WixComponentRef> foundRefs = new List<WixComponentRef>();
            foreach (WixObject s in Objects)
            {
                if (s is WixComponent)
                    foundRefs.Add(((WixComponent)s).GetComponentRef());
                else
                    foundRefs.AddRange(((WixDirectory)s).GetComponentRefs());
            }
            return foundRefs;
        }

    }

    public class WixDirectory : WixObject
    {
        public string Name { get; set; }
        public string FileSource { get; set; }
        private WixComponent removalComponent { get; set; }
        //private List<string> specialFolders = new List<string>() { "TARGETDIR", "AppDataFolder" };
        private string _path { get; set; }
        private string AppRegFolder { get; set; }


        public WixDirectory(string name, string parentPath, string appRegFolder) : base("Directory")
        {
            this.Name = name;
            this.AppRegFolder = appRegFolder;
            if (!name.IsNullOrEmpty() && VariableConverter.VarToWixId.ContainsKey(name))//if it's a special folder, we just want to set the id verbatim
            {
                this.SetId(VariableConverter.VarToWixId[name]);
                this._path = name;
            }
            else//but if it's not a special folder, we need to create a unique ID for it using it's path
            {           //make sure it's not too long, and sanitize it and make a removal component for it.
                if (parentPath.IsNullOrEmpty()) throw new Exception(name + "'s parent path is null or empty!");
                this._path = System.IO.Path.Combine(parentPath, Name);
                this.SanitizeAndSetId(this._path);
                //if (parentPath != null)
                //{
                //    this._path = System.IO.Path.Combine(parentPath, Name);
                //    this.SanitizeAndSetId(this._path);
                //}
                //else
                //    this.SanitizeAndSetId(id);

                //now that our ID is set, let's create a removal component.
                string removeId = "Rmv_" + this.Id;
                WixComponent removeC = new WixComponent(removeId, GuidManager.GetGuid(removeId), AppRegFolder);
                removeC.AddRemoveFolder(new WixRemoveFolder(this.Id));
                Objects.Add(removeC);
                removalComponent = removeC;
            }
        }
        public void AddDirectory(WixDirectory obj)
        {
            Objects.Add(obj);

        }
        public void AddComponent(WixComponent obj)
        {
            Objects.Add(obj);
        }

        public WixComponentRef GetRemovalRef()
        {
            return new WixComponentRef(this.removalComponent.Id);
        }
        public WixComponent GetRemovalComponent()
        {
            return this.removalComponent;
        }


        public void AddFile(WixFile wfile)
        {
            this.Objects.Add(wfile.GetComponent());
        }
        public string GetPath()
        {
            return this._path;
        }


    }

    public class WixFile : WixObject
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string DiskId { get; set; }
        public string KeyPath { get; set; }
        private string _path { get; set; }
        private WixComponent _fileComponent { get; set; }

        public WixFile(string relativeParentFolder, string name, string source, string appRegFolder) : base("File")
        {
            this._path = Path.Combine(relativeParentFolder, name);
            this.SanitizeAndSetSource(source);
            this.SanitizeAndSetId(this._path);
            this.SanitizeAndSetName(name);
            this.DiskId = "1";
            this.KeyPath = "no";
            var compId = "comp_" + this._path;
            this._fileComponent = new WixComponent(compId, GuidManager.GetGuid(compId), appRegFolder);
            _fileComponent.AddFile(this);
        }

        private void SanitizeAndSetName(string name)
        {
            if (name == null) return;
            this.Name = SanitizeXMLProp(name);
        }

        private void SanitizeAndSetSource(string source)
        {
            if (source == null) return;
            this.Source = SanitizeXMLProp(source);
        }

        public string GetPath()
        {
            return this._path;
        }

        public void SetComponent(WixComponent fileComponent)
        {
            _fileComponent = fileComponent;
        }
        public WixComponent GetComponent()
        {
            return _fileComponent;
        }
    }

    public class WixFeature : WixObject
    {
        public string Title { get; set; }
        public string Level { get; set; }
        public string AllowAdvertise { get; set; }
        public string Description { get; set; }

        public WixFeature(string id, string title, string description) : base("Feature")
        {
            AllowAdvertise = "no";
            this.Title = title;
            this.Level = "1";
            this.SanitizeAndSetId(id);
            this.Description = description;
        }

        public void AddComponentRef(WixComponentRef reff)
        {
            this.Objects.Add(reff);
        }

    }
    //public class WixRemoveFile : WixObject
    //{
    //    public string On { get; set; }
    //    public WixRemoveFile(string id)
    //    {
    //        this.SetId(id);
    //        this.On = "uninstall";
    //    }
    //}
    public class WixRemoveFolder : WixObject
    {
        public string On { get; set; }
        public WixRemoveFolder(string id) : base("RemoveFolder")
        {
            this.SetId(id);
            this.On = "uninstall";
        }
    }
    public class WixComponentRef : WixObject
    {
        public WixComponentRef(string id) : base("ComponentRef")
        {
            this.SetId(id);
        }
    }

    public class WixProduct : WixObject
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public string Version { get; set; }
        public string Manufacturer { get; set; }
        public string UpgradeCode { get; set; }

        public WixProduct(string name, string ver, string upgradeCode) : base("Product")
        {
            this.SetId("*");//id);
            this.Name = name;
            this.Language = "1033";
            this.Version = ver;
           // this.Manufacturer = "";
            this.UpgradeCode = upgradeCode;
        }
        public void AddObj(WixObject obj)
        {
            this.Objects.Add(obj);
        }
    }
    public class WixComponent : WixObject
    {
        public string Guid { get; set; }
        public WixRegistryValue RemoveC { get; set; }
        private WixComponentRef _ref { get; set; }

        public WixComponent(string id, string guid, string appRegFolder) : base("Component")
        {
            this.SanitizeAndSetId(id);
            this.Guid = guid;

            Objects.Add(new WixRegistryValue(Id, appRegFolder));
            _ref = new WixComponentRef(this.Id);
        }
        public void AddRemoveFolder(WixRemoveFolder f)
        {
            this.Objects.Add(f);
        }
        //public void AddRemoveFile(WixRemoveFile f)
        //{
        //    this.Objects.Add(f);
        //}

        public void AddFile(WixFile f)
        {
            this.Objects.Add(f);
        }

        //Get this component's particular reference.
        public WixComponentRef GetComponentRef()
        {
            return _ref;
        }
    }
    public class WixSetDirectory : WixObject
    {
        public string Value { get; set; }

        public WixSetDirectory(string id, string value) : base("SetDirectory")
        {
            this.SetId(id);
            this.Value = value;
        }
    }
    public class WixRegistryValue : WixObject
    {
        public string Root { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string KeyPath { get; set; }

        public WixRegistryValue(string name, string appRegFolder) : base("RegistryValue")
        {
            this.Name = name;
            this.Root = "HKCU";
            this.Key = @"Software\" + appRegFolder;
            this.Value = "";
            this.Type = "string";
            this.KeyPath = "yes";
        }
    }

    public class WixUIRef : WixObject
    {
        public WixUIRef(string id) : base("UIRef")
        {
            this.SetId(id);
        }
    }

    public class WixVariable : WixObject
    {
        public string Value { get; set; }

        public WixVariable(string id, string value) : base("WixVariable")
        {
            this.SetId(id);
            this.Value = value;
        }
    }

    /*  <WixPackage InstallerVersion = "200" Compressed="yes" InstallScope="perUser" InstallPrivileges="limited"/>

          <MajorUpgrade DowngradeErrorMessage = "A newer version of [ProductName] is already installed." />

          < Media Id ="1" Cabinet="Sample.cab" EmbedCab="yes"></Media>
          */
    public class WixPackage : WixObject
    {
        public string InstallerVersion { get; set; }
        public string Compressed { get; set; }
        public string InstallScope { get; set; }
        public string InstallPrivileges { get; set; }
        public WixPackage() : base("Package")
        {
            InstallerVersion = "200";
            Compressed = "yes";
            InstallScope = "perUser";
            InstallPrivileges = "limited";
        }
    }

    public class WixRemoveExistingProducts : WixObject
    {
        public string Before { get; set; }
        public WixRemoveExistingProducts(string before) : base("RemoveExistingProducts")
        {
            this.Before = before;
        }
    }
    public class WixInstallExecuteSequence : WixObject
    {
        public WixInstallExecuteSequence() : base("InstallExecuteSequence")
        {
            this.Objects.Add(new WixRemoveExistingProducts("InstallInitialize"));
        }
    }
    public class WixNotNewerVersionDetected : WixObject
    {
        public override List<string> Print(int indent)
        {
            string currIndent = String.Concat(Enumerable.Repeat("	", indent));
            List<string> output = new List<string>();
            output.Add(currIndent + "NOT NEWERVERSIONDETECTED");
            return output;
        }

        public WixNotNewerVersionDetected() : base("NotNewerVersionDetected")
        {
        }
    }
    public class WixCondition : WixObject
    {
        public string Message { get; set; }
        public WixCondition(string message) : base("Condition")
        {
            this.Message = message;
            this.Objects.Add(new WixNotNewerVersionDetected());
        }
    }
    public class WixUpgrade : WixObject
    {
        private WixInstallExecuteSequence installEx { get; set; }
        private WixCondition cond { get; set; }

        public WixUpgrade(string upgradeCode, string currentVersion) : base("Upgrade")
        {
            this.SetId(upgradeCode);
            WixUpgradeVersion newerVersion = new WixUpgradeVersion(currentVersion, "NEWERVERSIONDETECTED");
            newerVersion.OnlyDetect = "yes";
            newerVersion.IncludeMinimum = "yes";
            this.Objects.Add(newerVersion);

            WixUpgradeVersion olderVersion = new WixUpgradeVersion("0.0.0.1", "OLDERVERSIONBEINGUPGRADED");
            olderVersion.IncludeMinimum = "yes";
            olderVersion.Maximum = currentVersion;
            olderVersion.IncludeMaximum = "no";
            this.Objects.Add(olderVersion);

            installEx = new WixInstallExecuteSequence();
            cond = new WixCondition("Newer version detected!");
        }
        public override List<string> Print(int indent)
        {
            List<string> output = base.Print(indent);
            output.AddRange(this.installEx.Print(indent));
            output.AddRange(this.cond.Print(indent));
            return output;
        }
    }
    public class WixUpgradeVersion : WixObject
    {
        public string Minimum { get; set; }
        public string OnlyDetect { get; set; }
        public string Property { get; set; }
        public string IncludeMinimum { get; set; }
        public string Maximum { get; set; }
        public string IncludeMaximum { get; set; }

        public WixUpgradeVersion(string min, string prop) : base("UpgradeVersion")
        {
            Minimum = min;
            Property = prop;
        }

    }
    public class WixMedia : WixObject
    {
        public string Cabinet { get; set; }
        public string EmbedCab { get; set; }
        public WixMedia() : base("Media")
        {
            this.SetId("1");
            this.Cabinet = "Sample.cab";
            this.EmbedCab = "yes";
        }
    }
    public class Wix : WixObject
    {
        public string xmlns { get; set; }
        public Wix() : base("Wix")
        {
            this.xmlns = @"http://schemas.microsoft.com/wix/2006/wi";
        }
        public void AddProduct(WixProduct p)
        {
            this.Objects.Add(p);
        }
    }

    public class WixIcon : WixObject
    {
        public string SourceFile { get; set; }
        public WixIcon(string id, string sourceFile) : base("Icon")
        {
            this.SourceFile = sourceFile;
            this.SetId(id);
        }

    }
    public class WixProperty : WixObject
    {
        public string Value { get; set; }
        public WixProperty(string id, string value) : base("Property")
        {
            Value = value;
            this.SetId(id);
        }
    }

}
