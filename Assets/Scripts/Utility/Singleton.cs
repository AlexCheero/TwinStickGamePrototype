using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameObject(typeof(T).Name).AddComponent<T>();
            return _instance;
        }
    }

    protected virtual void Awake()
    {
#if DEBUG
        if (_instance != null)
            throw new System.Exception(GetType().FullName + " instance already created!");
#endif
        _instance = this as T;
    }
}