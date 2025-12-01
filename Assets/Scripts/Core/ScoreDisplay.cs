using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    private bool isMoved = false;
    private int moves = 0;
    private void Update()
    {
        if(moves == 2)
        {
            Destroy(gameObject);
        }
        if(isMoved == false)
        {
            isMoved = true;
            StartCoroutine(MoveScoreDisplay());
        }
    }
    private IEnumerator MoveScoreDisplay()
    {
        yield return new WaitForSeconds(1);
        transform.position += Vector3.up;
        moves++;
        isMoved = false;
    }
}
