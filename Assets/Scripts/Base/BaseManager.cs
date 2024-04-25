/// <summary>
/// ����������
/// 1.����
/// 2.���ģʽ-����ģʽ
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseManager<T> where T : new()
{
    private static T instance;

    //��������-����ʽ
    public static T GetInstance()
    {
        if (instance == null)
        {
            instance = new T();
        }
        return instance;
    }
}
