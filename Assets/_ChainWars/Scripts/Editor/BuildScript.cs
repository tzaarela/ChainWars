using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO.Compression;
using System.IO;

public class BuildScript
{
    [PostProcessBuild(0)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltExe)
    {
        var directory = Path.GetDirectoryName(pathToBuiltExe);

        Compression.Compress(directory, Path.Combine(directory));
    }
}
