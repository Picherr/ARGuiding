using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ������
/// 1.�ҵ������Լ�����µĿؼ�����
/// 2.Ӧ���ṩ��ʾ�����ص���Ϊ
/// 3.���������������д����߼�
/// 4.��Լ�ҿؼ��Ĺ�����
/// </summary>
public class BasePanel : MonoBehaviour
{
    //ͨ������ת��ԭ��洢���еĿؼ�
    //�ؼ��Ļ�����UIBehaviour
    //ʹ��List��Ϊ�˴洢һ�������ϵĲ�ͬ�ؼ�
    private Dictionary<string, List<UIBehaviour>> controlDic = new Dictionary<string, List<UIBehaviour>>();

    protected virtual void Awake()
    {
        FindChildrenControl<Button>();
        FindChildrenControl<Text>();
        FindChildrenControl<Image>();
        FindChildrenControl<Slider>();
        FindChildrenControl<ScrollRect>();
        FindChildrenControl<InputField>();
        FindChildrenControl<TextMeshProUGUI>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    protected virtual void OnDestroy()
    {
        
    }

    /// <summary>
    /// ��ʾ�����
    /// </summary>
    public virtual void Show()
    {

    }

    /// <summary>
    /// ���ظ����
    /// </summary>
    public virtual void Hide()
    {

    }

    protected virtual void OnClick(string btnName)
    {

    }

    protected virtual void OnValueChanged(string inputFieldName, string s)
    {

    }

    public virtual T GetControl<T>(string name) where T : UIBehaviour
    {
        if (controlDic.ContainsKey(name))
        {
            for (int i = 0; i < controlDic[name].Count; ++i)
            {
                if (controlDic[name][i] is T)
                    return controlDic[name][i] as T;
            }
        }
        return null;
    }

    /// <summary>
    /// �ҵ��Ӷ���Ķ�Ӧ�ؼ�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private void FindChildrenControl<T>() where T : UIBehaviour
    {
        T[] controls = this.GetComponentsInChildren<T>();
        for (int i = 0; i < controls.Length; i++)
        {
            string objName = controls[i].gameObject.name;
            if (controlDic.ContainsKey(objName))
            {
                controlDic[objName].Add(controls[i]);
            }
            else
            {
                controlDic.Add(objName, new List<UIBehaviour>() { controls[i] });
            }

            //����ǰ�ť�ؼ�
            if (controls[i] is Button)
            {
                (controls[i] as Button).onClick.AddListener(() =>
                {
                    OnClick(objName);
                });
            }
            /*//����ǵ�ѡ����ѡ��
            else if (controls[i] is InputField)
            {
                (controls[i] as InputField).onValueChanged.AddListener((s) =>
                {
                    OnValueChanged(objName, s);
                });
            }*/
        }
    }
}
