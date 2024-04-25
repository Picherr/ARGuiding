using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Info
{
    /// <summary>
    /// ��������
    /// </summary>
    private static Dictionary<int, string> desInfo = new Dictionary<int, string>()
    {
        {1,"��������" },
        {2,"��ʮ����ʿ֮Ĺ" },
        {3,"����ԪĹ" },
        {4,"�ƻ��Ļ���" },
        {5,"����" },
    };

    /// <summary>
    /// �������
    /// </summary>
    private static Dictionary<int, string> desIntro = new Dictionary<int, string>()
    {
        {1,"\t\t��������\n������1936�꣨�����ʮ���꣩�������Ʒ���31�ס���3�ס���13�ס��Ŷ����Ի���ʯ�Կ�������ɽ������д�ġ��������桱�ĸ�������֡��Ӵ�����˵����������������" },
        {2,"\t��ʮ����ʿ֮Ĺ\n1911��4��27�գ�ũ�����¶�ʮ���գ���ͬ�˻ᷢ����������ʧ�ܣ������ס����ġ��־��񡢷�������100����ѳ�ѣ��˴�΢�����������72���ź�Ӫ��˵ء����ݻƻ�����ʮ����ʿĹ԰����Ϊ����1911��4��27�գ�ũ�����¶�ʮ��������ɽ�����쵼��ͬ�˻��ڡ��������塱���ֳƻƻ������壩ս������������ʿ�����ġ�" },
        {3,"\t\t����ԪĹ\n����ԪĹλ�ڹ㶫ʡ�����ж�ɽ������·�ƻ��ڹ�԰��1922��3��21�ձ������¾������´�ɱ������ɽ�Դ���ͳ����׷������ԪΪ½���Ͻ������ڻƻ��ڣ�������Ĺ�٣�ʯ����4�ס�" },
        {4,"\t\t�ƻ��Ļ���\n�ƻ��Ļ���λ�ڹ����лƻ��ڹ�԰��Ĺ�����࣬���١��ķ��ء����ɻƻ��ڹ�԰ԭչ����ȫ�¸�����ɣ��Ļ�������ׯ�����ۡ���Ƴ��ȴ󷽡�������ȫ���������Լ600ƽ���ף�����������˹����Ļ������ܣ��ǻƻ��ڹ�԰�Ļ�����Ͱ��������������Ҫ��ء�" },
        {5,"\t\t����\n��������1926��3�£���������������ʯ��̶��ɣ���3�ף�����Ϊ���������������Ϊ����Ծ���ţ������˸�������Ϊ�л������ڷɶ��ܶ��İ������ơ�" },
    };

    /// <summary>
    /// ��������
    /// </summary>
    private static Dictionary<int, Vector2> desCoord = new Dictionary<int, Vector2>()
    {
        //{1,new Vector2(113.245600f,23.070910f) },//����
        {1,new Vector2(113.296394f,23.139277f) },//��������
        {2,new Vector2(113.294761f,23.140487f) },//��ʮ����ʿ֮Ĺ
        {3,new Vector2(113.295087f,23.140953f) },//����ԪĹ
        {4,new Vector2(113.289800f,23.141700f) },//�ƻ��Ļ���
        {5,new Vector2(113.295082f,23.138099f) },//����
    };

    /// <summary>
    /// ��ȡ��������
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static string DesInfo(int index)
    {
        return desInfo[index];
    }

    /// <summary>
    /// ��ȡ�������
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static string DesIntro(int index)
    {
        return desIntro[index];
    }

    /// <summary>
    /// ��ȡ��������
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static Vector2 DesCoord(int index)
    {
        return desCoord[index];
    }
}
