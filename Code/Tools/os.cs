using System;
using System.IO;
using UnityEngine;
using System.Text;

public static partial class os
{
    public static void mkdir(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }
    }

    public static void DeleteDirectory(string directoryPath)
    {
        var di = new DirectoryInfo(directoryPath);
        if (di.Exists)
        {
            Directory.Delete(directoryPath, true);
        }
    }

    public static void startfile(string filename, string arguments = null, bool shell = false)
    {
        var process = new System.Diagnostics.Process();
        var si = process.StartInfo;
        si.FileName = filename;
        si.Arguments = arguments;
        si.UseShellExecute = shell;
        process.Start();
    }

    public static void swap<T>(ref T lhs, ref T rhs)
    {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    public static void dispose<T>(ref T obj) where T : class, IDisposable
    {
        if (null != obj)
        {
            obj.Dispose();
            obj = null;
        }
    }

    public static bool isEditor
    {
        get { return Application.isEditor; }
    }
    public static bool isIPhonePlayer
    {
        get
        {
            var platform = Application.platform;
            var result = platform == RuntimePlatform.IPhonePlayer
#if UNITY_EDITOR
            || UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS
#endif
            ;
            return result;
        }
    }


    public static bool isAndroid
    {
        get
        {
            var platform = Application.platform;
            var result = platform == RuntimePlatform.Android
#if UNITY_EDITOR
            || UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android
#endif
            ;
            return result;
        }
    }

    public static bool isWindows
    {
        get
        {
            var platform = Application.platform;
            var result = platform == RuntimePlatform.WindowsEditor
                         || platform == RuntimePlatform.WindowsPlayer;
            return result;
        }
    }

    public static int frameCount { get; internal set; }
    public static float time { get; internal set; }

    public const string linesep = "\n";

    public static readonly Encoding UTF8 = new UTF8Encoding(false, false);
}