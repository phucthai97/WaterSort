using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiquidController : MonoBehaviour
{
    [Space(4)]
    [Header("[INFORMATION]")]
    [SerializeField] private int _currentIndex = -1;
    [SerializeField] private UtilityManager.LiquidColor _typeLiquidColor;
    [SerializeField] private BottleController _bottle;
    [SerializeField] private Image _imgLiquid;

    public int CurrentIndex { get => _currentIndex; set => _currentIndex = value; }
    public UtilityManager.LiquidColor TypeLiquidColor { get => _typeLiquidColor; set => _typeLiquidColor = value; }
    public BottleController Bottle { get => _bottle; set => _bottle = value; }
    public Image ImgLiquid { get => _imgLiquid; }

}