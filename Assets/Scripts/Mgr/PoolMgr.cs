using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 抽屉数据-缓存池中的一种容器，用于存放对象
/// </summary>
public class PoolData
{
    //抽屉中对象挂载的父结点
    public GameObject fatherObj;
    //对象的容器
    public List<GameObject> poolList;

    public PoolData(GameObject obj,GameObject poolObj)
    {
        //给抽屉创建一个父对象并把它作为我们Pool对象（衣柜）的子物体
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.parent = poolObj.transform;

        poolList = new List<GameObject>() {};
        PushObj(obj);
    }

    public void PushObj(GameObject obj)
    {
        //失活，让对象隐藏
        obj.SetActive(false);
        //存起来
        poolList.Add(obj);
        //设置父对象
        obj.transform.parent = fatherObj.transform;
    }

    public GameObject GetObj()
    {
        GameObject obj = null;
        //取出第一个
        obj = poolList[0];
        poolList.RemoveAt(0);
        //激活
        obj.SetActive(true);
        //断开父子关系
        obj.transform.parent = null;

        return obj;
    }
}

/// <summary>
/// 缓存池模块
/// 1.Dictionary List
/// 2.GameObject 和 Resources 两个公共类中的API
/// </summary>
public class PoolMgr : BaseManager<PoolMgr>//继承单例模式父类
{
    //缓存池容器（衣柜）
    public Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();
    //预制体根节点
    public GameObject poolObj;

    public void Start()
    {
        //EventCenter.GetInstance().AddEventListener("ChangeScene", Clear);
    }

    /*public GameObject GetObj(string keyName)
    {
        if (poolDic.ContainsKey(keyName) && poolDic[keyName].poolList.Count > 0)//判断已经存在这种抽屉且抽屉内含有keyName类别的物体
        {
            obj = poolDic[keyName].GetObj();//赋值取出
        }
        else//无这种抽屉或者已经没有这种物体的话，实例化一个出来（第一次实例化时不必先创建抽屉，抽屉在Push时再创建）
        {
            obj = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/" + keyName));//Resources.Load可以结合后面的资源管理器更正使用
            //Instantiate克隆出去的名字都带有（clone）后缀，故需要把obj的名字改成和抽屉（即原物体的名字）一致
            obj.name = keyName;
        }

        return obj;
    }*/

    public void GetObj(string keyName,UnityAction<GameObject> callback)
    {
        if (poolDic.ContainsKey(keyName) && poolDic[keyName].poolList.Count > 0)//判断已经存在这种抽屉且抽屉内含有keyName类别的物体
        {
            callback(poolDic[keyName].GetObj());
        }
        else//无这种抽屉或者已经没有这种物体的话，实例化一个出来（第一次实例化时不必先创建抽屉，抽屉在Push时再创建）
        {
            //通过异步加载资源创建对象给外部用
            ResMgr.GetInstance().LoadAsync<GameObject>(keyName, (o) =>
            {
                o.name = keyName;
                callback(o);
            });
        }
    }

    public void PushObj(string keyName,GameObject obj)//外面物体的方法会在激活一段时间后自动invoke PushObj这个方法
    {
        if (poolObj == null)//若收容的pool不存在，新建一个
        {
            poolObj = new GameObject("Pool");
        }
        
        if (poolDic.ContainsKey(keyName))//如果有抽屉则将该物体压入list中
        {
            poolDic[keyName].PushObj(obj);
        }
        else//如果没有抽屉
        {
            poolDic.Add(keyName, new PoolData(obj, poolObj));//创建抽屉，且把这个物体放进去
        }
    }

    /// <summary>
    /// 清空缓存池的方法
    /// 主要用在切换场景时
    /// </summary>
    /// <param name="obj"></param>
    public void Clear(object obj)
    {
        poolDic.Clear();
        poolObj = null;
    }
}
