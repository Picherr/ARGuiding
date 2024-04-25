using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 面板基类
/// 1.找到所有自己面板下的控件对象
/// 2.应该提供显示或隐藏的行为
/// 3.方便我们在子类中处理逻辑
/// 4.节约找控件的工作量
/// </summary>
public class BasePanel : MonoBehaviour
{
    //通过里氏转换原则存储所有的控件
    //控件的基类是UIBehaviour
    //使用List是为了存储一个对象上的不同控件
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
    /// 显示该面板
    /// </summary>
    public virtual void Show()
    {

    }

    /// <summary>
    /// 隐藏该面板
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
    /// 找到子对象的对应控件
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

            //如果是按钮控件
            if (controls[i] is Button)
            {
                (controls[i] as Button).onClick.AddListener(() =>
                {
                    OnClick(objName);
                });
            }
            /*//如果是单选框或多选框
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
