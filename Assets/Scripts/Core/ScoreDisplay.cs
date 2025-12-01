using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    private bool isMoved = false;
    private int moves = 0;
    private Vector3 moveDirection = new Vector3(0, 0.2f, 0);
    private void Update()
    {
        if(moves == 5)
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
        yield return new WaitForSeconds(0.2f);
        transform.position += moveDirection;
        moves++;
        isMoved = false;
    }
}
