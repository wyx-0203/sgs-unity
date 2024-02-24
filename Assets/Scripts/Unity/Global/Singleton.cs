using System.Threading.Tasks;
using UnityEngine;

// public class Singleton<T> where T : new()
// {
//     private static T instance;
//     public static T Instance
//     {
//         get
//         {
//             if (instance is null)
//             {
//                 instance = new T();
//                 UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
//             }
//             return instance;
//         }
//     }

//     private static void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
//     {
//         UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
//         instance = default;
//         // if (instance is null) Debug.Log("destroy " + typeof(T).ToString());
//     }

//     // public static T SaveInstance() => instance;
//     public static void NewInstance() => instance = new();
//     public static void SetInstance(T _instance) => instance = _instance;
// }

public class GlobalSingleton<T> where T : new()
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
    public static T Instance;
    public static async Task Init()
    {
        if (Instance is null)
        {
            string abName = $"scriptable/{typeof(T).ToString().ToLower()}";
            var ab = await ABManager.Instance.Load(abName);
            Instance = ab.LoadAsset<T>($"{typeof(T)}.asset");
        }
    }
}