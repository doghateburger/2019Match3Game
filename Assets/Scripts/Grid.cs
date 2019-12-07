using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Vector2 gridPos;
    [SerializeField] float moveSpeed = 2f;
    private GameBoardManager gbm;
    private GameStateManager gsm;
    private ScoreManager sm;

    private Vector2 mouseDownPos;
    private Vector2 mouseUpPos;
    private GameObject previousParent;
    public bool childInPos;


    void Awake()
    {
        gbm = FindObjectOfType<GameBoardManager>();
        gsm = FindObjectOfType<GameStateManager>();
        sm = FindObjectOfType<ScoreManager>();
        GetGemObj(transform.position);
    }

    private void Update()
    {

        if (transform.childCount > 0)
        {
            childInPos = transform.GetChild(0).transform.position == transform.position; //childinpos is true only when the transform match up

            if (!childInPos) // other wise move until position is touch
            {
                transform.GetChild(0).transform.position = Vector2.MoveTowards(transform.GetChild(0).transform.position, transform.position, moveSpeed * Time.deltaTime);
            }
        }
         
    }

    public void GetGemObj(Vector3 pos)
    {
        for (int i = 0; i < gbm.gemPool.Count; i++)
        {
            GameObject obj = gbm.gemPool[i];
            if (!obj.activeSelf)
            {
                //go through gempool list and look for next disabled obj
                obj.transform.position = pos;
                obj.transform.parent = transform;
                obj.SetActive(true);
                break;
            }
        }
    }

    private void OnMouseDown()
    {
        mouseDownPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        mouseUpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (gsm.GetIsInteractable()) // only do this if isInteractable is true
        {
            sm.ForceResetCombo();
            CalculateSwipeAngle();
        }
    }

    private void CalculateSwipeAngle()
    {
        float swipe = Mathf.Atan2(mouseUpPos.y - mouseDownPos.y, mouseUpPos.x - mouseDownPos.x) * 180 / Mathf.PI; //*180 / pi converts Atanr results to degrees
        float distance = Mathf.Sqrt(Mathf.Pow(mouseUpPos.y - mouseDownPos.y, 2) + Mathf.Pow(mouseUpPos.x - mouseDownPos.x, 2)); // get length of swipe

       
        if (distance > 0.4f)
        {
            if (swipe <= 23 && swipe >= -22)
                GetOtherGem(1, 0); // east
            else if (swipe <= 68 && swipe >= 24)
                GetOtherGem(1, 1);// north east
            else if (swipe <= 113 && swipe >= 69)
                GetOtherGem(0, 1); // north
            else if (swipe <= 158 && swipe >= 114)
                GetOtherGem(-1, 1); //north west
            else if (swipe >= 158 || swipe <= -157)
                GetOtherGem(-1, 0); //west
            else if (swipe <= -23 && swipe >= -67)
                GetOtherGem(1, -1); // south east
            else if (swipe <= -68 && swipe >= -112)
                GetOtherGem(0, -1); // south
            else if (swipe <= -113 && swipe >= -156)
                GetOtherGem(-1, -1); //south west
        }
       
    }

    private void GetOtherGem (int x, int y)
    {
        int getX = (int)gridPos.x + x;
        int getY = (int)gridPos.y + y;

        if (getX >= 0 && getX < gbm.gridBoard.GetLength(0) && getY >= 0 && getY < gbm.gridBoard.GetLength(1)) //make sure is withing array range
        {
            previousParent = gbm.gridBoard[getX, getY].gameObject; //stores the previous parent of the current child
            SwapChild(transform, previousParent.transform);


            // if there is no matches swap back
            if (!gbm.CheckForMatches())
                StartCoroutine(SwapChildBack());
        }
        
    }

    private void SwapChild(Transform parentA, Transform parentB)
    {
        //get the children objects of the current and to be swap obj
        GameObject myChild = parentA.GetChild(0).gameObject;
        GameObject newChild = parentB.transform.GetChild(0).gameObject;

        //swap parents which will cause childInPos to be false
        myChild.transform.parent = parentB;
        newChild.transform.parent = parentA;
    }

    private IEnumerator SwapChildBack()
    {
        gsm.SetIsInteractable(false);
        yield return new WaitForSeconds(0.3f);
        SwapChild(transform, previousParent.transform);
        gsm.SetIsInteractable(true);
    }

    public bool CheckIfGemObjIsActive()
    {
        if (transform.childCount <= 0) //if there is no child
        {
            GameObject myChild;

            for (int y = (int)gridPos.y; y < gbm.gridBoard.GetLength(1); y++) //start checking every grid box above it
            {
                if (gbm.gridBoard[(int)gridPos.x, y].transform.childCount > 0) //if there is a child, grab the child and exit the loop
                {
                    myChild = gbm.gridBoard[(int)gridPos.x, y].transform.GetChild(0).gameObject;
                    myChild.transform.parent = transform;
                    return true;
                }
                //return false if it couldnt find a child
                else if (y == gbm.gridBoard.GetLength(1) - 1 && transform.childCount <= 0)
                    return false;
            }
        }

        return true;
    }
}
