//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class AssetMgr : MonoBehaviour
//{
//    public Dictionary<string, string> bundleIDMap = new Dictionary<string, string>();

//   //public static Dictionary<string, AssetRef> mAssetMap = new Dictionary<string, AssetRef>();

//    public static Dictionary<string, AssetBundleInfo> mBundleMap = new Dictionary<string, AssetBundleInfo>();

//    private static AssetMgr mgr = null;
//    public static AssetMgr Instance
//    {
//        get
//        {
//            if (mgr == null)
//                mgr = new AssetMgr();
//            return mgr;
//        }
//    }



//    public AssetInfo LoadAssetSync(string path)
//    {
//        string bdname;

//        if(bundleIDMap.TryGetValue(path, out bdname))
//        {
//            Debug.LogError("error");
//            return null;
//        }


//        AssetInfo assetRef = null;

//        if(mBundleMap.ContainsKey(bdname))
//        {
//            AssetBundleInfo bundleinfo = mBundleMap[bdname];
//            CreatSubBundles(bundleinfo.childBundles);

//            //if (mAssetMap.ContainsKey(path))
//            //{
//            //    assetRef = mAssetMap[path];
//            //    assetRef.iAssetRef++;

//            //}
//            //else
//            //{
//            //    assetRef = CreatAssetRef(bundleinfo,path);
//            //}
//        }
//        else
//        {
//            AssetBundleInfo info = CreatBundleInfo(bdname);
//            if (info == null)
//                return null;

//            mBundleMap.Add(bdname,info);
//            CreatSubBundles(info.childBundles);

//            assetRef = CreatAssetRef(info, path);
//        }

//        return assetRef;
//    }

//    private void CreatSubBundles(string[] childBundles)
//    {
//        for (int i = 0; i < childBundles.Length; i++)
//        {
//            string bdname = childBundles[i];
//            if (!mBundleMap.ContainsKey(bdname))
//            {
//                AssetBundleInfo info = CreatBundleInfo(bdname);
//                if (info == null)
//                    continue;

//                mBundleMap.Add(bdname, info);
//            }
//        }
//    }

//    //private AssetRef CreatAssetRef(AssetBundleInfo info,string assetname)
//    //{
//    //    AssetRef assetRef = null;
//    //    Object loaderAsset = info.bundle.LoadAsset(assetname);
//    //    if (loaderAsset != null)
//    //    {
//    //        assetRef = new AssetRef(assetname, loaderAsset);
//    //        mAssetMap.Add(assetname, assetRef);
//    //    }

//    //    return assetRef;
//    //}

//    private AssetBundleInfo CreatBundleInfo(string bdname)
//    {
//        AssetBundle bundle = CreatBundle(bdname);
//        if(bundle == null)
//        {
//            return null;
//        }

//        AssetBundleInfo info = new AssetBundleInfo();
//        info.bundleName = bdname;
//        info.bundle = bundle;
//        info.childBundles = GetDepBundles(bdname);
//        info.iBundleRef = 1;

//        return info;
//    }

//    private AssetBundle CreatBundle(string bdname)
//    {
//        string path = GetBundlePath(bdname);
//        AssetBundle bundle = AssetBundle.LoadFromFile(path, 0, 0);
//        if(bundle == null)
//        {
//            Debug.LogErrorFormat("Bundle load is fail : {0}", path);
//            return null;
//        }
//        return bundle;
//    }

    
//    private string[] GetDepBundles(string bdname)
//    {
//        return new string[2];// 获取依赖bundle
//    }

//    private string GetBundlePath(string bdname)
//    {
//        return bdname;//获取bundlepath,  热更新等。
//    }
//}



//public class BundleInfo2
//{
//    public string bundleName;
//    public string bundlePath;
//}