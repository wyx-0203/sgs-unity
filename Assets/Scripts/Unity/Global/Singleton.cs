using System.Threading.Tasks;
using UnityEngine;

public class Singleton<T> where T : new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            instance ??= new T();
            return instance;
        }
    }
}

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<T>(true);
            return instance;
        }
    }

    protected virtual void Awake()
    {
        instance = this as T;
    }
}

public class GlobalSingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static GameObject _gameObject;
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance is null)
            {
                if (_gameObject == null)
                {
                    _gameObject = new GameObject("Global");
                    DontDestroyOnLoad(_gameObject);
                }
                instance = _gameObject.AddComponent<T>();
            }
            return instance;
        }
    }
}

public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
{
    public static T Instance { get; set; }

    public static async Task Init()
    {
        if (Instance == null)
        {
            // 获取"scriptable/xxx"包中的"XXX.asset"文件
            string abName = $"scriptable/{typeof(T).ToString().ToLower()}";
            var ab = await ABManager.Instance.Load(abName);
            Instance = ab.LoadAsset<T>($"{typeof(T)}.asset");
        }
    }
}