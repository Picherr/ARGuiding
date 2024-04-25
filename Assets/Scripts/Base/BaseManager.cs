/// <summary>
/// 管理器基类
/// 1.泛型
/// 2.设计模式-单例模式
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseManager<T> where T : new()
{
    private static T instance;

    //创建单例-懒汉式
    public static T GetInstance()
    {
        if (instance == null)
        {
            instance = new T();
        }
        return instance;
    }
}
