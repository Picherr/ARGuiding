using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum UI_Layer
{
    Bot,
    Mid,
    Top,
    System
}

/// <summary>
/// UI������
/// 1.����������ʾ�����
/// 2.�ṩ���ⲿ��ʾ�����صȽӿ�
/// </summary>
public class UIManager : BaseManager<UIManager>
{
    public Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

    //private Transform canvas;

    private Transform bot;
    private Transform mid;
    private Transform top;
    private Transform system;

    //��¼Canvas�����󣬷����Ժ��ⲿ���ܻ�ʹ����
    public RectTransform canvas;

    public UIManager()
    {
        //�ҵ�Canvas����
        GameObject obj = ResMgr.GetInstance().Load<GameObject>("UI/Canvas");
        canvas = obj.transform as RectTransform;
        GameObject.DontDestroyOnLoad(obj);

        //�ҵ�����
        bot = canvas.Find("Bot");
        mid = canvas.Find("Mid");
        top = canvas.Find("Top");
        system = canvas.Find("System");

        //����EventSystemʹ�������ʱ�����Ƴ�
        obj = ResMgr.GetInstance().Load<GameObject>("UI/EventSystem");
        GameObject.DontDestroyOnLoad(obj);
    }

    /// <summary>
    /// ͨ���㼶ö�٣��õ���Ӧ�ĸ�����
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    public Transform GetLayerFather(UI_Layer layer)
    {
        switch (layer)
        {
            case UI_Layer.Bot:
                return this.bot;
            case UI_Layer.Mid:
                return this.mid;
            case UI_Layer.Top:
                return this.top;
            case UI_Layer.System:
                return this.system;
        }
        return null;
    }

    /// <summary>
    /// ��ʾ���
    /// </summary>
    /// <typeparam name="T">���ű�����</typeparam>
    /// <param name="panelName">�������</param>
    /// <param name="layer">��ʾ����һ��</param>
    /// <param name="callback">Ԥ���崴���ɹ���Ҫ������߼�</param>
    public void ShowPanel<T>(string panelName, UI_Layer layer, UnityAction<T> callback = null) where T : BasePanel
    {
        if (panelDic.ContainsKey(panelName))
        {
            //������崴����ɺ���߼�
            if (callback != null)
            {
                callback(panelDic[panelName] as T);
            }

            //��������ظ����أ�������ڸ���弴ֱ����ʾ�����ûص�������ֱ��return���ٴ��������첽�����߼�
            return;
        }

        ResMgr.GetInstance().LoadAsync<GameObject>("UI/" + panelName, (obj) =>
        {
            //������ΪCanvas���Ӷ���
            //�����������λ��
            Transform father = bot;
            switch (layer)
            {
                case UI_Layer.Mid:
                    father = mid;
                    break;
                case UI_Layer.Top:
                    father = top;
                    break;
                case UI_Layer.System:
                    father = system;
                    break;
            }
            //���ø�����
            obj.transform.SetParent(father);
            //�������λ�úʹ�С
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;

            (obj.transform as RectTransform).offsetMax = Vector2.zero;
            (obj.transform as RectTransform).offsetMin = Vector2.zero;

            //�õ�Ԥ�����ϵ����ű�
            T panel = obj.GetComponent<T>();
            //������崴����ɺ���߼�
            if (callback != null)
            {
                callback(panel);
            }
            //�����
            panelDic.Add(panelName, panel);
        });
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="panelName"></param>
    public void HidePanel(string panelName)
    {
        if (panelDic.ContainsKey(panelName))
        {
            GameObject.Destroy(panelDic[panelName].gameObject);
            panelDic.Remove(panelName);
        }
    }

    /// <summary>
    /// �õ�ĳһ���Ѿ���ʾ�����
    /// </summary>
    public T GetPanel<T>(string panelName) where T : BasePanel
    {
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName] as T;
        }
        return null;
    }

    /// <summary>
    /// ���ؼ�����Զ����¼�����
    /// </summary>
    /// <param name="control">�ؼ�����</param>
    /// <param name="type">�¼�����</param>
    /// <param name="callback">�¼�����Ӧ����</param>
    public static void AddCustomEventListener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callback)
    {
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = control.gameObject.AddComponent<EventTrigger>();
        }
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callback);

        trigger.triggers.Add(entry);
    }
}
