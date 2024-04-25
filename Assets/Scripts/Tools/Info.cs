using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Info
{
    /// <summary>
    /// 景点名称
    /// </summary>
    private static Dictionary<int, string> desInfo = new Dictionary<int, string>()
    {
        {1,"浩气长存" },
        {2,"七十二烈士之墓" },
        {3,"邓仲元墓" },
        {4,"黄花文化馆" },
        {5,"龙柱" },
    };

    /// <summary>
    /// 景点介绍
    /// </summary>
    private static Dictionary<int, string> desIntro = new Dictionary<int, string>()
    {
        {1,"\t\t浩气长存\n竣工于1936年（民国二十五年）。正门牌坊长31米、宽3米、高13米。门额上以花岗石镌刻着孙中山先生题写的“浩气长存”四个贴金大字。从词义上说，浩气就是正气。" },
        {2,"\t七十二烈士之墓\n1911年4月27日（农历三月二十九日），同盟会发动广州起义失败，喻培伦、林文、林觉民、方声洞等100多人殉难，潘达微先生将收殓的72具遗骸营葬此地。广州黄花岗七十二烈士墓园，是为纪念1911年4月27日（农历三月二十九日孙中山先生领导的同盟会在“广州起义”（又称黄花岗起义）战役中牺牲的烈士而建的。" },
        {3,"\t\t邓仲元墓\n邓仲元墓位于广东省广州市东山区先烈路黄花岗公园。1922年3月21日被军阀陈炯明部下刺杀。孙中山以大总统名义追赠邓仲元为陆军上将，葬于黄花岗，并亲书墓碣，石碑高4米。" },
        {4,"\t\t黄花文化馆\n黄花文化馆位于广州市黄花岗公园主墓道西侧，毗临“四方池”，由黄花岗公园原展厅经全新改造而成，文化馆外形庄重美观、设计沉稳大方、功能齐全，建筑面积约600平方米，极大地提升了公共文化服务功能，是黄花岗公园文化建设和爱国主义教育的重要阵地。" },
        {5,"\t\t龙柱\n龙柱建于1926年3月，用著名的连柱青石雕刻而成，高3米，柱身为倒卷的青龙，柱底为鲤鱼跃龙门，体现了革命先烈为中华民族腾飞而奋斗的磅礴气势。" },
    };

    /// <summary>
    /// 景点坐标
    /// </summary>
    private static Dictionary<int, Vector2> desCoord = new Dictionary<int, Vector2>()
    {
        //{1,new Vector2(113.245600f,23.070910f) },//测试
        {1,new Vector2(113.296394f,23.139277f) },//浩气长存
        {2,new Vector2(113.294761f,23.140487f) },//七十二烈士之墓
        {3,new Vector2(113.295087f,23.140953f) },//邓仲元墓
        {4,new Vector2(113.289800f,23.141700f) },//黄花文化馆
        {5,new Vector2(113.295082f,23.138099f) },//龙柱
    };

    /// <summary>
    /// 获取景点名称
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static string DesInfo(int index)
    {
        return desInfo[index];
    }

    /// <summary>
    /// 获取景点介绍
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static string DesIntro(int index)
    {
        return desIntro[index];
    }

    /// <summary>
    /// 获取景点坐标
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static Vector2 DesCoord(int index)
    {
        return desCoord[index];
    }
}
