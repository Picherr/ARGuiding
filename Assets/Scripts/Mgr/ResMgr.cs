using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ��Դ����ģ��
/// 1.�첽����
/// 2.ί�к�lambda���ʽ
/// 3.Э��
/// 4.����
/// </summary>
public class ResMgr : BaseManager<ResMgr>
{
    //ͬ��������Դ
    public T Load<T>(string name) where T : Object
    {
        T res = Resources.Load<T>(name);
        //���������GameObject���ͣ�ʵ�������ٷ��س�ȥ�ⲿʹ�ü���
        if (res is GameObject)
            return GameObject.Instantiate(res);
        else//TextAsset AudioClip
            return res;
    }

    //�첽������Դ
    public void LoadAsync<T>(string name, UnityAction<T> callback=null) where T : Object
    {
        //�����첽���ص�Э��
        MonoMgr.GetInstance().StartCoroutine(ReallyLoadAsync(name, callback));
    }

    //������Эͬ�����������ڿ����첽���ض�Ӧ����Դ
    private IEnumerator ReallyLoadAsync<T>(string name, UnityAction<T> callback) where T : Object
    {
        ResourceRequest req = Resources.LoadAsync<T>(name);
        yield return req;

        if (req.asset is GameObject)
        {
            callback(GameObject.Instantiate(req.asset) as T);
        }
        else
        {
            callback(req.asset as T);
        }
    }
}
