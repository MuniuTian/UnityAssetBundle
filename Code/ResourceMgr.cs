using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Networking;
using System.IO;

using Object = UnityEngine.Object;

public enum ResType
{
    None,
    AssetBundle,
    Native,
    BundlesOTA,
}


public class ResourceMgr : MonoBehaviour 
{
    private static ResourceMgr _mgr;
    public static ResourceMgr Instance
    {
        get { return _mgr; }
    }

    private void Awake()
    {
        _mgr = this;
        StartCoroutine(InitAssetMapCortinue());
    }

    private GameObject gObj = null;
    void OnGUI()
    {
        if (GUILayout.Button("加载预制体Cube"))
        {
             int loaderid = LoadPrefabAsync("testCube.prefab",null);
        }

        if (GUILayout.Button("卸载Cube"))
        {
            UnloadPrefab(gObj);
        }
    }


    public List<UnityEngine.Object> _DelayDestroyList = new List<Object>();
    private void Update()
    {
        AssetBundelMgr.Instance.UpdateLoadAsset();
        AssetBundelMgr.Instance.UpdateLoadBundle();
    }


    //资源加载
    public ResType m_Restype = ResType.AssetBundle;

    private static int loaderID = 100;

    private Dictionary<Object, int> mSyncSpanDict = new Dictionary<Object, int>();

    public static string AssetMap_Path = "file://" + @Application.streamingAssetsPath + "/" + "AssetMap.ab";

    //初始化Mapping
    public Dictionary<string, MappingAsset> mappingAssetDic = new Dictionary<string, MappingAsset>();

    private bool bInit = false;

    private IEnumerator InitAssetMapCortinue()
    {
        yield return null;
        string url = AssetMap_Path;

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        var bytes = request.downloadHandler.data;

        using (var reader = new CsvReader(new MemoryStream(bytes)))
        {
            var it = reader.GetEnumerator();
            while (it.MoveNext())
            {
                var element = it.Current;
                _AddAssetRowItem(element);
            }
        }
        yield return null;
        bInit = true;

    }

    private void _AddAssetRowItem(List<string> row)
    {
        if (row.Count < 2)
        {
            UnityEngine.Debug.LogError(string.Format("[MappingDB._AddAssetRowItem()] row.Count < 2, row = {0}", row.ToString()));
            return;
        }

        var idx = 0;
        var assetPath_ = string.Intern(row[idx++]);
        var bundlePath_ = string.Intern(row[idx++]);

        var count = row.Count - idx;
        string[] dependsAsset = count > 0 ? new string[count] : null;

        for (int i = 0; i < count; ++i)
        {
            dependsAsset[i] = string.Intern(row[idx++]);
        }

        var info = new MappingAsset()
        {
            assetPath = assetPath_,
            bundlePath = bundlePath_,
            dependsAssetPath = dependsAsset,
        };

        AddAssetToDB(assetPath_, info);
    }

    public void AddAssetToDB(string assetPath, MappingAsset info)
    {
        mappingAssetDic.Remove(assetPath);
        mappingAssetDic.Add(assetPath, info);
    }


    /// <summary>
    /// 加载物体 ，外部不要再inistate 
    /// </summary>
    /// <returns>The prefab sync.</returns>
    /// <param name="path">Path.</param>
    public GameObject LoadPrefabSync(string path)
    {
        GameObject obj = null;

        if (m_Restype == ResType.AssetBundle)
        {
            AssetInfo asset = AssetBundelMgr.Instance.LoadAssetSync(path);
            obj = Instantiate(asset._AssetObj) as GameObject;

            mSyncSpanDict.Add(obj, GetLoaderID());
            AssetBundelMgr.Instance._LoaderAssetDic.Add(mSyncSpanDict[obj],asset);
        }
        else
        {
            Object @object = LoadAssetFromDisk<Object>(path);
            obj = Instantiate(@object) as GameObject;

            mSyncSpanDict.Add(obj, GetLoaderID());
        }

        return obj;
    }




    private void AsyncLoadedCallback(GameObject obj)
    {
        obj.name = "TestAsync";
    }



    private int cacheloaderID = 0;
    public delegate void asyncLoaded(GameObject obj);

    private int LoadPrefabAsync(string path,asyncLoaded loaded)
    {
        GameObject obj = null;

        if (m_Restype == ResType.AssetBundle)
        {
            AssetBundelMgr.Instance.LoadAssetAsync(path, AsyncLoadedCallback,GetLoaderID());
        }
        else
        {
            Object @object = LoadAssetFromDisk<Object>(path);
            obj = Instantiate(@object) as GameObject;

            cacheloaderID = GetLoaderID();

            mSyncSpanDict.Add(obj, cacheloaderID);
        }

        return cacheloaderID;
    }

   

    public List<LoadAssetInfo> LoadAssetList = new List<LoadAssetInfo>();




    private IEnumerator AsyncCrotinue()
    {
        yield return null;


    }




    public void UnloadPrefab(Object obj)
    {
        int loaderID = 0;
        if(mSyncSpanDict.TryGetValue(obj,out loaderID))
        {
            DestroyImmediate(obj);
            AssetBundelMgr.Instance.ReleaseObject(loaderID);
            mSyncSpanDict.Remove(obj);
        }
        else
        {
            DestroyImmediate(obj);
        }

    }


    /// <summary>
    /// 加载图片
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="uiTexture">User interface texture.</param>
    public void LoadTexture(string path,UITexture uiTexture)
    {
        AssetInfo asset = AssetBundelMgr.Instance.LoadAssetSync(path);
        uiTexture.mainTexture = asset._AssetObj as Texture2D;
    }



    private int GetLoaderID()
    {
        return loaderID++;
    }


    private T LoadAssetFromDisk<T>(string path) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        string diskPath = GetDiskPath(path);
        if(string.IsNullOrEmpty(diskPath))
        {
            Debug.LogError("路径不存在" +  path);
            return null;
        }

        T obj = AssetDatabase.LoadAssetAtPath<T>(diskPath);
        if(obj == null)
        {
            Debug.LogErrorFormat("LoadAssetFromDisk is Null {0}", path);
        }
        return obj;
#else
        return null;
#endif
    }

    private string GetDiskPath(string path)
    {
        return string.Format("Assets/Bundles/{0}", path);
    }


    
}


