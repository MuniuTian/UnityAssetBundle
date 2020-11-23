using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 1.在打bundle时，将依赖文件写入一个csv文件
/// 2.将csv文件保存在streamassets文件夹，等到加载bundle时加载出来
/// 
/// map结构：name,bundlePath,depdences
/// assepMap assetName,BundlePath
/// bundleMap BundleName,Depdences(为assetname) 
/// 
///  一：第一版暂不考虑懒加载，直接将Asset以及Bundle内容加载出来。
/// 二：第二版，将prefab上的资源剥离，做懒加载。
/// 
/// 
/// 
/// 
/// </summary>




public class EditorBundleInfo
{
    public string assetName;
    public string bundleName;
    public string[] assetPaths;
    public string[] dependces;
}




public class BundleEditor:MonoBehaviour
{
    public static string AssetBundle_TargetDirectory_Path = @Application.streamingAssetsPath + "/" + "AssetBundles";  //目标文件夹
    private static string AssetBundle_BuildDirectory_Path = @Application.streamingAssetsPath + "/" + "TempABFiles";   //生成文件夹
    private static string AssetBundle_SourceDirectory_Path = @Application.dataPath + "/" + "Bundles";//源文件夹
    private static string BundleMap_Path = @Application.streamingAssetsPath + "/" + "BundleMap.ab";//
    public static string AssetMap_Path = @Application.streamingAssetsPath + "/" + "AssetMap.ab";
    private static BuildTarget buildTarget = BuildTarget.StandaloneOSX;



    /// <summary>
    /// The bundle dic.
    /// </summary>
    private static Dictionary<string, EditorBundleInfo> bundleDic = new Dictionary<string, EditorBundleInfo>();



    /// <summary>
    /// Checks the depdences.
    /// 收集资源到一个List中，然后check的时候从list中寻找
    /// </summary>
    [MenuItem("Tools/Asset Bundle/Check Depdences", false, 0)]
    private static void CheckDepdences()
    {
        CollectBundleList();

        AnalyBundleDependces();  
    }



    private static void CollectBundleList()
    {
        bundleDic.Clear();

        if (Directory.Exists(AssetBundle_SourceDirectory_Path))
        {
            var dir = new DirectoryInfo(AssetBundle_SourceDirectory_Path);
            var files = dir.GetFiles("*", SearchOption.AllDirectories);

            for (var i = 0; i < files.Length; ++i)
            {
                var fileInfo = files[i];

                if (!fileInfo.Name.EndsWith(".meta", System.StringComparison.Ordinal))
                {
                    string bundleName = fileInfo.Name.Split('.')[0];
                    string assetPath = string.Format("Assets/{0}/{1}", "Bundles", fileInfo.Name);
                    string[] assets = new string[] { assetPath };

                    string[] deps = AssetDatabase.GetDependencies(assetPath);

                    EditorBundleInfo info = new EditorBundleInfo();
                    info.assetPaths = assets;
                    info.dependces = deps;
                    info.bundleName = bundleName;
                    info.assetName = assetPath;

                    bundleDic.Add(fileInfo.Name, info);

                }
            }
        }
    }

    private static bool AnalyBundleDependces()
    {
        
        foreach (string temp in bundleDic.Keys)
        {
            for (int index = 0; index < bundleDic[temp].dependces.Length; index++)
            {
                string assetPath = bundleDic[temp].dependces[index].Substring("Assets/Bundles/".Length);

                if (!bundleDic.ContainsKey(assetPath))
                {
                    Debug.LogErrorFormat("Error {0} Dont Have Dependence :{1}", temp, assetPath);
                    return false;
                }
            }
        }
        Debug.Log("Check Done");
        return true;

    }



    [MenuItem("Tools/Asset Bundle/Build AssetBundle", false, 0)]
    private static void BuildAssetBundle()
    {
        CollectBundleList();

        if (!AnalyBundleDependces())
            return;

        AssetBundleBuild[] bundleBuilds;
        int index = 0;

  
        bundleBuilds = new AssetBundleBuild[bundleDic.Count];

        CsvWriter csvWriter = new CsvWriter(AssetMap_Path);

        foreach (var temp in bundleDic)
        {
            EditorUtility.DisplayProgressBar("收集bundle资源", "收集bundle资源中", 1f * index / bundleDic.Count);

            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = temp.Value.bundleName;
            build.assetNames = temp.Value.assetPaths;
            bundleBuilds[index] = build;
            index++;

            List<string> assetData = new List<string>();
            assetData.Add(temp.Value.assetName.Substring("Assets/Bundles/".Length));
            assetData.Add(temp.Value.bundleName);

            for(int i = 0;i < temp.Value.dependces.Length;i++)
            {
                if (temp.Value.dependces[i] == temp.Value.assetName)
                    continue;
                assetData.Add(temp.Value.dependces[i].Substring("Assets/Bundles/".Length));
            }


            csvWriter.WriteRow(assetData);
        }






#if UNITY_ANDROID
        buildTarget = BuildTarget.Android;
#elif UNITY_IOS
        buildTarget = BuildTarget.IOS;
#endif


            //第一步，删除旧的bundle资源
            DirectoryInfo AB_Directory = new DirectoryInfo(AssetBundle_BuildDirectory_Path);
            if (!AB_Directory.Exists)
            {
                AB_Directory.Create();
            }

            FileInfo[] filesAB = AB_Directory.GetFiles();
            foreach (var item in filesAB)
            {
                Debug.Log("******删除旧文件：" + item.FullName + "******");
                item.Delete();
            }





            //第二步，开始打新的bundle资源
            AssetBundleManifest abm = BuildPipeline.BuildAssetBundles(AssetBundle_BuildDirectory_Path,
                                                                          bundleBuilds,
                                                                          BuildAssetBundleOptions.ChunkBasedCompression,
                                                                          buildTarget);


            Debug.Log("******AssetBundle打包完成******");

            Debug.Log("将要转移的文件夹是：" + AssetBundle_TargetDirectory_Path);
            FileInfo[] filesAB_temp = AB_Directory.GetFiles();

            DirectoryInfo streaming_Directory = new DirectoryInfo(AssetBundle_TargetDirectory_Path);
            if (!streaming_Directory.Exists)
            {
                streaming_Directory.Create();
            }

            FileInfo[] streaming_files = streaming_Directory.GetFiles();
            foreach (var item in streaming_files)
            {
                item.Delete();
            }

            AssetDatabase.Refresh();

            foreach (var item in filesAB_temp)
            {
                item.CopyTo(AssetBundle_TargetDirectory_Path + "/" + item.Name, true);
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            Debug.Log("******文件传输完成******");


    }


    private static void WriteCsv(string path,List<string> datas)
    {
        CsvWriter csvWriter = new CsvWriter(path);
        csvWriter.WriteRow(datas);
    }


    












    [MenuItem("AssetBundle/SetPathName")]
    private static void SetPathName()
    {
        string Path = "Prefabs";
        string abName = "building.ab";
        SetVersionDirAssetName(Path, abName);//第一个参数是路径 第二个参数是Ab名字 默认前缀为 Application.dataPath + "/"＋ Path  
    }
    private static void SetVersionDirAssetName(string path,string abName)
    {
        var relativeLen = path.Length + 8; // Assets 长度  
        path = Application.dataPath + "/" + path + "/";

        if (Directory.Exists(path))
        {
            EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 0f);
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles("*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; ++i)
            {
                var fileInfo = files[i];
                EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 1f * i / files.Length);
               
                if (!fileInfo.Name.EndsWith(".meta",System.StringComparison.Ordinal))
                {
                    var basePath = fileInfo.FullName.Substring(path.Length - relativeLen);//.Replace('\\', '/');  
                    var importer = AssetImporter.GetAtPath(basePath);
                    if (importer && importer.assetBundleName != abName)
                    {
                        importer.assetBundleName = abName;
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }


    [MenuItem("AssetBundle/设定文件名")]
    public static void SetFileName()
    {
        string Path = "Prefabs";
        SetAssetNameAsPrefabName(Path);//第一个参数是路径   
    }
    public static void SetAssetNameAsPrefabName(string fullPath)
    {
        var relativeLen = fullPath.Length + 8; // Assets 长度  
        fullPath = Application.dataPath + "/" + fullPath + "/";

        if (Directory.Exists(fullPath))
        {
            EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 0f);
            var dir = new DirectoryInfo(fullPath);
            var files = dir.GetFiles("*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; ++i)
            {
                var fileInfo = files[i];
                string abName = fileInfo.Name;
                EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 1f * i / files.Length);
                if (!fileInfo.Name.EndsWith(".meta",System.StringComparison.Ordinal))
                {
                    var basePath = fileInfo.FullName.Substring(fullPath.Length - relativeLen);//.Replace('\\', '/');  
                    var importer = AssetImporter.GetAtPath(basePath);
                    //abName = AssetDatabase.AssetPathToGUID(basePath);
                    if (importer && importer.assetBundleName != abName)
                    {
                        importer.assetBundleName = abName;
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }

    /// <summary>  
    /// AssetBundleManifestName == 对应AB依赖列表文件  
    /// </summary>  

    
    [MenuItem("Tools/Asset Bundle/Build Asset Bundles", false, 0)]
    public static void BuildAssetBundleAndroid()
    {

    }

    private static string _dirName = "";
    /// <summary>  
    /// 批量命名所选文件夹下资源的AssetBundleName.  
    /// </summary>  
    [MenuItem("Tools/Asset Bundle/Set Asset Bundle Name")]
    static void SetSelectFolderFileBundleName()
    {
        UnityEngine.Object[] selObj = Selection.GetFiltered(typeof(UnityEngine.Object),SelectionMode.Unfiltered);
        foreach (Object item in selObj)
        {
            string objPath = AssetDatabase.GetAssetPath(item);
            DirectoryInfo dirInfo = new DirectoryInfo(objPath);
            if (dirInfo == null)
            {
                Debug.LogError("******请检查，是否选中了非文件夹对象******");
                return;
            }
            _dirName = dirInfo.Name;

            string filePath = dirInfo.FullName.Replace('\\', '/');
            filePath = filePath.Replace(Application.dataPath, "Assets");
            AssetImporter ai = AssetImporter.GetAtPath(filePath);
            ai.assetBundleName = _dirName;

            SetAssetBundleName(dirInfo);
        }
        AssetDatabase.Refresh();
        Debug.Log("******批量设置AssetBundle名称成功******");
    }

    static void SetAssetBundleName(DirectoryInfo dirInfo)
    {
        FileSystemInfo[] files = dirInfo.GetFileSystemInfos();
        foreach (FileSystemInfo file in files)
        {
            if (file is FileInfo && file.Extension != ".meta" && file.Extension != ".txt")
            {
                string filePath = file.FullName.Replace('\\', '/');
                filePath = filePath.Replace(Application.dataPath, "Assets");
                AssetImporter ai = AssetImporter.GetAtPath(filePath);
                ai.assetBundleName = _dirName;
            }
            else if (file is DirectoryInfo)
            {
                string filePath = file.FullName.Replace('\\', '/');
                filePath = filePath.Replace(Application.dataPath, "Assets");
                AssetImporter ai = AssetImporter.GetAtPath(filePath);
                ai.assetBundleName = _dirName;
                SetAssetBundleName(file as DirectoryInfo);
            }
        }
    }



    /// <summary>  
    /// 批量清空所选文件夹下资源的AssetBundleName.  
    /// </summary>  
    /// 
    [MenuItem("Tools/Asset Bundle/Reset Asset Bundle Name")]
    static void ResetSelectFolderFileBundleName()
    {
        UnityEngine.Object[] selObj = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        foreach (UnityEngine.Object item in selObj)
        {
            string objPath = AssetDatabase.GetAssetPath(item);
            DirectoryInfo dirInfo = new DirectoryInfo(objPath);
            if (dirInfo == null)
            {
                Debug.LogError("******请检查，是否选中了非文件夹对象******");
                return;
            }
            _dirName = null;

            string filePath = dirInfo.FullName.Replace('\\', '/');
            filePath = filePath.Replace(Application.dataPath, "Assets");
            AssetImporter ai = AssetImporter.GetAtPath(filePath);
            ai.assetBundleName = _dirName;

            SetAssetBundleName(dirInfo);
        }
        AssetDatabase.Refresh();
        Debug.Log("******批量清除AssetBundle名称成功******");
    }


    /// <summary>  
    /// 设置文件夹下文件bundle名称     
    /// </summary>  
    /// 
    [MenuItem("Tools/Asset Bundle/My SetName")]
    static void MyTestSetBundleName()
    {
        string Path = "Prefabs";
        
        string fullPath = Application.dataPath + "/" + Path + "/";

        if (Directory.Exists(fullPath))
        {
            EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 0f);

            var dir = new DirectoryInfo(fullPath);
            var files = dir.GetFiles("*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; ++i)
            {
                var fileInfo = files[i];

                EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 1f * i / files.Length);
                if (!fileInfo.Name.EndsWith(".meta", System.StringComparison.Ordinal))
                {
                    string abName = fileInfo.Name.Split('.')[0];
                    abName = string.Format("{0}.ab",abName);
                    //var basePath = fileInfo.FullName.Substring(fullPath.Length - relativeLen);//.Replace('\\', '/'); 
                    var basePath = string.Format("Assets/{0}/{1}",Path,fileInfo.Name);

                    var importer = AssetImporter.GetAtPath(basePath);

                    if (importer && importer.assetBundleName != abName)
                    {
                        importer.assetBundleName = abName;

                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }



    public const string BundlePath = "Bundles";

    public static string BundleTarget_Path = @Application.streamingAssetsPath + "/../../../" + "AssetBundles";

    [MenuItem("Tools/Asset Bundle/Build Asset Bundles", false, 0)]
    public static void BuildAssetBundles()
    {
        UnityEngine.Debug.Log("--" + AssetBundle_BuildDirectory_Path);
        //Application.streamingAssetsPath对应的StreamingAssets的子目录  
        DirectoryInfo AB_Directory = new DirectoryInfo(AssetBundle_BuildDirectory_Path);
        if (!AB_Directory.Exists)
        {
            AB_Directory.Create();
        }
        FileInfo[] filesAB = AB_Directory.GetFiles();
        foreach (var item in filesAB)
        {
            Debug.Log("******删除旧文件：" + item.FullName + "******");
            item.Delete();
        }
#if UNITY_ANDROID
        BuildPipeline.BuildAssetBundles(AB_Directory.FullName, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);  
#elif UNITY_EDITOR_OSX
        BuildPipeline.BuildAssetBundles(AB_Directory.FullName, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
#else
        BuildPipeline.BuildAssetBundles(AB_Directory.FullName, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
#endif
        Debug.Log("******AssetBundle打包完成******");

        Debug.Log("将要转移的文件夹是：" + AssetBundle_TargetDirectory_Path);
        FileInfo[] filesAB_temp = AB_Directory.GetFiles();

        DirectoryInfo streaming_Directory = new DirectoryInfo(AssetBundle_TargetDirectory_Path);
        if (!streaming_Directory.Exists)
        {
            streaming_Directory.Create();
        }



        FileInfo[] streaming_files = streaming_Directory.GetFiles();
        foreach (var item in streaming_files)
        {
            item.Delete();
        }
        AssetDatabase.Refresh();
        foreach (var item in filesAB_temp)
        {
            //if (item.Extension == "")
            {
                item.CopyTo(AssetBundle_TargetDirectory_Path + "/" + item.Name, true);
            }
        }
        AssetDatabase.Refresh();
        Debug.Log("******文件传输完成******");
    }




}




