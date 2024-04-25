using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class Compass : MonoBehaviour
{
    float angle = 0;//��¼��������
    float tempAngle = 0;//��ʱ��¼�������жϽǶȱ仯�Ƿ����5

    public Image compass;//ָ����

    private void Start()
    {
        Input.compass.enabled = true;
    }

    private void FixedUpdate()
    {
        //Input.location.Start();

        //The heading in degrees relative to the geographic North Pole. (Read Only)
        angle = Input.compass.trueHeading;

        //Ϊ��ֹ�����������仯����5ʱ�Ÿ�ֵ
        if (Mathf.Abs(tempAngle - angle) > 5)
        {
            tempAngle = angle;
            compass.transform.eulerAngles = new Vector3(0, 0, -angle);
        }
    }


    // Update is called once per frame
    /*private void FixedUpdate()
    {
        //���ȷ��������
        //������Ϊ  358-2��  �ֻ�����ǰ�����Ǳ���

        Input.location.Start();
        text.text = " rawVector: " + Input.compass.rawVector.ToString();//��microteslas������ԭʼ�ش�����

        //���Ӧ�������Ķ��� �ֻ�ͷ���Է���  ����360/0   ����90     ����180    �Ϸ�270  
        text1.text = " trueHeading: " + Input.compass.trueHeading.ToString();

        text2.text = " headingAccuracy: " + Input.compass.headingAccuracy.ToString(); //���������׼ȷ��
        text3.text = " magneticHeading: " + Input.compass.magneticHeading.ToString(); //����ڴű����Ķ���
        angle = Input.compass.trueHeading;

        *//*trueHeading          image/z
      ����358  360 0 2                0
       ����88  92                   90
       �Ϸ�269 272                 270
       ����180                    180
        *//*

        //Ϊ��ֹ����  �����仯��������ʱ��Ÿ�ֵ
        if (Mathf.Abs(tempAngle - angle) > 5)
        {
            tempAngle = angle;
            compass.transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }*/
}