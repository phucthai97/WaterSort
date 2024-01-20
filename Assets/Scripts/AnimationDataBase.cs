using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AnimationDataBase", fileName = "NewAnimationDataBase")]
public class AnimationDataBase : ScriptableObject
{
    [Space(12)]
    [Header("ANIMATION GENERAL")]
    public AnimationCurve moveUpCurve;
    public AnimationCurve moveAndPourCurve;
    public AnimationCurve rotateCurveStep1;
    public AnimationCurve pouredLiquidMoveUp;
    public AnimationCurve rotateCurveStep2;
    public float moveTimeUpAndDown = 0.3f;
    public float moveAndPourTimeStep1 = 2f;
    public float moveAndPourTimeStep2 = 0.7f;

    [Space(12)]
    [Header("ANIMATIONS")]
    public List<PourAnimation> PourAnimations;
    public List<Color32> colorList;
    public List<Color32> particleColorList;
    public List<Color32> ligthEffectColorList;
}

[Serializable]
public class PourAnimation
{
    [field: SerializeField] public string Name {get; private set;}
    [Space(4)]
    [Header("[FOR RORATION OF FISTBOTTLE]")]
    //Set threshold for detect rotation of first-Bottle to spawn flow of liqid
    [field: SerializeField] public float offsetZTargetDegrees;
    [field: SerializeField] public List<float> offsetZThreshHoldDegressToSpawnFlow;

    [Space(15)]
    //For liquids parent in first-Bottle will be move up
    [Header("[FOR LIQUIDS PARENT MOVE UP]")]
    [field: SerializeField] public AnimationCurve liquidsParentMoveUpStep1;
    [field: SerializeField] public AnimationCurve liquidsParentMoveDownStep2;
    [field: SerializeField] public List<Vector3> targetLiquidsParentMoveUp ;

    [Space(15)]
    //For surface liquid in first-Bottle will be move down
    [Header("[FOR LIQUID MOVE DOWN]")]
    [field: SerializeField] public List<AnimationCurve> liquidMoveDownStep1;
    [field: SerializeField] public List<float> ratioOffsetPosYLq ;
    [field: SerializeField] public List<AnimationCurve> midLiquidLayerMoveDownStep1 ;
    [field: SerializeField] public List<AnimationCurve> midLiquidLayerMoveUpStep2 ;
}

[Serializable]
public class PosofBottlesEachLevel
{
    [field: SerializeField] public List<Vector3> posOfBottles;
}