using System.Collections.Generic;

namespace SimpleWix.App.IntermediateFileSystem
{
    public class TempFeature
    {
        public TempFeature(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public string Title { get; }
        public string Description { get; }

        public HashSet<TempFolderObject> ComponentIndex { get; } = new HashSet<TempFolderObject>();


        public void RegisterFolder(TempFolder tempFolder)
        {
            if (ComponentIndex.Contains(tempFolder)) return;
            this.ComponentIndex.Add(tempFolder);
        }

        public void RegisterFile(TempFile tempFile)
        {
            if (ComponentIndex.Contains(tempFile)) return;
            this.ComponentIndex.Add(tempFile);
        }
    }
}