using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string _nameScene;

    /// <summary>
    /// SingleTon Parttern
    /// </summary>
    private static GameManager inst;
    public static GameManager Inst { get => inst; }

    void ManagerSingleTon()
    {
        if (inst != null)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            inst = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {  
        ManagerSingleTon();
    }
}
