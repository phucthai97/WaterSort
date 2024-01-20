using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "PositionDataBase", fileName = "NewPosDataBase")]
public class PosOfBottlesDataBase : ScriptableObject
{
    [Space(4)]
    [Header("[SETTINGS]")]
    [SerializeField] private List<PosOfBottlesEachLevels> _posOfBottlesOriginal;
    [SerializeField] private List<PosOfBottlesEachLevels> _posOfBottlesNewStyle;

    public List<PosOfBottlesEachLevels> posOfBottlesEachLevels;

    /// <summary>
    /// Chọn kiểu danh sách vị trí đặt các lọ nước dựa trên dropdown khi chọn trên UI
    /// </summary>
    public void PickListPosOfBottles(int index)
    {
        Debug.Log($"PickListPosOfBottles {index}");
        posOfBottlesEachLevels.Clear();
        posOfBottlesEachLevels = new List<PosOfBottlesEachLevels>();
        if (index == 0)
            posOfBottlesEachLevels.AddRange(_posOfBottlesOriginal);
        else
            posOfBottlesEachLevels.AddRange(_posOfBottlesNewStyle);
    }
}

[Serializable]
public class PosOfBottlesEachLevels
{
    [field: SerializeField] public List<Vector3> posOfBottles;
}
