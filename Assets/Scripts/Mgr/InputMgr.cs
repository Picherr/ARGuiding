using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������ģ��
/// 1.Input��
/// 2.�¼�����ģ��
/// 3.����Monoģ���ʹ��
/// </summary>
public class InputMgr : BaseManager<InputMgr>
{
    private bool isStart = false;
    public bool isModelSet = false;

    /// <summary>
    /// ���캯�������Update����
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
    /// ������ⰴ��̧���·ַ��¼���
    /// </summary>
    /// <param name="key"></param>
    private void CheckKeyCode(KeyCode key)
    {
        /*if (Input.GetKeyDown(key))
        {
            EventCenter.GetInstance().EventTrigger("ĳ������",key);
        }
        if (Input.GetKeyUp(key))
        {
            EventCenter.GetInstance().EventTrigger("ĳ��̧��", key);
        }*/
    }

    private void InputUpdate()
    {
        //û�п���������Ͳ���⣬ֱ��return
        if (!isStart)
            return;
        //CheckKeyCode(KeyCode.W);

        if (isStart && !isModelSet)//����⿪������ģ��δ����ʱ�����ܵ������
        {
            //����û��Ƿ�����Ļ
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                //��ȡ����λ��
                Touch touch = Input.GetTouch(0);

                //ִ�е�����
                //ARGroundPlane.GetInstance().Perform(touch.position);
            }
        }
    }
}
