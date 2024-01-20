using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PrefabDataBase", fileName = "NewPrefabDataBase")]
public class PrefabDataBase : ScriptableObject
{
    [Space(4)]
    [Header("[OTHER]")]
    [SerializeField] public GameObject bottlePrefab;
    [SerializeField] public GameObject liquidPrefab;
    [SerializeField] public GameObject flowPrefab;
    [SerializeField] public GameObject fingerUpPrefab;
    [SerializeField] public GameObject dropsWaterPrefab;

    [Space(12)]
    [Header("[PARTICLE SYSTEMS]")]
    [SerializeField] public GameObject parZoomOutInSquare;
    [SerializeField] public GameObject parShootPaper;
    [SerializeField] public GameObject parShootTinselPaper1;
    [SerializeField] public GameObject parSurfaceAirBubbles;
    [SerializeField] public GameObject parLightEffect;

    [Space(12)]
    [Header("[SCRIPTABLE OBJECT]")]
    [SerializeField] public LevelDataBase levelDataBasePrefab;
}
