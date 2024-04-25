using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ��������-������е�һ�����������ڴ�Ŷ���
/// </summary>
public class PoolData
{
    //�����ж�����صĸ����
    public GameObject fatherObj;
    //���������
    public List<GameObject> poolList;

    public PoolData(GameObject obj,GameObject poolObj)
    {
        //�����봴��һ�������󲢰�����Ϊ����Pool�����¹񣩵�������
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.parent = poolObj.transform;

        poolList = new List<GameObject>() {};
        PushObj(obj);
    }

    public void PushObj(GameObject obj)
    {
        //ʧ��ö�������
        obj.SetActive(false);
        //������
        poolList.Add(obj);
        //���ø�����
        obj.transform.parent = fatherObj.transform;
    }

    public GameObject GetObj()
    {
        GameObject obj = null;
        //ȡ����һ��
        obj = poolList[0];
        poolList.RemoveAt(0);
        //����
        obj.SetActive(true);
        //�Ͽ����ӹ�ϵ
        obj.transform.parent = null;

        return obj;
    }
}

/// <summary>
/// �����ģ��
/// 1.Dictionary List
/// 2.GameObject �� Resources �����������е�API
/// </summary>
public class PoolMgr : BaseManager<PoolMgr>//�̳е���ģʽ����
{
    //������������¹�
    public Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();
    //Ԥ������ڵ�
    public GameObject poolObj;

    public void Start()
    {
        //EventCenter.GetInstance().AddEventListener("ChangeScene", Clear);
    }

    /*public GameObject GetObj(string keyName)
    {
        if (poolDic.ContainsKey(keyName) && poolDic[keyName].poolList.Count > 0)//�ж��Ѿ��������ֳ����ҳ����ں���keyName��������
        {
            obj = poolDic[keyName].GetObj();//��ֵȡ��
        }
        else//�����ֳ�������Ѿ�û����������Ļ���ʵ����һ����������һ��ʵ����ʱ�����ȴ������룬������Pushʱ�ٴ�����
        {
            obj = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/" + keyName));//Resources.Load���Խ�Ϻ������Դ����������ʹ��
            //Instantiate��¡��ȥ�����ֶ����У�clone����׺������Ҫ��obj�����ָĳɺͳ��루��ԭ��������֣�һ��
            obj.name = keyName;
        }

        return obj;
    }*/

    public void GetObj(string keyName,UnityAction<GameObject> callback)
    {
        if (poolDic.ContainsKey(keyName) && poolDic[keyName].poolList.Count > 0)//�ж��Ѿ��������ֳ����ҳ����ں���keyName��������
        {
            callback(poolDic[keyName].GetObj());
        }
        else//�����ֳ�������Ѿ�û����������Ļ���ʵ����һ����������һ��ʵ����ʱ�����ȴ������룬������Pushʱ�ٴ�����
        {
            //ͨ���첽������Դ����������ⲿ��
            ResMgr.GetInstance().LoadAsync<GameObject>(keyName, (o) =>
            {
                o.name = keyName;
                callback(o);
            });
        }
    }

    public void PushObj(string keyName,GameObject obj)//��������ķ������ڼ���һ��ʱ����Զ�invoke PushObj�������
    {
        if (poolObj == null)//�����ݵ�pool�����ڣ��½�һ��
        {
            poolObj = new GameObject("Pool");
        }
        
        if (poolDic.ContainsKey(keyName))//����г����򽫸�����ѹ��list��
        {
            poolDic[keyName].PushObj(obj);
        }
        else//���û�г���
        {
            poolDic.Add(keyName, new PoolData(obj, poolObj));//�������룬�Ұ��������Ž�ȥ
        }
    }

    /// <summary>
    /// ��ջ���صķ���
    /// ��Ҫ�����л�����ʱ
    /// </summary>
    /// <param name="obj"></param>
    public void Clear(object obj)
    {
        poolDic.Clear();
        poolObj = null;
    }
}
