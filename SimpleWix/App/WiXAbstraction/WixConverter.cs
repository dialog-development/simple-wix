using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SimpleWix.App.IntermediateFileSystem;


namespace SimpleWix.App.WiXAbstraction
{
    public static class WixConverter
    {

        public static Wix ConvertFileSystem(TempFileSystem fs, string productid, string upgradecode, string appname, string version, string icon, string manufacturer, string dialogPath, string bannerPath, string licensePath)
        {

            WixProduct prod = new WixProduct(productid, appname, version, upgradecode);
            prod.AddObj(new WixPackage());
            prod.AddObj(new WixUpgrade(upgradecode, version));
            prod.AddObj(new WixMedia());

            string iconFileName = icon;
            if (iconFileName.IsNullOrEmpty()) iconFileName = SaveIconToDisk();
            var iconName = Path.GetFileName(iconFileName);
            prod.AddObj(new WixIcon(iconName, iconFileName));
            prod.AddObj(new WixProperty("ARPPRODUCTICON", iconName));

            //check manufacturer and app name for valid registry names.




            //string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appRegistryFolder = manufacturer + "\\" + appname;


            var lookupDict = new Dictionary<TempFolderObject, WixObject>();
            var wixObjects = GenerateFileSystemObjects(lookupDict, fs.Root, appRegistryFolder);
            foreach (var wixObj in wixObjects)
            {
                prod.AddObj(wixObj);

            }

            foreach (var feat in fs.Features.Values)
            {
                WixFeature wfeat = new WixFeature("_" + feat.Title, feat.Title, feat.Description);
                foreach (var comp in feat.ComponentIndex)
                {
                    var wixObj = lookupDict[comp];
                    if (wixObj is WixFile wfile) wfeat.AddComponentRef(wfile.GetComponent().GetComponentRef());
                    else if (wixObj is WixDirectory wFold) wfeat.AddComponentRef(wFold.GetRemovalRef());

                }
                prod.AddObj(wfeat);
            }

            //**************** UI BELOW 
            prod.AddObj(new WixUIRef("WixUI_FeatureTree"));
            prod.AddObj(new WixUIRef("WixUI_ErrorProgressText"));

            prod.Objects.AddRange(UnpackUIResources( dialogPath,  bannerPath,  licensePath));


            Wix wix = new Wix();
            wix.AddProduct(prod);

            return wix;
     
        }


        static List<WixVariable> UnpackUIResources(string dialogPath, string bannerPath, string licensePath)
        {
            List<WixVariable> outputs = new List<WixVariable>();
            string tempPath = Path.GetTempPath();
            tempPath = Path.Combine(tempPath, @"SimpleWix");
            System.IO.Directory.CreateDirectory(tempPath);

            if (dialogPath.IsNullOrEmpty())
            {
                dialogPath = Path.Combine(tempPath, "WixUIDialogBmp.bmp");
                using (FileStream fs = new FileStream(dialogPath, FileMode.Create))
                    Properties.Resources.WixUIDialog.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            outputs.Add(new WixVariable("WixUIDialogBmp", dialogPath));

            if (bannerPath.IsNullOrEmpty())
            {
                bannerPath = Path.Combine(tempPath, "WixUIBannerBmp.bmp");
                using (FileStream fs = new FileStream(bannerPath, FileMode.Create))
                    Properties.Resources.WixUIBanner.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            outputs.Add(new WixVariable("WixUIBannerBmp", bannerPath));




            if (licensePath.IsNullOrEmpty())
            {
                licensePath = Path.Combine(tempPath, "WixUILicense.rtf");
                File.WriteAllText(licensePath, Properties.Resources.License_Agreement___Template);
            }

            outputs.Add(new WixVariable("WixUILicenseRtf", licensePath));

            return outputs;

        }
        private static List<WixObject> GenerateFileSystemObjects(Dictionary<TempFolderObject, WixObject> lookupDict, TempFolder root, string appRegistryFolder)
        {

            WixDirectory targetDir = new WixDirectory("SourceDir", null, appRegistryFolder);
            TempFolder windowsVolumeFlag = PopulateWixDir_Rec(lookupDict, root, targetDir, appRegistryFolder);

            if(windowsVolumeFlag == null)
                return new List<WixObject>() {targetDir};
            else
            {
                var wixFolder = lookupDict[windowsVolumeFlag];
                return new List<WixObject>() {targetDir, new WixSetDirectory(wixFolder.Id, "[WindowsVolume]"+windowsVolumeFlag.Name) };
            }

        }

        private static TempFolder PopulateWixDir_Rec(Dictionary<TempFolderObject, WixObject> lookupDict, TempFolder currFolder, WixDirectory currWixFolder, string appRegistryFolder)
        {
            foreach (var file in currFolder.Files)
            {
                WixFile wfile = new WixFile(currWixFolder.GetPath(), file.Name, file.Source, appRegistryFolder);
                currWixFolder.AddFile(wfile);
                lookupDict.Add(file, wfile);
            }

            TempFolder windowsVolumeFlag = null;
            foreach (var folder in currFolder.Folders)
            {
                if (folder.Empty) continue;
                if (folder.Name != @"%windowsvolume%")
                {
                    WixDirectory wfolder = new WixDirectory(folder.Name, currWixFolder.GetPath(), appRegistryFolder);
                    currWixFolder.AddDirectory(wfolder);
                    lookupDict.Add(folder, wfolder);
                    windowsVolumeFlag = PopulateWixDir_Rec(lookupDict, folder, wfolder, appRegistryFolder);
                }
                else
                {
                    var nextFolder = folder.Folders.First();
                  //  WixDirectory wfolder = new WixDirectory(nextFolder.Name, currWixFolder.GetPath(), appRegistryFolder);
                   // currWixFolder.AddDirectory(wfolder);
                    //lookupDict.Add(nextFolder, wfolder);
                    PopulateWixDir_Rec(lookupDict, folder, currWixFolder, appRegistryFolder);//we're skipping ahead in our recursive search but returning a flag.
                    windowsVolumeFlag = nextFolder;
                }
            }
            return windowsVolumeFlag;
        }

        //static void Serialize(Manifest inputs)
        //{

        //    WixProduct prod = new WixProduct(inputs.productid, inputs.appname, inputs.version, inputs.upgradecode);
        //    prod.AddObj(new WixPackage());
        //    prod.AddObj(new WixUpgrade(inputs.upgradecode, inputs.version));
        //    prod.AddObj(new WixMedia());

        //    string iconFileName = inputs.icon;
        //    if (iconFileName.IsNullOrEmpty()) iconFileName = SaveIconToDisk();
        //    var iconName = Path.GetFileName(iconFileName);
        //    prod.AddObj(new WixIcon(iconName, iconFileName));
        //    prod.AddObj(new WixProperty("ARPPRODUCTICON", iconName));

        //    //check manufacturer and app name for valid registry names.




        //    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        //    string appRegistryFolder = inputs.manufacturer + "\\" + inputs.appname;

        //    WixDirectory targetDir = new WixDirectory("TARGETDIR", "SourceDir", null, appRegistryFolder);




        //    WixDirectory appD = new WixDirectory("AppDataFolder", "AData", null, appRegistryFolder);
        //    //WixDirectory adesk = new WixDirectory("_Autodesk", "Autodesk", null, inputs.AddInName);
        //    //WixDirectory revit = new WixDirectory("_Revit", "Revit", null, inputs.AddInName);
        //    //WixDirectory addins = new WixDirectory("_Addins", "Addins", null, inputs.AddInName);

        //    foreach (string v in inputs.features)
        //    {
        //        WixDirectory year = new WixDirectory("_" + v, v, "source", appRegistryFolder);
        //        year.AddSource(inputs.SourcePath);
        //        year.AppendSubDirIds(v);
        //        addins.AddDirectory(year);
        //        WixFeature feat = new WixFeature("_" + v, v);
        //        foreach (WixComponentRef c in year.GetComponentRefs())
        //        {
        //            feat.AddComponentRef(c);
        //        }

        //        feat.AddComponentRef(adesk.GetRemovalRef());
        //        feat.AddComponentRef(revit.GetRemovalRef());
        //        feat.AddComponentRef(addins.GetRemovalRef());
        //        features.Add(feat);
        //    }
        //    revit.AddDirectory(addins);
        //    adesk.AddDirectory(revit);
        //    appD.AddDirectory(adesk);
        //    targetDir.AddDirectory(appD);

        //    prod.AddObj(targetDir);
        //    foreach (WixFeature f in features)
        //        prod.AddObj(f);










        //    //**************** UI BELOW 
        //    prod.AddObj(new WixUIRef("WixUI_FeatureTree"));
        //    prod.AddObj(new WixUIRef("WixUI_ErrorProgressText"));

        //    prod.Objects.AddRange(UnpackUIResources(inputs));
        //    //****************

        //    Wix wix = new Wix();
        //    wix.AddProduct(prod);
        //    List<string> outPut = wix.Print(0);
        //    outPut.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

        //    using (StreamWriter wr = new StreamWriter(inputs.outputfile))
        //    {
        //        foreach (string s in outPut)
        //            wr.WriteLine(s);
        //        // xs.Serialize(wr, targetDir);
        //    }

        //    using (StreamReader sr = new StreamReader(inputs.outputfile))
        //    {
        //        while (sr.Peek() >= 0)
        //        {
        //            Console.WriteLine(sr.ReadLine());
        //        }
        //    }
        //    int x = 0;

        //    Console.WriteLine("");
        //    Console.WriteLine("");
        //    Console.WriteLine("XML WixFile Succesfully Saved To: " + inputs.outputfile);
        //}



  

        static string SaveIconToDisk()
        {
            string tempPath = Path.GetTempPath();
            tempPath = Path.Combine(tempPath, @"SimpleWix");
            System.IO.Directory.CreateDirectory(tempPath);

            string filename = Path.Combine(tempPath, "dialogLogo.ico");

            using (FileStream fs = new FileStream(filename, FileMode.Create))
                Properties.Resources.WixUIIcon.Save(fs);
            return filename;
        }
    }
}
