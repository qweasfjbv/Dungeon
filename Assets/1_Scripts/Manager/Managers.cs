using UnityEditor.EditorTools;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Managers : MonoBehaviour
{

    static Managers s_instance;
    public static Managers Instance { get { return s_instance; } }


    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    DataManager _data = new DataManager();

    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static DataManager Data { get { return Instance._data; } }

    void Awake()
    {
        Init();
    }
    private void Start()
    {
        //SetResolution();
    }


    public void SetResolution()
    {
        int setWidth = 1920;
        int setHeight = 1080;

        Screen.SetResolution(setWidth, setHeight, true);
    }

    public void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        s_instance._resource.Init();


    }

    public static void Clear()
    {

    }
}