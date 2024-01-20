using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UtilityManager : MonoBehaviour
{
    [Space(4)]
    [Header("[INFORMATION]")]
    [SerializeField] private List<Vector3Int> _globalPosLiquids;
    private BottleController _firstBottleClicked;

    [Space(12)]
    [Header("[REFERENCE]")]
    [SerializeField] private GameObject _canvasFullBottle;
    [SerializeField] private LevelManager _lvManager;
    [SerializeField] private SoundManager _soundManager;
    [SerializeField] private StepGuide _stepGuide;

    [Space(12)]
    [Header("[DATABASE]")]
    [SerializeField] public AnimationDataBase animaDB;
    [SerializeField] public PosOfBottlesDataBase posDB;
    [SerializeField] private PrefabDataBase _prefabDB;

    /// <summary>
    /// Enum add new color type
    /// </summary>
    public enum LiquidColor
    {
        Lightpurple = 0, Fuchsia = 1, Navajowhite = 2, Lightsalmon = 3, Lightskyblue = 4, Hotpink = 5
        , Mediumspringgreen = 6, Mediumorchid = 7, Lightskblue = 8, Gray = 9, Paleturquoise = 10
        , Mediumvioletred = 11, Empty = 99
    };
    public enum StateType { Idlle, Selected, Pouring };

    /// <summary>
    /// Properties
    /// </summary>
    public BottleController FirstBottleClicked { get => _firstBottleClicked; set => _firstBottleClicked = value; }
    public PrefabDataBase PrefabDB { get => _prefabDB; }
    public GameObject CanvasFullBottle { get => _canvasFullBottle; }
    public LevelManager LvManager { get => _lvManager; }
    public SoundManager SoundManager { get => _soundManager; }
    public StepGuide StepGuide { get => _stepGuide; }

    void Update() => CheckSecondClicked();

    private void CheckSecondClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //MoveUp is NOT executing
            if (_firstBottleClicked != null)
            {
                if (_firstBottleClicked.StateBottle == StateType.Selected && _firstBottleClicked.ElapsedTimeChoose > 0.1f)
                {
                    _firstBottleClicked.MoveDown();
                    _firstBottleClicked.ElapsedTimeChoose = 0;
                    _firstBottleClicked = null;
                }
            }
        }
    }

    public void InitialLayerColorInBottle(List<LiquidColor> layersOfLiquid, BottleController thisBottle)
    {
        Vector3 localPosTop = thisBottle.RtTopPointLq.localPosition;
        Vector3 localPosBottom = thisBottle.RtBottomPointLq.localPosition;

        List<Vector3> localPosLiquids = CalculateLocalPosLiquids(layersOfLiquid.Count, localPosTop, localPosBottom);
        for (int i = layersOfLiquid.Count - 1; i >= 0; i--)
        {
            //If layer lquid not empty
            if (layersOfLiquid[i] != LiquidColor.Empty)
            {
                GameObject objLiquid = Instantiate(_prefabDB.liquidPrefab, thisBottle.transform);

                //Set localPos at bottle.tranform calculate base on localPosTop and localPosBottom
                objLiquid.transform.localPosition = localPosLiquids[i];

                //Set current of liquid for dertermind targetPosLiquid in ScriptableObject
                Vector3Int referPosInt = new Vector3Int(0, (int)localPosLiquids[i].y, 0);
                objLiquid.GetComponent<LiquidController>().CurrentIndex = _globalPosLiquids.IndexOf(referPosInt);

                //Set name
                objLiquid.name = $"{layersOfLiquid[i]} Liquid";

                //Set typeColor for LiquidController of instance
                objLiquid.GetComponent<LiquidController>().TypeLiquidColor = layersOfLiquid[i];
                objLiquid.GetComponent<LiquidController>().Bottle = thisBottle;

                //Get & set color of liquid
                GameObject liquidImage = objLiquid.transform.GetChild(0).gameObject;
                liquidImage.GetComponent<UnityEngine.UI.Image>().color = animaDB.colorList[(int)layersOfLiquid[i]];

                //Get & set color childs of liquid
                Waving smallWave = liquidImage.transform.GetChild(0).GetComponent<Waving>();
                smallWave.SetColorWaves(animaDB.colorList[(int)layersOfLiquid[i]]);

                //SetParent in fill object
                objLiquid.transform.SetParent(thisBottle.Liquids.transform);

                //Add liquid into the list
                if (thisBottle.LiquidObjects.Count == 0)
                {
                    thisBottle.InsertLiquidObjects(objLiquid, layersOfLiquid[i]);

                    //Determine surface liquid
                    thisBottle.RtSurfaceLiquid = objLiquid.GetComponent<RectTransform>();
                    thisBottle.SurfaceLiquidColor = layersOfLiquid[i];
                }
                else if (thisBottle.LiquidObjects.Count > 0)
                {
                    //If liquid has the same type color as before -> don't adding & remove it!
                    if (thisBottle.LastLiquidColor == layersOfLiquid[i])
                    {
                        thisBottle.LiquidObjects.Insert(0, null);
                        Destroy(objLiquid);
                    }
                    else
                        thisBottle.InsertLiquidObjects(objLiquid, layersOfLiquid[i]);
                }
            }
        }
    }

    //Calculate the localPosition of each liquid layer
    List<Vector3> CalculateLocalPosLiquids(int countLayer, Vector3 toplocalPos, Vector3 bottomLocalPos)
    {
        List<Vector3> collectLocalPos = new List<Vector3>();

        //Calculate half the height of liquids 
        float halfHeight = (toplocalPos.y - bottomLocalPos.y) / countLayer / 2;

        //Set start from top to bottom on Y-axis
        float startTopPosY = toplocalPos.y;

        for (int i = 0; i < countLayer; i++)
        {
            float height = halfHeight * 2;
            height -= 4f;
            collectLocalPos.Insert(0, new Vector3(toplocalPos.x, startTopPosY - (i * height), toplocalPos.z));
        }
        return collectLocalPos;
    }

    //Determine the direction and location to pour
    public Vector3 CalculateToPassParaToPouring(BottleController firstBottle, BottleController secondBottle)
    {
        //Declare variables
        Vector3 startPosLiquidsParent = firstBottle.Liquids.GetComponent<RectTransform>().localPosition;
        Vector3 targetPosLiquidsParent;
        Vector3 startPosLiquid = firstBottle.RtSurfaceLiquid.localPosition;
        Vector3 targetPosLiquid;
        Quaternion targetDegrees;
        Quaternion thresholdDegreeToPouredMoveUp;
        Vector3 posForPour = secondBottle.RawPosForPour;

        int referIndex = FindIndexInScriptableObject(firstBottle.LiquidObjects);
        int currentIndex = firstBottle.RtSurfaceLiquid.GetComponent<LiquidController>().CurrentIndex;

        //Calculate precise value
        float preciseWidthBottle = secondBottle.Rect.rect.width * secondBottle.Rect.localScale.x;
        float preciseHeightFirstLiquid = firstBottle.RtSurfaceLiquid.rect.height * firstBottle.RtSurfaceLiquid.localScale.y;
        Vector3 rotation = firstBottle.Rect.localRotation.eulerAngles;

        //Case1: firstBottle (Left) < secondBottle (Right)
        if (firstBottle.transform.localPosition.x <= secondBottle.transform.localPosition.x)
        {
            targetPosLiquidsParent = animaDB.PourAnimations[referIndex].targetLiquidsParentMoveUp[currentIndex];
            //FirstBottle will move to this position to pour water
            posForPour.x = secondBottle.RawPosForPour.x - preciseWidthBottle * 0.225f;

            targetDegrees = Quaternion.Euler(0, 0, rotation.z - animaDB.PourAnimations[referIndex].offsetZTargetDegrees);
            thresholdDegreeToPouredMoveUp = Quaternion.Euler(0, 0, rotation.z - animaDB.PourAnimations[referIndex].offsetZThreshHoldDegressToSpawnFlow[currentIndex]);
        }

        //Case2: secondBottle (Left) < firstBottle (Right)
        else
        {
            targetPosLiquidsParent = animaDB.PourAnimations[referIndex].targetLiquidsParentMoveUp[currentIndex];
            targetPosLiquidsParent.x = -targetPosLiquidsParent.x;
            //FirstBottle will move to this position to pour water
            posForPour.x = secondBottle.RawPosForPour.x + preciseWidthBottle * 0.225f;

            targetDegrees = Quaternion.Euler(0, 0, rotation.z + animaDB.PourAnimations[referIndex].offsetZTargetDegrees);
            thresholdDegreeToPouredMoveUp = Quaternion.Euler(0, 0, rotation.z + animaDB.PourAnimations[referIndex].offsetZThreshHoldDegressToSpawnFlow[currentIndex]);
        }
        //print($"offsetZThreshHoldDegressFlow {animaDB.PourAnimations[referIndex].offsetZThreshHoldDegressToSpawnFlow[currentIndex]}");

        //The liquid in first-Bottle will moveup to this position when water is poured
        float offSetMoveDownPosy = startPosLiquid.y - (preciseHeightFirstLiquid * animaDB.PourAnimations[referIndex].ratioOffsetPosYLq[currentIndex]);
        targetPosLiquid = new Vector3(startPosLiquid.x
                                    , offSetMoveDownPosy
                                    , startPosLiquid.z);


        print($"referIndex {referIndex} - currentIndex {currentIndex} | value: {animaDB.PourAnimations[referIndex].ratioOffsetPosYLq[currentIndex]} | offSetPosy {offSetMoveDownPosy}");

        //For refer Animation in Scriptabl object
        AnimationCurve liquidsParentMoveUpStep1 = animaDB.PourAnimations[referIndex].liquidsParentMoveUpStep1;
        AnimationCurve liquidsParentMoveDownStep2 = animaDB.PourAnimations[referIndex].liquidsParentMoveDownStep2;
        AnimationCurve liquidMoveStep1 = animaDB.PourAnimations[referIndex].liquidMoveDownStep1[currentIndex];

        //Pass all parameter for first
        firstBottle.SetParaForPouring(startPosLiquidsParent, targetPosLiquidsParent, startPosLiquid, targetPosLiquid
                                    , targetDegrees, thresholdDegreeToPouredMoveUp
                                    , liquidMoveStep1
                                    , liquidsParentMoveUpStep1, liquidsParentMoveDownStep2);

        return posForPour;
    }

    public Vector3Int CreatePouredLiquidAndReturnTargetLocalPos(BottleController firstBottle, BottleController secondBottle, GameObject pouredLiquid)
    {
        //Set name of pouredLiquid
        pouredLiquid.name = $"{firstBottle.SurfaceLiquidColor} Liquid {secondBottle.LiquidObjects.Count}";

        //Set typeLiquidColor for pouredLiquid
        pouredLiquid.GetComponent<LiquidController>().TypeLiquidColor = firstBottle.SurfaceLiquidColor;

        //Set bottle for pouredLiquid
        pouredLiquid.GetComponent<LiquidController>().Bottle = secondBottle;

        //Set color liquid image
        GameObject liquidImage = pouredLiquid.transform.GetChild(0).gameObject;
        liquidImage.GetComponent<UnityEngine.UI.Image>().color = animaDB.colorList[(int)firstBottle.SurfaceLiquidColor];

        //Get & set color childs of liquid
        Waving childSmallWaving = liquidImage.transform.GetChild(0).GetComponent<Waving>();
        childSmallWaving.SetColorWaves(animaDB.colorList[(int)firstBottle.SurfaceLiquidColor]);

        //Get & set color childs of liquid
        Waving childBigWaving = liquidImage.transform.GetChild(1).GetComponent<Waving>();
        childBigWaving.SetColorWaves(animaDB.colorList[(int)firstBottle.SurfaceLiquidColor]);
        firstBottle.ChildBigWaveSecondBottle = childBigWaving.gameObject;

        //Add into surfaceLiquid
        secondBottle.RtSurfaceLiquid = pouredLiquid.GetComponent<RectTransform>();
        secondBottle.SurfaceLiquidColor = firstBottle.SurfaceLiquidColor;

        //Count layer liquid at second-Bottle after pouring
        int countLayersLiquid = secondBottle.LiquidObjects.Count + CountLayerSameTypeOnSurface(firstBottle.LiquidObjects);

        //Set target localPos for poured-liquid move up
        pouredLiquid.transform.localPosition = _globalPosLiquids[countLayersLiquid - 1];
        pouredLiquid.transform.SetParent(secondBottle.Liquids.transform);
        Vector3 targetLocalPosPouredLq = pouredLiquid.transform.localPosition;

        //Move pouredLiquid to the top Liquid.transform in Hierachy to its sprite behind all the other sprites
        pouredLiquid.transform.SetAsFirstSibling();

        //Set start-localPos of poured liquid at bottom bottle at liquids.transform parent
        pouredLiquid.transform.localPosition = new Vector3(0, -245f, 0);

        //Set currentIndex for pouredLiquid
        Vector3Int localPosPouredAtSecondBottle = GetLocalPosReferParent(secondBottle, targetLocalPosPouredLq);
        pouredLiquid.GetComponent<LiquidController>().CurrentIndex = _globalPosLiquids.IndexOf(localPosPouredAtSecondBottle);
        return new Vector3Int(Mathf.RoundToInt(targetLocalPosPouredLq.x), Mathf.RoundToInt(targetLocalPosPouredLq.y), Mathf.RoundToInt(targetLocalPosPouredLq.z));
    }

    public IEnumerator CreateFlowLiquid(BottleController fBottle, BottleController sBottle)
    {
        //WaitForSeconds waitForSeconds = new WaitForSeconds(fBottle.moveAndPourTimeStep1 * 0.75f);
        yield return new WaitForSeconds(0.01f);

        //Create water flow
        GameObject objectFlow = Instantiate(_prefabDB.flowPrefab.gameObject, sBottle.transform);

        fBottle.flowController = objectFlow.GetComponent<FlowController>();
        fBottle.flowController.SetReferenceObject(fBottle, sBottle, this);

        //Check left or right
        if (fBottle.transform.localPosition.x <= sBottle.transform.localPosition.x)
            objectFlow.GetComponent<RectTransform>().localPosition = fBottle.flowController.LeftLocalPos;
        else
            objectFlow.GetComponent<RectTransform>().localPosition = fBottle.flowController.RightLocalPos;

        //SetParent for flow to cover it by MaskLiquid
        objectFlow.transform.SetParent(sBottle.MaskFlow.transform);

        //Move objectFlow to the top secondBottle.transform in Hierachy to its sprite behind all the other sprites
        objectFlow.transform.SetAsFirstSibling();

        //Set color flow
        fBottle.flowController.SetImageColorFlowLiquid(animaDB.colorList[(int)fBottle.SurfaceLiquidColor]);
        _soundManager.PlayPourLiquidSFX(Camera.main.transform.position);
        fBottle.ChildBigWaveSecondBottle.SetActive(true);
    }

    public bool CheckBottleFullFill(List<GameObject> liquidObjects, int posYInt)
    {
        if (liquidObjects.Count < 4)
            return false;

        int countNull = liquidObjects.Where(x => x == null).Count();

        if (countNull == 3 && posYInt == 145)
            return true;
        else
            return false;
    }

    public void SpawnParticleSystem(int indexColor, BottleController bottleFullFill)
    {
        float scaleFactor = bottleFullFill.GetComponent<RectTransform>().localScale.x;
        //print($"scaleFactor {scaleFactor}");

        GameObject inst = Instantiate(_prefabDB.parZoomOutInSquare, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, 50, -100);

        inst = Instantiate(_prefabDB.parShootPaper, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, 50, -100);

        inst = Instantiate(_prefabDB.parShootTinselPaper1, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, 50, -100);

        inst = Instantiate(_prefabDB.parLightEffect, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, -95, -100);
        ParticleSystem particleSystem = inst.GetComponent<ParticleSystem>();
        var mainModule = particleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(animaDB.ligthEffectColorList[indexColor]);

        inst = Instantiate(_prefabDB.parLightEffect, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, -275, -100);
        particleSystem = inst.GetComponent<ParticleSystem>();
        mainModule = particleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(animaDB.ligthEffectColorList[indexColor]);

        inst = Instantiate(_prefabDB.parSurfaceAirBubbles, bottleFullFill.transform);
        inst.transform.localScale *= scaleFactor;
        inst.transform.localPosition = new Vector3(0, -55, -200);
        particleSystem = inst.GetComponent<ParticleSystem>();
        mainModule = particleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(Color.white, animaDB.particleColorList[indexColor]);

        bottleFullFill.parBottomAirBubbles.transform.localScale *= scaleFactor;
        particleSystem = bottleFullFill.parBottomAirBubbles;
        mainModule = particleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(animaDB.particleColorList[indexColor], Color.white);
        bottleFullFill.parBottomAirBubbles.GetComponent<ParticleSystemRenderer>().enabled = true;
    }

    public Vector3Int GetLocalPosReferParent(BottleController parentBottle, Vector3 localPosInLiquid)
    {
        GameObject temp = new GameObject();
        temp.transform.SetParent(parentBottle.Liquids.transform);
        temp.transform.localPosition = localPosInLiquid;
        temp.transform.SetParent(parentBottle.transform);
        Vector3Int referPos = new Vector3Int(0, Mathf.RoundToInt(temp.transform.localPosition.y), 0);
        Destroy(temp);

        return referPos;
    }

    public void CheckAllNullOrNotInList(List<GameObject> argLiquidObjects)
    {
        bool allElementInListIsNull = true;
        foreach (GameObject child in argLiquidObjects)
            if (child != null) allElementInListIsNull = false;

        if (allElementInListIsNull)
            argLiquidObjects.Clear();
    }

    //number layer will be pour from first-Bottle to second-Bottle
    public int CountLayerSameTypeOnSurface(List<GameObject> argLiquidObjects)
    {
        int countLayer = 0;
        for (int i = argLiquidObjects.Count - 1; i >= 0; i--)
        {
            //First iterate
            if (i == argLiquidObjects.Count - 1)
            {
                if (argLiquidObjects[i] != null)
                    countLayer++;
                else
                    Debug.LogError($"This argLiquidObjects has null at the last element!");
            }
            //And after...
            else
            {
                if (argLiquidObjects[i] == null)
                    countLayer++;
                else
                    break;
            }
        }
        return countLayer;
    }

    //Find the index that calculates from that index to the surface of the water and all the water will be poured out!
    public int FindIndexInScriptableObject(List<GameObject> argLiquidObjects)
    {
        int lastIndex = -1;
        for (int i = argLiquidObjects.Count - 1; i >= 0; i--)
        {
            //First iterate
            if (lastIndex == -1)
            {
                if (argLiquidObjects[i] != null)
                    lastIndex = i;
            }
            //And after...
            else
            {
                if (argLiquidObjects[i] == null)
                {
                    lastIndex = i;
                    if (i > 0)
                    {
                        if (argLiquidObjects[i - 1] != null)
                            break;
                    }
                }
                else
                    break;
            }
        }

        return lastIndex;
    }

    public void SetActiveLiquidCollider(GameObject surfaceLiquid, bool val, float startDelay) =>
                                        StartCoroutine(SetActiveColliderCoroutine(surfaceLiquid, val, startDelay));


    public IEnumerator SetActiveColliderCoroutine(GameObject surfaceLiquid, bool val, float startDelay)
    {
        //Wait time for case multiple bottles are poured at the same time
        // -> Need to wait a few seconds to turn off collider
        yield return new WaitForSeconds(startDelay);

        if (surfaceLiquid != null)
        {
            BoxCollider2D[] boxCollider2Ds = surfaceLiquid.GetComponents<BoxCollider2D>();
            foreach (BoxCollider2D collider in boxCollider2Ds)
            {
                if (collider != null)
                    collider.enabled = val;
            }
        }
    }

    public RectTransform FindRTSurfaceLiquid(List<GameObject> argLiquidObjects)
    {
        RectTransform rectTrans = null;
        if (argLiquidObjects.Count > 0)
        {
            for (int i = argLiquidObjects.Count - 1; i >= 0; i--)
            {
                if (argLiquidObjects[i] != null)
                {
                    rectTrans = argLiquidObjects[i].GetComponent<RectTransform>();
                    break;
                }
                else
                {
                    //Remove null element in list
                    argLiquidObjects.RemoveAt(i);
                }
            }
        }
        return rectTrans;
    }

    private int CalculateSumFromNToZero(int n)
    {
        if (n == 0)
            return 0;
        else
            return n + CalculateSumFromNToZero(n - 1);
    }
}
