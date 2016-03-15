﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

public static class Verifier
{
    public static void Verify(string beforeAssemblyPath, string afterAssemblyPath)
    {
        var before = Validate(beforeAssemblyPath);
        var after = Validate(afterAssemblyPath);
        var message = $"Failed processing {Path.GetFileName(afterAssemblyPath)}\r\n{after}";
        Assert.AreEqual(TrimLineNumbers(before), TrimLineNumbers(after), message);
    }

    public static string Validate(string assemblyPath2)
    {
        var exePath = GetPathToPEVerify();
        var process = Process.Start(new ProcessStartInfo(exePath, "\"" + assemblyPath2 + "\"")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });

        process.WaitForExit(10000);
        return process.StandardOutput.ReadToEnd().Trim().Replace(assemblyPath2, "");
    }

    static string GetPathToPEVerify()
    {
        var windowsSdkFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft SDKs\Windows");

        if (!Directory.Exists(windowsSdkFolder))
        {
            throw new DirectoryNotFoundException("Could not find the Windows SDK directory");
        }

        foreach (var version in Directory.GetDirectories(windowsSdkFolder))
        {
            // Find the .NETFX tools folder
            foreach (var dotNetFolder in Directory.GetDirectories(Path.Combine(version, "bin"), "NETFX*"))
            {
                string peVerify = Path.Combine(dotNetFolder, "PEVerify.exe");

                if(File.Exists(peVerify))
                {
                    return peVerify;
                }
            }
        }

        throw new FileNotFoundException("Could not find PEVerify.exe");
    }

    static string TrimLineNumbers(string foo)
    {
        return Regex.Replace(foo, @"0x.*]", "");
    }
}