using System;
using System.IO;
using System.IO.Compression;

using UnityEngine;

public class Compression
{
    public static void Compress(string sourcePath, string destinationFilePath)
    {
        Debug.Log("Compressing directory: " + sourcePath + " -> " + destinationFilePath);
        ZipFile.CreateFromDirectory(sourcePath, destinationFilePath);
    }
}