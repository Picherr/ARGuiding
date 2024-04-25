using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 输入控制模块
/// 1.Input类
/// 2.事件中心模块
/// 3.公共Mono模块的使用
/// </summary>
public class InputMgr : BaseManager<InputMgr>
{
    private bool isStart = false;
    public bool isModelSet = false;

    /// <summary>
    /// 构造函数中添加Update监听
    /// </summary>
    public InputMgr()
    {
        MonoMgr.GetInstance().AddUpdateListener(InputUpdate);
    }

    public void StartOrEndCheck(bool isOpen)
    {
        isStart = isOpen;
    }

    /// <summary>
    /// 用来检测按键抬起按下分发事件的
    /// </summary>
    /// <param name="key"></param>
    private void CheckKeyCode(KeyCode key)
    {
        /*if (Input.GetKeyDown(key))
        {
            EventCenter.GetInstance().EventTrigger("某键按下",key);
        }
        if (Input.GetKeyUp(key))
        {
            EventCenter.GetInstance().EventTrigger("某键抬起", key);
        }*/
    }

    private void InputUpdate()
    {
        //没有开启输入检测就不检测，直接return
        if (!isStart)
            return;
        //CheckKeyCode(KeyCode.W);

        if (isStart && !isModelSet)//当检测开启并且模型未创建时，才能点击创建
        {
            //检查用户是否触摸屏幕
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                //获取触摸位置
                Touch touch = Input.GetTouch(0);

                //执行地面检测
                //ARGroundPlane.GetInstance().Perform(touch.position);
            }
        }
    }
}
