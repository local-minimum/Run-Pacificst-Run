using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T>: MonoBehaviour where T : MonoBehaviour
{
    private static T _Instance;
    public static T Instance {
        get {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<T>();
            }
            return _Instance;
        }        
        private set => _Instance = value;
    }

    [SerializeField, Tooltip("Either entire object or just script")]
    bool destroyObjectOnConflict = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = GetComponent<T>();
            LateAwake();
        } else if (Instance == this) {
            LateAwake();
        } else
        {
            if (destroyObjectOnConflict)
            {
                Destroy(gameObject);
            } else
            {
                Destroy(this);
            }
        }        
    }

    virtual protected void LateAwake() {}

    private void OnDestroy()
    {
        if (_Instance == this) _Instance = null;
    }
}
