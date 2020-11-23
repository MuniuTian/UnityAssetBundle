using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResLoaderMgr : MonoBehaviour
{
    public static ResLoaderMgr _Inst;

    public ResLoaderMgr GetInstance
    {
        get { return _Inst; }
    }

    public delegate void lploadAssetEnd(GameObject obj);
    public delegate void lploadBundleEnd();


    public void Awake()
    {
        _Inst = this;
    }

    public void Update()
    {
        UpdateLoadAsset();
    }

    private List<LoadAssetInfo> _LoadAssetList = new List<LoadAssetInfo>();
    private List<LoadAssetInfo> _DelAssetList = new List<LoadAssetInfo>();

    public void AddLoadAsset(AssetInfo assetinfo,AssetBundleRequest assetRequest,lploadAssetEnd loadedAsset)
    {
        LoadAssetInfo info = new LoadAssetInfo();
        info.assetInfo = assetinfo;
        info.assetRequest = assetRequest;
        info.lploadAsset = loadedAsset;

        _LoadAssetList.Add(info);
    }

    public void UpdateLoadAsset()
    {
        if (_DelAssetList.Count > 0)
            _DelAssetList.Clear();

        for (int i = _LoadAssetList.Count - 1; i >= 0; i--)
        {
            try                                                         //try catch, bacause the exception maybe cause for dead update with no error hint.
            {
                LoadAssetInfo info = _LoadAssetList[i];
                if (info.assetRequest.isDone)
                {
                    AssetBundleMgr.Instance.DoAddAsset(info);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("UpdateLoadAsset Exception : " + e.ToString());
            }
        }

        for (int i = _DelAssetList.Count - 1; i >= 0; i--)
        {
            _LoadAssetList.Remove(_DelAssetList[i]);
        }

        _DelAssetList.Clear();
    }


    private List<LoadBundleInfo> _LoadBundleList = new List<LoadBundleInfo>();
    private List<LoadBundleInfo> _DelBundleList = new List<LoadBundleInfo>();

    public void AddLoadBundle()
    {

    }

    public void UpdateLoadBundle()
    {

    }

}

public class LoadAssetInfo
{
    public AssetBundleRequest assetRequest;
    public AssetInfo assetInfo;
    public ResLoaderMgr.lploadAssetEnd lploadAsset;

}

public class LoadBundleInfo
{
    public string assetName;
    public AssetBundleInfo bundleInfo;
    public ResLoaderMgr.lploadBundleEnd loaded;
}