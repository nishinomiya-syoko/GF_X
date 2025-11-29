using UnityEngine;

/// <summary>
/// 单例
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    /*单例类：继承monobehaviour ,所以场景一旦加载，就会生成一次新实例，切换场景容易导致出现多个单例对象，无法阻止其创建
     * 如果切换场景，此场景中含有单例，则先摧毁，在创建
     * 
     * 
     */
{
    public bool global = true;
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType<T>();
            }
            return instance;
        }

    }


    private void Awake()
    {
        Debug.LogWarningFormat("{0}{1} awake" ,typeof(T),this.GetInstanceID());
        if (global)
        {
            //如果单例脚本不为空，或者不是这个的脚本
            if (instance != null && instance != this.gameObject.GetComponent<T>())
            {
                Destroy(this.gameObject);
                return;
            }
            DontDestroyOnLoad(this.gameObject);
            instance = gameObject.GetComponent<T>();

        }
        this.OnStart();
    }

    protected virtual void OnStart()
    {

    }
}
