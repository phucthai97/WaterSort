using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waterdropssplash : MonoBehaviour
{
    [Space(4)]
    [SerializeField] private List<GameObject> _waterGroupList;

    // Start is called before the first frame update
    void Start() => StartCoroutine(SetActiveLoop());

    IEnumerator SetActiveLoop()
    {
        while (true)
        {
            for (int i = 0; i < _waterGroupList.Count; i++)
            {
                if (i > 0)
                {
                    _waterGroupList[i - 1].SetActive(false);
                    _waterGroupList[i].SetActive(true);
                }
                else
                    _waterGroupList[i].SetActive(true);
                yield return new WaitForSeconds(0.012f);
            }
        }
    }

    public void SetColor(Color newColor) => 
                                _waterGroupList.ForEach(x => x.GetComponent<UnityEngine.UI.Image>().color = newColor);
}
