using UnityEngine;

public class Singleton<T> where T : new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance is null)
            {
                instance = new T();
                UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
            return instance;
        }
    }

    private static void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
        instance = default;
        if (instance is null) Debug.Log("destroy " + typeof(T).ToString());
    }

    // public static T SaveInstance() => instance;
    public static void RemoveInstance() => instance = default;
    public static void RestoreInstance(T _instance) => instance = _instance;
}

public class GlobalSingleton<T> where T : new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance is null) instance = new T();
            return instance;
        }
    }
}

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    protected virtual void Awake()
    {
        Instance = this as T;
    }
}

public class GlobalSingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance is null) instance = GlobalGameObject.Instance.AddComponent<T>();
            return instance;
        }
    }
}

class GlobalGameObject
{
    private static GameObject instance;
    public static GameObject Instance
    {
        get
        {
            if (instance is null)
            {
                instance = new GameObject("Global");
                GameObject.DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }
}