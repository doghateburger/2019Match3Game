using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    private GameBoardManager gbm;
    private bool isInteractable = true;

    void Start()
    {
        gbm = FindObjectOfType<GameBoardManager>();
    }

    public void SetIsInteractable (bool state)
    {
        isInteractable = state;
    }

    public bool GetIsInteractable()
    {
        return isInteractable;
    }

    public bool CheckIfAnimationHasEnded()
    {
        int gridWidth = gbm.gridBoard.GetLength(0);
        int gridHeight = gbm.gridBoard.GetLength(1);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!gbm.gridBoard[x, y].GetComponent<Grid>().childInPos)
                    return false;
            }
        }
        return true;
    }

}
