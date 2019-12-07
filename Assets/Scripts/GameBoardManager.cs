using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoardManager : MonoBehaviour
{

    [SerializeField] GameObject gridObj;
    [SerializeField] GameObject[] gems;

    public List<GameObject> gemPool = new List<GameObject>();

    [Header("Grid specification")]
    [SerializeField] float offSet;
    [SerializeField] int gridWidth;
    [SerializeField] int gridHeight;
    [SerializeField] Vector2 startPos;

    [Header("Removing and Refreshing properties")]
    [SerializeField] float nextFadeRate = 0.3f;

    public Grid[,] gridBoard;

    public List<List<GameObject>> matchLists = new List<List<GameObject>>();

    private GameStateManager gsm;
    private ScoreManager sm;
    private bool hasStarted = false;

    void Start()
    {
        gsm = FindObjectOfType<GameStateManager>();
        sm = FindObjectOfType<ScoreManager>();
        gridBoard = new Grid[gridWidth, gridHeight];
        InitializeGemPool();
    }

    private void InitializeGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 spawnPos = (new Vector2(x, y) * offSet) + startPos;
                GameObject grid = Instantiate(gridObj, spawnPos, Quaternion.identity) as GameObject;
                grid.name = x + ", " + y;
                grid.GetComponent<Grid>().gridPos = new Vector2(x, y);
                gridBoard[x, y] = grid.GetComponent<Grid>();
            }
        }
        
        if (CheckForMatches())
            Repopulate();
        else
            hasStarted = true; 
    }

    private void Repopulate()
    {
        SuffleGemPool();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject child = gridBoard[x, y].transform.GetChild(0).gameObject;
                child.transform.SetParent(null);
                child.SetActive(false);
                gridBoard[x, y].GetComponent<Grid>().GetGemObj(gridBoard[x, y].transform.position);
            }
        }
        
        if (CheckForMatches())
            Repopulate();
        else
            hasStarted = true; 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Repopulate();
    }

    private void InitializeGemPool()
    {
        int gemPerColour = (gridHeight * gridWidth) / 3; //can be adjust to suit the game's design

        for (int i = 0; i < gems.Length; i++) // for each gem type inthe array
        {
            for (int j = 0; j < gemPerColour; j++) //set to spawn the amount set per gem type
            {
                GameObject gem = Instantiate(gems[i], new Vector3(-1, -1, 0), Quaternion.identity) as GameObject;
                gem.SetActive(false);
                gemPool.Add(gem);
            }
        }
        SuffleGemPool();
        InitializeGrid();
    }

    private void SuffleGemPool()
    {
        int n = 0;
        while (n < gemPool.Count)
        {
            int r = Random.Range(0, gemPool.Count); //picks a random obj from the gemPool list
            if (gemPool[r].activeSelf) // if the gem is already active, do not suffle
                continue;
            GameObject tempObj = gemPool[r]; //stores in temporary obj
            gemPool[r] = gemPool[n]; //swap the ramdomly picked obj with the nth count obj
            gemPool[n] = tempObj; //swap nth obj with temp
            n++;
        }
    }

    public bool CheckForMatches()
    {
        matchLists.Clear(); // clear the list before checking

        DoCheckAxis("Horizontal");
        DoCheckAxis("Vertical");
           
        if (matchLists.Count > 0) //if matchlist has a list of matches
        {
            if (!hasStarted)
                return true;

            gsm.SetIsInteractable(false);
            StartCoroutine(RefreshGameBoard());
            return true;
        }
        else
        {
            gsm.SetIsInteractable(true);
            return false;
        }
    }

    private void DoCheckAxis(string axis)
    {
        //use "Horizontal" or "Vertical"

        int firstAxis = 0, secondAxis = 0;
        //set up the x and y axis based on the direction inputed
        if (axis == "Horizontal") //a = y and b = x
        {
            firstAxis = gridHeight;
            secondAxis = gridWidth;
        }
        else if (axis == "Vertical")//a = x and b = y
        {
            firstAxis = gridWidth;
            secondAxis = gridHeight;
        }
        else
        {
            Debug.LogError("No Axis inputed");
        }

        int numOfMatches = 0; //count the number of matching gems in a row
        List<GameObject> currentList = new List<GameObject>();

        for (int a = 0; a < firstAxis; a++)
        {
            //clears the list and num of matches for new row
            currentList = new List<GameObject>(); //***** use new List<> ove List.clear() as List.clear() will clear the list from all entiry
            numOfMatches = 0;

            for (int b = 0; b < secondAxis; b++)
            {
                GameObject currentGem;

                if (axis == "Horizontal")
                    currentGem = gridBoard[b, a].transform.GetChild(0).gameObject;
                else if (axis == "Vertical")
                    currentGem = gridBoard[a, b].transform.GetChild(0).gameObject;
                else
                {
                    Debug.LogError("CurrentGem is assign due to no Axis input");
                    break;
                }

                if (currentList.Count == 0) // if there are no gems in current list, add the currently check gem to list
                {
                    currentList.Add(currentGem);
                    numOfMatches++;
                }
                else if (currentList.Count > 0) // if there is a gem.....
                {
                    if (currentGem.tag == currentList[0].tag) //match the tags, if matched, add to list
                    {
                        currentList.Add(currentGem);
                        numOfMatches++;
                    }
                    else if (currentGem.tag != currentList[0].tag) //other wise....
                    {
                        if (numOfMatches >= 3) //check if the list is 3 or more gems long, if so add the list 
                        {
                            DoAddToMatchList(currentList);
                        }
                        //clear the list and start a new 
                        currentList = new List<GameObject>();
                        currentList.Add(currentGem);
                        numOfMatches = 1;
                    }                  
                }

                if (b == secondAxis-1) // if last gem on row/column
                {
                    if (numOfMatches >= 3)
                    {
                        DoAddToMatchList(currentList);
                    }
                }
            }

        }
    }
    private void DoAddToMatchList(List<GameObject> objList)
    {

        List<GameObject> tempList = objList;
        if (DoCheckForConnectedList(objList) != null) // if it returns true
        {
            int index = matchLists.IndexOf(DoCheckForConnectedList(objList)); //get the index of the list that has a match
            tempList = matchLists[index].Union(objList).ToList(); //merge the two list together using List.Union, which doesnt add duplicates
            matchLists.RemoveAt(index); //remove the old list
        }

        matchLists.Add(tempList);
    }

    private List<GameObject> DoCheckForConnectedList(List<GameObject> objs)
    {
        foreach (List<GameObject> currentList in matchLists) //go through each list
        {
            if (currentList[0].tag == objs[0].tag) //check if that tags match up
            {
                foreach (GameObject listedObj in currentList) //for each obj in the pre existing list
                {
                    for (int i = 0; i <objs.Count;i++)//check if it matches the inputed list's obj
                    {
                        if (ReferenceEquals(listedObj,objs[i]))//GameObject.ReferenceEquals checks if the two obj are the same
                        {
                            return currentList;
                        }
                    }
                }
            }
            
        }
        return null;
    }

    private IEnumerator DisableMatchObj()
    {
        yield return new WaitForSeconds(0.25f); // small delay for animation tofinish

        foreach (List<GameObject> lists in matchLists)
        {
          foreach (GameObject obj in lists)
            {
                obj.GetComponent<Gem>().DisableObj();
            }
            sm.UpdateScore(lists);
            yield return new WaitForSeconds(nextFadeRate);
        }

    }

    private IEnumerator RefreshGameBoard()
    {
        yield return StartCoroutine(DisableMatchObj());
        yield return new WaitForSeconds(0.2f);// buffer for all fade animation to end before moving on
        SuffleGemPool(); // suffle the pool before grabing new gems


        for (int x = 0; x < gridWidth; x++)
        {
            int numberOfMissingGemInColumn = 0;//determine how high up it needs to be depending on the number of missing gemin the colum

            for (int y = 0; y < gridHeight; y++)
            {
               if (!gridBoard[x, y].GetComponent<Grid>().CheckIfGemObjIsActive())
                {
                    float yPos = (gridHeight - y) * offSet + gridBoard[x,y].transform.position.y + (numberOfMissingGemInColumn * offSet);
                    Vector3 spawnPos = new Vector3(gridBoard[x, y].transform.position.x, yPos);
                    gridBoard[x, y].GetComponent<Grid>().GetGemObj(spawnPos);
                    numberOfMissingGemInColumn++;
                }
            }
        }

        //wait until all animations have finish
        while (!gsm.CheckIfAnimationHasEnded())
        {
            yield return null;
        }

        CheckForMatches();
    }
}
