using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.IO;

public class MappingAsset
{
    /// <summary>
    /// 资源路径
    /// </summary>
    public string assetPath;

    /// <summary>
    /// 资源所在Bundle的路径
    /// </summary>
    public string bundlePath;

    /// <summary>
    /// 资源依赖的资源路径
    /// </summary>
    public string[] dependsAssetPath = null;
}



public class LoadBundle : MonoBehaviour
{
    public GameObject _Plane;


    void OnGUI()
    {
        if (GUILayout.Button("加载预制体Cube"))
        {
            _iIndex = 0;
            LoadBundleAAA("testCube.prefab");
        }


        if (GUILayout.Button("卸载bundle"))
        {
            DestroyImmediate(_Plane.transform.GetChild(0).gameObject);
            UnloadBundle();
        }


        if (GUILayout.Button("加载预制体椭圆"))
        {
            _iIndex = 2;
            MyLoadBundleFunc("testCap");
        }


    }


    /// <summary>
    /// 加载预制体
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IEnumerator LoadObj(string bundle_name, string name)
    {
        string path = "file://" + Application.streamingAssetsPath + "/AssetsBundles/" + bundle_name;
        Debug.LogError(string.Format("obj  {0}", path));

        WWW www = new WWW(path);
        //yield return www;

        while (!www.isDone)
        {
            yield return null;
        }

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("error:" + www.error);
            yield break;
        }


        //if (www.error == null)
        {
            AssetBundle bundle = www.assetBundle;
            //这里LoadAsset第二个参数 有没有都能正常运行，这个类型到底什么用途还有待研究

            Debug.Log("asset is : " + name);

            var o = bundle.LoadAsset(name);

            GameObject go = Instantiate(o) as GameObject;
            //go.transform.parent = transform;
            // 上一次加载完之后，下一次加载之前，必须卸载AssetBundle，不然再次加载报错：
            //    The AssetBundle 'Memory' can't be loaded because another AssetBundle with the same files is already loaded
            bundle.Unload(false);
        }
        //else
        //{
        //    Debug.LogError(www.error);
        //}
    }


    public static string AssetMap_Path = "file://" + @Application.streamingAssetsPath + "/" + "AssetMap.ab";
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

    private void Awake()
    {
        StartCoroutine(InitAssetMapCortinue());
    }


    private void LoadBundleAAA(string resName)
    {

        if (!bInit)
            return;


        MappingAsset info = null;
        if (mappingAssetDic.ContainsKey(resName))
        {
            info = mappingAssetDic[resName];

            AssetBundle bundle = LoadBundle_Base(info.bundlePath);

            LoadDependces(info.dependsAssetPath);

            GameObject obj = bundle.LoadAsset<GameObject>(resName);
            if (null != obj)
            {
                GameObject InObject = Instantiate(obj);

                InObject.transform.SetParent(_Plane.transform);
                InObject.transform.localPosition = new Vector3(0, 0.5f);
                InObject.SetActive(true);
            }
        }
    }


    private Dictionary<string, AssetBundle> bundleDic = new Dictionary<string, AssetBundle>();

    private AssetBundle LoadBundle_Base(string bundleName)
    {
        if (bundleDic.ContainsKey(bundleName))
        {
            //加计数
            return bundleDic[bundleName];
        }

        else
        {
            string abPath = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, "AssetBundles", bundleName);
            AssetBundle bundle = AssetBundle.LoadFromFile(abPath);
            bundleDic.Add(bundleName,bundle);
            return bundle;
        }
    }

    private void LoadDependces(string[] dependces)
    {
        if (dependces != null && dependces.Length > 0)
        {
            for (int i = 0; i < dependces.Length; i++)
            {
                string tempName = FilterPath(dependces[i]);

                MappingAsset info = null;
                if (mappingAssetDic.ContainsKey(tempName))
                {
                    info = mappingAssetDic[tempName];

                    LoadBundle_Base(info.bundlePath);
                }
            }
        }
    }


    private void UnloadBundle()
    {
        foreach(string temp in bundleDic.Keys)
        {
            bundleDic[temp].Unload(true);
        }
        bundleDic.Clear();

        for(int i = 0;i < _Plane.transform.childCount;i++)
        {
            DestroyImmediate(_Plane.transform.GetChild(i).gameObject);
        }
    }





    /// <summary>
    /// 加载bundle 通用方法，包括主bundle和引用bundle
    /// </summary>
    private AssetBundle LoadSingleBundle(string resName)
    {
        resName = FilterPath(resName);
        MappingAsset info = null;
        if (mappingAssetDic.ContainsKey(resName))
        {
            info = mappingAssetDic[resName];
            string abPath = info.bundlePath;
            abPath = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, "AssetBundles", abPath);
            AssetBundle bundle = AssetBundle.LoadFromFile(abPath);
            return bundle;
        }

        return null;
    }

    private string FilterPath(string oriPath)
    {
        string[] splits = oriPath.Split('/');
        if (splits.Length == 1)
            return oriPath;

        string sourcePath = oriPath.Remove(0, "Assets/Bundles/".Length);
        return sourcePath;
    }






    private void InitBundleAssetMap()
    {
        string url = "";
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        webRequest.SendWebRequest();

        var bytes = webRequest.downloadHandler.data;
        using (var reader = new CsvReader(new MemoryStream(bytes)))
        {
            var it = reader.GetEnumerator();
            while (it.MoveNext())
            {
                var element = it.Current;
                _AddAssetRowItem(element);
            }
        }
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
            assetPath = assetPath_
            ,
            bundlePath = bundlePath_
            ,
            dependsAssetPath = dependsAsset
        };

        AddAssetToDB(assetPath_, info);
    }

    public Dictionary<string, MappingAsset> mappingAssetDic = new Dictionary<string, MappingAsset>();


    public void AddAssetToDB(string assetPath, MappingAsset info)
    {
        mappingAssetDic.Remove(assetPath);
        mappingAssetDic.Add(assetPath, info);
    }



    //

    // Use this for initialization
    void Start()
    {
        //this.load1();
        //this.load2();
        //this.load3();
        //this.load4();
    }

    void load1()
    {
        //1.先加载cube后加载sphere
        //这种方式并没有先加载cube的依赖文件，按理说应该加载出来的cube上是sphere是missing的，但是unity5.6.3f1
        //加载并未missing，不知是不是unity版本的优化，不过好习惯还是先加载依赖文件，如load2()。

        //加载assetbundlemanifest文件
        AssetBundle assetBundleManifest = AssetBundle.LoadFromFile(Application.dataPath + "/AssetBundles/AssetBundles");
        if (null != assetBundleManifest)
        {
            AssetBundleManifest manifest = assetBundleManifest.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            //加载cube
            AssetBundle bundle = AssetBundle.LoadFromFile(Application.dataPath + "/AssetBundles/cube.prefab");
            GameObject obj = bundle.LoadAsset<GameObject>("Cube");
            if (null != obj)
            {
                GameObject cube = Instantiate(obj);
                cube.transform.SetParent(GameObject.Find("UIRoot").transform);
            }

            //加载cube的依赖文件
            string[] depends = manifest.GetAllDependencies("cube.prefab");
            AssetBundle[] dependsAssetbundle = new AssetBundle[depends.Length];
            for (int index = 0; index < depends.Length; ++index)
            {
                dependsAssetbundle[index] = AssetBundle.LoadFromFile(Application.dataPath + "/AssetBundles/" + depends[index]);

                obj = dependsAssetbundle[index].LoadAsset<GameObject>("Sphere");
                if (null != obj)
                {
                    GameObject sphere = Instantiate(obj);
                    sphere.transform.SetParent(GameObject.Find("UIRoot").transform);
                }
            }
        }
    }

    void load2()
    {
        //2.先加载sphere再加载cube

        //加载assetBundleManifest文件
        AssetBundle assetBundleManifest = AssetBundle.LoadFromFile(Application.dataPath + "/AssetBundles/AssetBundles");
        if (null != assetBundleManifest)
        {
            AssetBundleManifest manifest = assetBundleManifest.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            //加载cube依赖文件
            string[] depends = manifest.GetAllDependencies("cube.prefab");
            AssetBundle[] dependsAssetbundle = new AssetBundle[depends.Length];
            for (int index = 0; index < depends.Length; ++index)
            {
                dependsAssetbundle[index] = AssetBundle.LoadFromFile(Application.dataPath + "/AssetBundles/" + depends[index]);

                GameObject obj1 = dependsAssetbundle[index].LoadAsset<GameObject>("Sphere");
                if (null != obj1)
                {
                    GameObject sphere = Instantiate(obj1);
                    sphere.transform.SetParent(GameObject.Find("UIRoot").transform);
                }
            }

            //加载cube
            AssetBundle bundle = AssetBundle.LoadFromFile(Application.dataPath + "/AssetBundles/cube.prefab");
            GameObject obj = bundle.LoadAsset<GameObject>("Cube");
            if (null != obj)
            {
                GameObject sphere = Instantiate(obj);
                sphere.transform.SetParent(GameObject.Find("UIRoot").transform);
            }
        }
    }

    void load3()
    {
        //3.只加载cube不加载sphere

        //无需加载assetBundleManifest文件，直接加载cube
        AssetBundle bundle = AssetBundle.LoadFromFile(Application.dataPath + "/AssetBundles/cube.prefab");
        GameObject obj = bundle.LoadAsset<GameObject>("Cube");
        if (null != obj)
        {
            GameObject sphere = Instantiate(obj);
            sphere.transform.SetParent(GameObject.Find("UIRoot").transform);
        }
    }

    void load4()
    {
        //4.两个预制打包成同一个AssetBundle

        //无需加载assetBundleManifest文件，直接加载cube
        AssetBundle bundle = AssetBundle.LoadFromFile(Application.dataPath + "/AssetBundles/cube2.prefab");
        GameObject obj = bundle.LoadAsset<GameObject>("Cube_1");
        if (null != obj)
        {
            GameObject cube = Instantiate(obj);
            cube.transform.SetParent(GameObject.Find("UIRoot").transform);
        }

        obj = bundle.LoadAsset<GameObject>("Cube_2");
        if (null != obj)
        {
            GameObject cube = Instantiate(obj);
            cube.transform.SetParent(GameObject.Find("UIRoot").transform);
        }
    }







    private int _iIndex = 0;
    public void MyLoadBundleFunc(string bundleName)
    {


        AssetBundle assetBundleManifest = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/ABFiles/AssetBundles");
        if (null != assetBundleManifest)
        {
            AssetBundleManifest manifest = assetBundleManifest.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            string fullbundleName = string.Format("{0}.ab", bundleName);

            string bFullPath = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, "ABFiles", fullbundleName);

            //加载cube的依赖文件
            string[] depends = manifest.GetAllDependencies(fullbundleName);
            AssetBundle[] dependsAssetbundle = new AssetBundle[depends.Length];
            for (int index = 0; index < depends.Length; ++index)
            {

                dependsAssetbundle[index] = AssetBundle.LoadFromFile(string.Format("{0}/{1}/{2}",
                Application.streamingAssetsPath, "ABFiles", depends[index]));
                //obj = dependsAssetbundle[index].LoadAsset<GameObject>(depends[index]);

                //dependsAssetbundle[index].Unload(false);

            }

            //加载cube
            AssetBundle bundle = AssetBundle.LoadFromFile(bFullPath);

            GameObject obj = bundle.LoadAsset<GameObject>(bundleName);
            if (null != obj)
            {
                GameObject InObject = Instantiate(obj);
                InObject.transform.localPosition = new Vector3(_iIndex, 0);
            }

            for (int i = dependsAssetbundle.Length - 1; i >= 0; i--)
            {
                dependsAssetbundle[i].Unload(false);
            }
            bundle.Unload(false);

        }

        assetBundleManifest.Unload(false);
    }







}




