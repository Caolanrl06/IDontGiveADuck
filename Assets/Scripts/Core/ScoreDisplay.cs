using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _textMeshPro;
    private GoodDuck gDuck;
    private DecoyDuck dDuck;
    public bool goodDuckClicked;

    void Start()
    {
        int pModifyier = gDuck.PointValue;
        int nModifyier = dDuck.timePenalty;
        
        if(goodDuckClicked == true)
        {
            _textMeshPro.text = $"+{pModifyier}";
        }
        else if(goodDuckClicked == false)
        {
            _textMeshPro.text = $"-{nModifyier} seconds";
        }
        Destroy(gameObject, 2);
    }
  
}
