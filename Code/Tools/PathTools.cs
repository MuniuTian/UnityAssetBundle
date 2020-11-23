using System;
using System.IO;
using System.Text;
using UnityEngine;

public static partial class PathTools
{
    /// <summary>
    /// 资源所在的根路径(原始资源, 比如prefab等)
    /// </summary>
    public static string EditorResourceRoot
    {
        get
        {
            return Application.dataPath;
        }
    }
    
    /// <summary>
    /// 资源导出根路径
    /// </summary>
    public static string ExportResourceRoot
    {
        get
        {
            return GetDataPath();
            //var path = "../bundle";
            //return System.IO.Path.GetFullPath (path);
        }
    }

    /// <summary>
    /// 各平台的资源路径
    /// </summary>
    public static string ExportResourcePlatform
    {
        get
        {
            return  os.path.join(ExportResourceRoot, PlatformResFolder);
        }
    }

    /// <summary>
    /// 数据导出路径
    /// </summary>
    public static string ExportMetadataRoot
    {
        get
        {
            return Application.dataPath + "/Resources/Data";
        }
    }

    /// <summary>
    /// 视频路径
    /// </summary>
    public static string ExportVideoPath
    {
        get
        {
            return Application.dataPath + "/Resources/Video";
        }
    }

    public static string PlatformResFolder
    {
        get
        {
            if (os.isIPhonePlayer)
            {
                return "ios";
            }
            else if (os.isAndroid)
            {
                return "android";
            }
            else if (os.isWindows)
            {
                return "pc";
            }
            return string.Empty;
        }
    }

    public static string DiskPath
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    //if (Model.GlobalConfigComponent.Instance.isForceBundle)
                    //    return ExportResourcePlatform;
                    //else
                        return os.path.join(Application.dataPath, "Resources");
                case RuntimePlatform.IPhonePlayer:
                    return Application.temporaryCachePath + "/res";
                case RuntimePlatform.Android:
                    return Application.persistentDataPath + "/res";
                case RuntimePlatform.WindowsPlayer:
                    return Application.persistentDataPath + "/res";
                default:
                    throw new NotImplementedException("[PathTools.DiskPath] invalid platform type! " + Application.platform);
            }
        }
    }

    public static string DiskUrl
    {
        get
        {
            return FileProtocolHead + DiskPath;
        }
    }

    public static string BuiltinPath
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    //if (Model.GlobalConfigComponent.Instance.isForceBundle)
                    //return ExportResourcePlatform;
                    //else
                    return os.path.join(Application.dataPath, "Resources");
                case RuntimePlatform.IPhonePlayer:
                    return ExportResourcePlatform; // Application.streamingAssetsPath;
                case RuntimePlatform.Android:
                    return BuiltinUrl; 
                case RuntimePlatform.WindowsPlayer:
                    return BuiltinUrl;
                default:
                    throw new NotImplementedException("[PathTools.DiskPath] invalid platform type!");
            }
        }
    }

    public static string BuiltinUrl
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    return DiskUrl;
                case RuntimePlatform.IPhonePlayer:
                    return FileProtocolHead + ExportResourcePlatform; // FileProtocolHead + Application.streamingAssetsPath;
                case RuntimePlatform.Android:
                    return  ExportResourcePlatform; // Application.streamingAssetsPath;

                case RuntimePlatform.WindowsPlayer:
                    return  ExportResourcePlatform; //Application.streamingAssetsPath;

                default:
                    throw new NotImplementedException("[PathTools.BuiltInUrl] invalid platform type!");
            }
        }
    }

    public static string FileProtocolHead
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    return "file:///";

                default:
                    return "file://";
            }
        }
    }

    public static string GetStreamAssetsPath
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    return Application.streamingAssetsPath;
                case RuntimePlatform.Android:
                    return Application.streamingAssetsPath;
                case RuntimePlatform.IPhonePlayer:
                    return string.Format("{0}{1}", FileProtocolHead, Application.streamingAssetsPath);
                default:
                    return "";
            }
        }
    }

    public static string GetLocalPath(string fullpath)
    {
        var head = os.path.normpath(ExportResourcePlatform);
        fullpath = os.path.normpath(fullpath);

        if (string.IsNullOrEmpty(fullpath) || !fullpath.StartsWith(head))
        {
            return fullpath;
        }

        return fullpath.Substring(head.Length + 1);
    }

    public static string GetExportPath(string localPath)
    {
        return os.path.join(EditorResourceRoot, PlatformResFolder, localPath);
    }

    internal static int LastIndexOfExtensionDot(string path)
    {
        if (null == path)
        {
            return -1;
        }

        var length = path.Length;
        if (length == 0)
        {
            return -1;
        }

        for (int i = length - 1; i >= 0; i--)
        {
            var c = path[i];
            if (c == '.')
            {
                return i;
            }
            else if (c == '/' || c == '\\')
            {
                return -1;
            }
        }

        return -1;
    }

    public static string GetLocalPathWithDigest(string localPath, string digest)
    {
        var lastDotIndex = LastIndexOfExtensionDot(localPath);
        if (lastDotIndex > 0)
        {
            var localPathWithoutExtension = localPath.Substring(0, lastDotIndex);
            var extension = localPath.Substring(lastDotIndex);

            var localPathWithDigest = localPathWithoutExtension + "." + digest + extension;
            return localPathWithDigest;
        }
        else
        {
            var localPathWithDigest = localPath + "." + digest;
            return localPathWithDigest;
        }
    }

    internal static string ExtractLocalPath(string localPathWithDigest)
    {
        var endDotIndex = LastIndexOfExtensionDot(localPathWithDigest);
        if (endDotIndex == -1)
        {
            return localPathWithDigest;
        }

        var startDotIndex = localPathWithDigest.LastIndexOf('.', endDotIndex - 1);
        var digestLength = endDotIndex - startDotIndex - 1;

        if (digestLength != Md5sum.AssetDigestLength)
        {
            return localPathWithDigest;
        }

        var localPath = localPathWithDigest.Substring(0, startDotIndex) + localPathWithDigest.Substring(endDotIndex);
        return localPath;
    }

    internal static string ExtractAssetDigest(string localPathWithDigest)
    {
        var endDotIndex = LastIndexOfExtensionDot(localPathWithDigest);
        if (endDotIndex == -1)
        {
            return string.Empty;
        }

        var startDigestIndex = localPathWithDigest.LastIndexOf('.', endDotIndex - 1) + 1;
        var digestLength = endDotIndex - startDigestIndex;

        if (digestLength != Md5sum.AssetDigestLength)
        {
            return string.Empty;
        }

        var digest = localPathWithDigest.Substring(startDigestIndex, digestLength);
        return digest;
    }

    /// <summary>
    /// 获取资源名称
    /// </summary>
    /// <returns></returns>
    internal static string GetAssetNameFromAssetPath(string assetPath)
    {
        var folderIdx = assetPath.LastIndexOf('/') + 1;
        return assetPath.Substring(folderIdx);
    }

    //================热更新新加=====
    public static readonly string VERSION_LINE_END = "\r\n";
    public static readonly string VERSION_FILE = "version.txt";

    //AB包导出路径
    public static string ExportABPath
    {
        get
        {
            return os.path.join(Application.streamingAssetsPath, PathTools.PlatformResFolder);
        }
    }

    /// <summary>
    /// 资源打包资源导出根路径
    /// </summary>
    public static string BuildResourceRoot
    {
        get
        {
            var path = "../bundle";
            return System.IO.Path.GetFullPath(path);
        }
    }

    public static string GetDataPath()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return BuildResourceRoot;  //Application.persistentDataPath;
#elif UNITY_ANDROID
		return Application.persistentDataPath;
#elif UNITY_IPHONE
		return Application.temporaryCachePath;
#endif
        return "";
    }

    public static string GetLocalResPath(string fullpath)
    {
       // var head = Application.streamingAssetsPath;
        var head = os.path.normpath(os.path.join(Application.streamingAssetsPath, PlatformResFolder));
        fullpath = os.path.normpath(fullpath);

        if (string.IsNullOrEmpty(fullpath) || !fullpath.StartsWith(head))
        {
            return fullpath;
        }

        return fullpath.Substring(head.Length + 1);
    }

    /// <summary>
	/// 计算文件的MD5值
	/// </summary>
	public static string md5file(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Dispose();
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }

}