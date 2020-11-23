using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetInfo
{
    public string AssetName;
    public string bundleName;
    public int iAssetRef = 0;
    public bool isSyncLoadAsset;

    public UnityEngine.Object _AssetObj;

    public int iLoaderID;


    public AssetInfo(string path, UnityEngine.Object loadedAsset)
    {
        this.AssetName = path;
        this._AssetObj = loadedAsset;
        iAssetRef = 1;
    }

    public AssetInfo() { }
}


public class AssetBundleInfo
{
    public string bundleName;
    public int iBundleRef = 0;
    public AssetBundle bundle = null;
    public AssetBundleCreateRequest createRequest = null;
    public string[] childBundles = null;
}



public class AssetBundleMgr
{
    private static AssetBundleMgr mgr;
    public static AssetBundleMgr Instance
    {
        get
        {
            if (mgr == null)
                mgr = new AssetBundleMgr();
            return mgr;
        }
    }


    private Dictionary<string, AssetInfo> _AssetInfoDic = new Dictionary<string, AssetInfo>();
    public Dictionary<string, AssetBundleInfo> _BundleDic = new Dictionary<string, AssetBundleInfo>();
    public Dictionary<int, AssetInfo> _LoaderAssetDic = new Dictionary<int, AssetInfo>();


    private int loaderId = 100;

    public delegate void asyncLoaded(GameObject obj);
    public delegate void asyncLoadedBundle(AssetBundleInfo info,string assetName);

    public int LoadAssetAsync(string path,asyncLoaded loaded,int loaderID)
    {
        if (_AssetInfoDic.ContainsKey(path))
        {
            AssetInfo asset = _AssetInfoDic[path];
            _AssetInfoDic[path].iAssetRef++;

            GameObject obj = UnityEngine.Object.Instantiate(asset._AssetObj) as GameObject;

            loaded(obj);

            return asset.iLoaderID;
        }
        else
        {
            AssetInfo assetinfo = new AssetInfo();
            assetinfo.AssetName = path;
            assetinfo.iAssetRef = 1;
            assetinfo.iLoaderID = GetLoaderID();
            CreatBundleAssetAsync(assetinfo,loaded,false);
            return assetinfo.iLoaderID;
        }
    }

    private string GetAssetName(string assetName)
    {
        return assetName.Split('.')[0];
    }


    public void DoAddAsset(LoadAssetInfo loadAsset)
    {
        if (_AssetInfoDic.ContainsKey(loadAsset.assetInfo.AssetName))
        {
            _AssetInfoDic[loadAsset.assetInfo.AssetName].iAssetRef++;
        }
        else
        {
            loadAsset.assetInfo._AssetObj = UnityEngine.Object.Instantiate(loadAsset.assetRequest.asset);

            _AssetInfoDic.Add(loadAsset.assetInfo.AssetName, loadAsset.assetInfo);
            _LoaderAssetDic.Add(loadAsset.assetInfo.iLoaderID, loadAsset.assetInfo);

            loadAsset.lploadAsset?.Invoke((GameObject)loadAsset.assetInfo._AssetObj);     
        }
    }


    /// <summary>
    /// 异步加载，主Bundle加载完成后开始异步加载Asset，Asset加载完成后执行委托方法。
    /// </summary>
    /// <param name="assetinfo">Assetinfo.</param>
    public void CreatBundleAssetAsync(AssetInfo assetinfo,ResLoaderMgr.lploadAssetEnd loadedAsset)
    {
        MappingAsset info = GetMappingInfo(assetinfo.AssetName);
        if (info == null) return;

        string abPath = GetBundlePath(info.bundlePath);

        if(_BundleDic.ContainsKey(info.bundlePath))
        {
            if(!assetinfo.isSyncLoadAsset)
            {
                AssetBundleInfo bundleInfo = _BundleDic[info.bundlePath];

                AssetBundleRequest assetRequest = bundleInfo.bundle.LoadAssetAsync(assetinfo.AssetName);

                ResLoaderMgr.Instance.AddLoadAsset(assetinfo, assetRequest, loadedAsset);
            }
            else
            {
                AssetBundleInfo bundleInfo = _BundleDic[info.bundlePath];

                assetinfo._AssetObj = bundleInfo.bundle.LoadAsset(assetName);

                _AssetInfoDic.Add(assetName, asset);
                _LoaderAssetDic.Add(asset.iLoaderID, asset);
            }
        }
        else
        {
            AssetBundleInfo bundleInfo = new AssetBundleInfo();
            bundleInfo.bundleName = info.bundlePath;
            bundleInfo.childBundles = info.dependsAssetPath;
            bundleInfo.createRequest = AssetBundle.LoadFromFileAsync(abPath);

            LoadDependcesAsync(info.dependsAssetPath);

            LoadBundleInfo loadInfo = new LoadBundleInfo();
            loadInfo.loaded = OnBundleLoaded;
            loadInfo.bundleInfo = bundleInfo;
            loadInfo.assetName = assetinfo.AssetName;

            loadbundleList.Add(loadInfo);
        }





    }

    private String GetBundlePath(string bdName)
    {
        return string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, "AssetBundles", bdName);
    }








    private void OnBundleLoaded(AssetBundleInfo bundleInfo, string assetName)
    {

        AssetBundleRequest assetRequest = bundleInfo.createRequest.assetBundle.LoadAssetAsync(assetName);

        LoadAssetInfo loadInfo = new LoadAssetInfo();
        loadInfo.name = assetName;
        loadInfo.request = assetRequest;
        //loadInfo.listener = asyncLoaded;
        loadAssetList.Add(loadInfo);
        
    }


    private void LoadDependcesAsync(string[] dependces)
    {
        if (dependces != null && dependces.Length > 0)
        {
            for (int i = 0; i < dependces.Length; i++)
            {
                string tempName = FilterPath(dependces[i]);



                MappingAsset info = GetMappingInfo(dependces[i]);

                string abPath = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, "AssetBundles", info.bundlePath);

                AssetBundleInfo bundleInfo = new AssetBundleInfo();
                bundleInfo.bundleName = info.bundlePath;
                bundleInfo.childBundles = info.dependsAssetPath;
                bundleInfo.createRequest = AssetBundle.LoadFromFileAsync(abPath);

                _BundleDic.Add(info.bundlePath, bundleInfo);

            }
        }
    }


    private List<LoadBundleInfo> delbundleList = new List<LoadBundleInfo>();
    private List<LoadBundleInfo> loadbundleList = new List<LoadBundleInfo>();
    public void UpdateLoadBundle()
    {
        if (delbundleList.Count > 0)
            delbundleList.Clear();

        for (int i = loadbundleList.Count - 1; i >= 0; i--)
        {
            //try catch, bacause the exception maybe cause for dead update with no error hint.
            try
            {
                LoadBundleInfo info = loadbundleList[i];
                if (info.bundleInfo.createRequest.isDone)
                {
                    if (_BundleDic.ContainsKey(info.bundleInfo.bundleName))
                    {
                        _BundleDic[info.bundleInfo.bundleName].iBundleRef++;
                    }
                    else
                    {
                        _BundleDic.Add(info.bundleInfo.bundleName, info.bundleInfo);

                        info.loaded?.Invoke(info.bundleInfo, info.assetName);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("UpdateLoadAsset Exception : " + e.ToString());
            }
        }

        for (int i = delbundleList.Count - 1; i >= 0; i--)
        {
            loadbundleList.Remove(delbundleList[i]);
        }

        delbundleList.Clear();
    }



    private List<LoadAssetInfo> delList = new List<LoadAssetInfo>();
    private List<LoadAssetInfo> loadAssetList = new List<LoadAssetInfo>();
    public void UpdateLoadAsset()
    {
        if (delList.Count > 0)
            delList.Clear();

        for (int i = loadAssetList.Count - 1; i >= 0; i--)
        {
            //try catch, bacause the exception maybe cause for dead update with no error hint.
            try
            {
                LoadAssetInfo info = loadAssetList[i];
                if (info.request.isDone)
                {
                    if(_AssetInfoDic.ContainsKey(info.name))
                    {
                        _AssetInfoDic[info.name].iAssetRef++;
                    }
                    else
                    {
                        AssetInfo asset = new AssetInfo();
                        asset.iAssetRef = 1;
                        asset.AssetName = info.name;
                        asset._AssetObj = UnityEngine.Object.Instantiate(info.request.asset);
                        asset.iLoaderID = GetLoaderID();

                        _AssetInfoDic.Add(info.name, asset);
                        _LoaderAssetDic.Add(asset.iLoaderID, asset);

                        if (info.listener != null)
                            info.listener((GameObject)asset._AssetObj);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("UpdateLoadAsset Exception : " + e.ToString());
            }
        }

        for (int i = delList.Count - 1; i >= 0; i--)
        {
            loadAssetList.Remove(delList[i]);
        }

        delList.Clear();
    }


    private int GetLoaderID()
    {
        return loaderId++;
    }



    public AssetInfo LoadAssetSync(string path)
    {
        AssetInfo asset = null;
        if(_AssetInfoDic.ContainsKey(path))
        {
            asset = _AssetInfoDic[path];
            _AssetInfoDic[path].iAssetRef++;
        }
        else
        {
            asset = CreatBundelAssetSync(path);
        }
        return asset;
    }

    private AssetInfo CreatBundelAssetSync(string path)
    {
        AssetInfo asset = new AssetInfo();
        asset.iAssetRef = 1;
        asset.AssetName = path;
        _AssetInfoDic[path] = asset;

        LoadBundleSync(path,ref asset);
        return asset;
    }




    private AssetBundleInfo LoadBundleSync(string assetPath,ref AssetInfo asset)
    {
        MappingAsset info = null;

        if (ResourceMgr.Instance.mappingAssetDic.TryGetValue(assetPath,out info))
        {
            AssetBundleInfo bundle = LoadSingleBundleSync(info.bundlePath);
            bundle.childBundles = info.dependsAssetPath;

            LoadDependces(info.dependsAssetPath);

            asset._AssetObj = bundle.bundle.LoadAsset(assetPath);
        }
        else
        {
            Debug.LogError("Map中找不到加载资源：" + assetPath);
            return null;
        }
        return null;
    }

    private void LoadDependces(string[] dependces)
    {
        if (dependces != null && dependces.Length > 0)
        {
            for (int i = 0; i < dependces.Length; i++)
            {
                string tempName = FilterPath(dependces[i]);

                MappingAsset info = null;
                if (ResourceMgr.Instance.mappingAssetDic.ContainsKey(tempName))
                {
                    info = ResourceMgr.Instance.mappingAssetDic[tempName];

                    LoadSingleBundleSync(info.bundlePath);
                }
            }
        }
    }


    private AssetInfo CreatAssetSync(string assetName)
    {
        MappingAsset mapInfo = GetMappingInfo(assetName);
        if (mapInfo == null) return null;

        AssetInfo asset = new AssetInfo();
        asset.AssetName = assetName;
        asset.bundleName = mapInfo.bundlePath;
        asset.iAssetRef = 1;
        asset.iLoaderID = GetLoaderID();

        AssetBundleInfo bundleInfo = GetAssetBundleInfo(asset.bundleName);
        if (bundleInfo == null) return null;

        asset._AssetObj = bundleInfo.bundle.LoadAsset(assetName);

        _AssetInfoDic.Add(assetName, asset);
        _LoaderAssetDic.Add(asset.iLoaderID, asset);
        return asset;
    }




    private string FilterPath(string oriPath)
    {
        string[] splits = oriPath.Split('/');
        if (splits.Length == 1)
            return oriPath;

        string sourcePath = oriPath.Remove(0, "Assets/Bundles/".Length);
        return sourcePath;
    }

    private AssetBundleInfo LoadSingleBundleSync(string bundleName)
    {
        if (_BundleDic.ContainsKey(bundleName))
        {
            _BundleDic[bundleName].iBundleRef++;
            return _BundleDic[bundleName];
        }
        else
        {
            string abPath = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, "AssetBundles", bundleName);

            AssetBundle bundle = AssetBundle.LoadFromFile(abPath);

            AssetBundleInfo info = new AssetBundleInfo();
            info.bundleName = bundleName;
            info.bundle = bundle;
            info.iBundleRef = 1;

            _BundleDic.Add(bundleName, info);
            return info;
        }
    }


    public void ReleaseObject(int loaderID)
    {
        AssetInfo asset = null;
        if(_LoaderAssetDic.TryGetValue(loaderID,out asset))
        {
            asset.iAssetRef--;
            SubAssetRef(asset.AssetName);

            if(asset.iAssetRef <= 0)
            {

            }
        }
    }

    public void SubAssetRef(string assetName)
    {
        MappingAsset info = null;

        if (ResourceMgr.Instance.mappingAssetDic.TryGetValue(assetName, out info))
        {
            if (!_BundleDic.ContainsKey(info.bundlePath))
            {
                UnityEngine.Debug.LogWarning("bundle not cache: " + info.bundlePath);
                return;
            }

            --_BundleDic[info.bundlePath].iBundleRef;

            AssetBundleInfo bundleInfo = _BundleDic[info.bundlePath];
            if (bundleInfo.iBundleRef > 0)
                return;

            bundleInfo.bundle.Unload(true);

            UnityEngine.Object.Destroy(bundleInfo.bundle);
            bundleInfo.bundle = null;
            _BundleDic.Remove(info.bundlePath);

            if (bundleInfo.childBundles != null)
            {
                foreach (string child in bundleInfo.childBundles)
                    ReleaseBaseBundle(child);
            }
        }
        else
        {
            Debug.LogError("Map中找不到加载资源：" + assetName);
            return;
        }
    }

    private void ReleaseBaseBundle(string bdName)
    {
        MappingAsset info = null;

        if (ResourceMgr.Instance.mappingAssetDic.TryGetValue(bdName, out info))
        {
            bdName = info.bundlePath;
            if (!_BundleDic.ContainsKey(bdName))
            {
                UnityEngine.Debug.LogWarning("bundle not cache: " + bdName);
                return;
            }

            --_BundleDic[bdName].iBundleRef;

            AssetBundleInfo bundleInfo = _BundleDic[bdName];
            if (bundleInfo.iBundleRef > 0)
                return;

            bundleInfo.bundle.Unload(true);

            UnityEngine.Object.Destroy(bundleInfo.bundle);
            bundleInfo.bundle = null;
            _BundleDic.Remove(bdName);
        }

    }






    private MappingAsset GetMappingInfo(string assetName)
    {
        MappingAsset info = null;

        if (!ResourceMgr.Instance.mappingAssetDic.TryGetValue(assetName, out info))
        {
            Debug.LogError("Map中找不到加载资源：" + assetName);
        }
        return info;
    }

    private AssetBundleInfo GetAssetBundleInfo(string bdName)
    {
        AssetBundleInfo info = null;
        if (!_BundleDic.TryGetValue(bdName, out info))
        {
            Debug.LogError("bundleDic中找不到资源：" + bdName);
        }
        return info;
    }






    public void ReleaseTexture(string pathName)
    {
        if (_BundleDic.ContainsKey(pathName))
        {
            AssetInfo asset = _AssetInfoDic[pathName];
            asset.iAssetRef--;
            if(asset.iAssetRef == 0)
            {
                //asset._AssetObj.Unload(true);
                //UnityEngine.Object.DestroyImmediate(asset._Asset, true);
                //asset.iAssetRef = 0;

               //UnityEngine.Object.DestroyImmediate(asset._bundleObj,true);
                //asset._bundleObj = null;

                _BundleDic.Remove(pathName);
            }
        }
        else
        {
            Debug.LogError("No Bundle Release");
        }
    }
}




