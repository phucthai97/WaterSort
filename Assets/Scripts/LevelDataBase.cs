using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LevelDataBase", fileName = "NewLevelDataBase")]
public class LevelDataBase : ScriptableObject
{
    [Space(4)]
    [Header("[SETTINGS]")]
    [Range(1, 20)] [SerializeField] private int conditionToWin;
    [SerializeField] private List<GameObject> bottleList;

    /// <summary>
    /// Properties
    /// </summary>
    public List<GameObject> BottleList { get => bottleList; set => bottleList = value;}
    public int ConditionToWin { get => conditionToWin; set => conditionToWin = value; }

}


