using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public List<Sprite> Mastericons = new List<Sprite>();
    private List<Sprite> icons = new List<Sprite>();
    public GameObject tile;     
    public int xSize, ySize;     

    private GameObject[,] tiles;   
    public bool IsShifting { get; set; }

    [SerializeField]
    private float moveTileDelay = 0.5f;

    private int numColsToShift = 0;

    IEnumerator FindNullTilesCoroutine;

    GameUIScript GameUI;

    [SerializeField]
    private Button EasyButton;

    [SerializeField]
    private Button MediumButton;

    [SerializeField]
    private Button HardButton;
    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(this.gameObject);
            }
        }


        GameUI = FindObjectOfType<GameUIScript>();
        TileBehaviour.GameUI = GameUI;


        EasyDifficultyChosen();
    }

    private void OnDisable()
    {
        ClearBoard();
    }
    private void CreateBoard(float xOffset, float yOffset)
    {
        tiles = new GameObject[xSize, ySize];   

        float startX = transform.position.x;   
        float startY = transform.position.y;

        Sprite[] previousLeft = new Sprite[ySize];
        Sprite previousBelow = null;

        //populate our tiles
        for (int x = 0; x < xSize; x++)
        {      
            for (int y = 0; y < ySize; y++)
            {
                GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
                tiles[x, y] = newTile;
                newTile.transform.parent = transform;


                List<Sprite> possibleCharacters = new List<Sprite>();
                possibleCharacters.AddRange(icons);
                //prevent any matches from forming
                possibleCharacters.Remove(previousLeft[y]); 
                possibleCharacters.Remove(previousBelow);
                //apply the new choice
                int choice = Random.Range(0, possibleCharacters.Count);
                Sprite newSprite = possibleCharacters[choice];
                newTile.GetComponentInChildren<SpriteRenderer>().sprite = newSprite;
                previousLeft[y] = newSprite;
                previousBelow = newSprite;
                //set the tile type
                newTile.GetComponentInChildren<TileBehaviour>().tileType = (TileType)icons.IndexOf(newSprite);
                ;
            }
        }
    }

    //manages the coroutine for finding empty tiles to prevent it from double counting
    public void FindEmptyTiles()
    {
        if (FindNullTilesCoroutine != null)
        {
            StopCoroutine(FindNullTilesCoroutine);
        }
        FindNullTilesCoroutine = FindNullTiles();
        StartCoroutine(FindNullTilesCoroutine);
    }
    private IEnumerator FindNullTiles()
    {
        numColsToShift = 0;
        //check if there is a match
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (tiles[x, y].GetComponentInChildren<SpriteRenderer>().sprite == null) //found a match
                {
                    numColsToShift++;
                    //begin shifting
                    IsShifting = true;
                    yield return new WaitForSeconds(moveTileDelay);
                    ShiftColumnDown(x, y); //shift all our tiles down
                    IsShifting = false;
                    break;
                }
            }
        }
        while (numColsToShift != 0)
            yield return new WaitForSeconds(0.1f);

        yield return new WaitForEndOfFrame();
        //check for any new matches formed
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                tiles[x, y].GetComponentInChildren<TileBehaviour>().ClearAllMatches();
            }
        }

    }

    //shifts the colums down until there are no more empty tiles
    private void ShiftColumnDown(int x, int yStart)
    {
        int yPos = yStart;
        List<SpriteRenderer> renders = new List<SpriteRenderer>();
        bool foundNull = true;
        while (foundNull)
        {
            //assume we didnt find a empty
            foundNull = false;
            //populate the renders
            for (int y = yPos; y < ySize; y++)
            {
                SpriteRenderer render = tiles[x, y].GetComponentInChildren<SpriteRenderer>();
                renders.Add(render);

            }
            //check if the base is an empty tile
            if (renders[0].sprite == null)
            {
                foundNull = true;
                //shift all tiles down 1
                ShiftTilesDown1(x, renders);
                for (int y = 0; y < renders.Count; y++)
                {
                    if (renders[y].sprite == null)
                    {
                        //add the y difference to get our new y pos
                        yPos += y;
                        break;
                    }
                }
                //so we can repopulate if the next empty if father above and not adjacent
                renders.Clear();
            }
        }
        numColsToShift--;
    }
    private void ShiftTilesDown1(int x, List<SpriteRenderer> renders)
    {
        //mode through the different sprites and shift them down one
        for (int k = 0; k < renders.Count - 1; k++)
        {
            renders[k].sprite = renders[k + 1].sprite;
            //set tile type
            renders[k].gameObject.GetComponent<TileBehaviour>().tileType = (TileType)icons.IndexOf(renders[k].sprite);
            if (k == renders.Count - 2)//last one in loop
            {
                renders[k + 1].sprite = GetNewSprite(x, ySize - 1);
                //set tile type
                renders[k + 1].gameObject.GetComponent<TileBehaviour>().tileType = (TileType)icons.IndexOf(renders[k + 1].sprite);

            }
        }
        //if it a top empty
        if (renders.Count == 1)
        {
            renders[0].sprite = GetNewSprite(x, ySize - 1);
            //set tile type
            renders[0].gameObject.GetComponent<TileBehaviour>().tileType = (TileType)icons.IndexOf(renders[0].sprite);
        }
    }

    //provide a new sprite to use 
    private Sprite GetNewSprite(int x, int y)
    {
        List<Sprite> possibleCharacters = new List<Sprite>();
        possibleCharacters.AddRange(icons);


        return possibleCharacters[Random.Range(0, possibleCharacters.Count)];
    }

    private void ClearBoard()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Destroy(tiles[x, y]);
            }
        }
    }
    //set values for easy difficulty
    public void EasyDifficultyChosen()
    {
        if (tiles!= null)
        {
            ClearBoard();
        }
        icons.Clear();
        icons.AddRange(Mastericons);
        icons.RemoveRange(icons.Count - 3, 3); //set the icons that are available
        //recreate the board
        Vector2 offset = tile.GetComponentInChildren<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);
        GameUI.ResetGameUI();
        GameUI.SetTargetScore(20000);
        GameUI.SetMaxNumMoves(20);

        EasyButton.image.color = Color.green;
        MediumButton.image.color = Color.white;
        HardButton.image.color = Color.white;

    }
    //set values for medium difficulty
    public void MediumDifficultyChosen()
    {
        if (tiles != null)
        {
            ClearBoard();
        }
        icons.Clear();
        icons.AddRange(Mastericons);
        icons.RemoveRange(icons.Count - 1, 1);//set the icons that are available
        //recreate the board
        Vector2 offset = tile.GetComponentInChildren<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);

        GameUI.ResetGameUI();
        GameUI.SetTargetScore(7000);
        GameUI.SetMaxNumMoves(25);

        EasyButton.image.color = Color.white;
        MediumButton.image.color = Color.green;
        HardButton.image.color = Color.white;
    }
    //set values for hard difficulty
    public void HardDifficultyChosen()
    {
        if (tiles != null)
        {
            ClearBoard();
        }
        icons.Clear();
        icons.AddRange(Mastericons);//set the icons that are available
        //recreate the board
        Vector2 offset = tile.GetComponentInChildren<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);

        GameUI.ResetGameUI();
        GameUI.SetTargetScore(4500);
        GameUI.SetMaxNumMoves(30);

        EasyButton.image.color = Color.white;
        MediumButton.image.color = Color.white;
        HardButton.image.color = Color.green;
    }

}
