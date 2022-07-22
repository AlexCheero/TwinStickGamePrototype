using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
#if DEBUG
            if (_instance == null)
                throw new System.Exception("no instance of " + typeof(T).FullName + " created!");
#endif
            return _instance;
        }
    }

    protected void Awake()
    {
#if DEBUG
        if (_instance != null)
            throw new System.Exception(GetType().FullName + " instance already created!");
#endif
        _instance = this as T;
    }
}