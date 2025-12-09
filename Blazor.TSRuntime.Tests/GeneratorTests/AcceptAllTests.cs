#!/usr/bin/env dotnet

/**
 * goes throgh the folder and subfolders
 *   if file name is "*.received.txt"
 *     rename file to "*.verified.txt" (and overwrite existing file if any)
 **/

foreach (string fileName in Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories))
    if (fileName.EndsWith(".received.txt")) {
        string baseName = fileName[..^".received.txt".Length];
        File.Move(fileName, $"{baseName}.verified.txt", overwrite: true);
        Console.WriteLine($"accepted {Path.GetFileName(baseName)}");
    }
