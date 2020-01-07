//2019-9-14 单例泛型类
using UnityEngine;

public class UnitySingleton<T> : MonoBehaviour 
    where T : Component {

    private static T _instance;
    public static T Instance {
        get {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(T)) as T;
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.hideFlags = HideFlags.HideAndDontSave;//隐藏实例化的new game object，下同
                    _instance = (T)obj.AddComponent(typeof(T));
                }
            }
            return _instance;
        }
    }
}
