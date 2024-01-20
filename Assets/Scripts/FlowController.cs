using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FlowController : MonoBehaviour
{
    private BottleController _firstBottle;
    private BottleController _secondBottle;

    [Space(4)]
    [Header("[SETTINGS]")]
    [SerializeField] private Vector3 _leftLocalPos;
    [SerializeField] private Vector3 _rightLocalPos;
    [SerializeField] private float _speedMoveDown = 2f;

    [Space(12)]
    [Header("[REFERENCES]")]
    [SerializeField] private UtilityManager _uManager;
    [SerializeField] private RectTransform _rtFlow;
    [SerializeField] private GameObject _touchPoint;
    [SerializeField] private List<UnityEngine.UI.Image> _imageChildFlow;
    private Coroutine _pouredLiquidMoveUpCoroutine;

    /// <summary>
    /// Properties
    /// </summary>
    public Vector3 LeftLocalPos { get => _leftLocalPos; }
    public Vector3 RightLocalPos { get => _rightLocalPos; }

    void FixedUpdate() =>
                    MoveDown();

    //Name it to distinguish objects when touched
    public void SetReferenceObject(BottleController fBottle, BottleController sBottle, UtilityManager argUtilityManager)
    {
        _firstBottle = fBottle;
        _secondBottle = sBottle;
        _uManager = argUtilityManager;
    }

    private void MoveDown()
    {
        Vector3 newLocalPos = _rtFlow.localPosition;
        newLocalPos.y -= _speedMoveDown;
        _rtFlow.localPosition = newLocalPos;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //If first bottle touche
        if (col.gameObject.CompareTag("Liquid") && _pouredLiquidMoveUpCoroutine == null)
        {
            // //Create particle water
            // GameObject waterDropsSplashObj = Instantiate(_uManager.PrefabDB.dropsWaterPrefab
            //                                             , _firstBottle.PouredLiquidAtSecondBottle.transform);
            // if (transform.localPosition.x < 0)
            //     waterDropsSplashObj.transform.localPosition += new Vector3(-4, 7, 0);
            // else
            //     waterDropsSplashObj.transform.localPosition += new Vector3(4, 7, 0);

            // Waterdropssplash waterdropssplash = waterDropsSplashObj.GetComponent<Waterdropssplash>();
            // waterdropssplash.SetColor(_imageChildFlow.First().color);

            _touchPoint.transform.SetParent(_secondBottle.MaskLiquids.transform);

            //Set fasle collider
            GetComponent<BoxCollider2D>().enabled = false;
            _uManager.SetActiveLiquidCollider(col.gameObject, false, 1.5f);


            //Poured Liquid move up in second-Bottle
            _pouredLiquidMoveUpCoroutine = StartCoroutine(
                                        _firstBottle.PouredLiquidMoveUp(_firstBottle.PouredLiquidAtSecondBottle.GetComponent<RectTransform>()
                                        , _touchPoint.transform.localPosition
                                        , _firstBottle.TargetLocalPosPouredLqInt));
            Destroy(_touchPoint);
        }
    }

    public void SetImageColorFlowLiquid(Color color)
    {
        _imageChildFlow.ForEach(x => x.color = color);
    }

    public void DisableImageFlow()
    {
        _imageChildFlow.ForEach(x => x.enabled = false);
    }
}
