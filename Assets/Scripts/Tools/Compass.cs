using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class Compass : MonoBehaviour
{
    float angle = 0;//记录北方度数
    float tempAngle = 0;//临时记录数据来判断角度变化是否大于5

    public Image compass;//指南针

    private void Start()
    {
        Input.compass.enabled = true;
    }

    private void FixedUpdate()
    {
        //Input.location.Start();

        //The heading in degrees relative to the geographic North Pole. (Read Only)
        angle = Input.compass.trueHeading;

        //为防止抖动，度数变化超过5时才赋值
        if (Mathf.Abs(tempAngle - angle) > 5)
        {
            tempAngle = angle;
            compass.transform.eulerAngles = new Vector3(0, 0, -angle);
        }
    }


    // Update is called once per frame
    /*private void FixedUpdate()
    {
        //如何确定参照物
        //当度数为  358-2度  手机的正前方就是北方

        Input.location.Start();
        text.text = " rawVector: " + Input.compass.rawVector.ToString();//用microteslas测量的原始地磁数据

        //相对应地理北极的度数 手机头正对方向  北方360/0   东方90     西方180    南方270  
        text1.text = " trueHeading: " + Input.compass.trueHeading.ToString();

        text2.text = " headingAccuracy: " + Input.compass.headingAccuracy.ToString(); //标题度数的准确度
        text3.text = " magneticHeading: " + Input.compass.magneticHeading.ToString(); //相对于磁北极的度数
        angle = Input.compass.trueHeading;

        *//*trueHeading          image/z
      北方358  360 0 2                0
       东方88  92                   90
       南方269 272                 270
       西方180                    180
        *//*

        //为防止抖动  度数变化超过二的时候才赋值
        if (Mathf.Abs(tempAngle - angle) > 5)
        {
            tempAngle = angle;
            compass.transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }*/
}