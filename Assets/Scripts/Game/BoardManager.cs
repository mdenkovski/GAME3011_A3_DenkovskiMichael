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


        for (int x = 0; x < xSize; x++)
        {      
            for (int y = 0; y < ySize; y++)
            {
                GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
                tiles[x, y] = newTile;
                newTile.transform.parent = transform;


                List<Sprite> possibleCharacters = new List<Sprite>();
                possibleCharacters.AddRange(icons);

                possibleCharacters.Remove(previousLeft[y]); 
                possibleCharacters.Remove(previousBelow);

                int choice = Random.Range(0, possibleCharacters.Count);
                Sprite newSprite = possibleCharacters[choice];
                newTile.GetComponentInChildren<SpriteRenderer>().sprite = newSprite;
                previousLeft[y] = newSprite;
                previousBelow = newSprite;

                newTile.GetComponentInChildren<TileBehaviour>().tileType = (TileType)icons.IndexOf(newSprite);
                ;
            }
        }
    }

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
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (tiles[x, y].GetComponentInChildren<SpriteRenderer>().sprite == null)
                {
                    numColsToShift++;

                    yield return new WaitForSeconds(moveTileDelay);
                    ShiftColumnDown(x, y);
                    
                    break;
                }
            }
        }
        while (numColsToShift != 0)
            yield return new WaitForSeconds(0.1f);

        yield return new WaitForEndOfFrame();

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                tiles[x, y].GetComponentInChildren<TileBehaviour>().ClearAllMatches();
            }
        }

    }

    private void ShiftColumnDown(int x, int yStart)
    {
        int yPos = yStart;
        IsShifting = true;
        List<SpriteRenderer> renders = new List<SpriteRenderer>();
        bool foundNull = true;
        while (foundNull)
        {
            foundNull = false;
            //populate the renders
            for (int y = yPos; y < ySize; y++)
            {
                SpriteRenderer render = tiles[x, y].GetComponentInChildren<SpriteRenderer>();
                renders.Add(render);

            }

            if (renders[0].sprite == null)
            {
                foundNull = true;
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
                renders.Clear();
            }
        }
        IsShifting = false;
        numColsToShift--;
    }
    private void ShiftTilesDown1(int x, List<SpriteRenderer> renders)
    {
        for (int k = 0; k < renders.Count - 1; k++)
        {
            renders[k].sprite = renders[k + 1].sprite;
            renders[k].gameObject.GetComponent<TileBehaviour>().tileType = (TileType)icons.IndexOf(renders[k].sprite);
            if (k == renders.Count - 2)//last one in loop
            {
                renders[k + 1].sprite = GetNewSprite(x, ySize - 1);
                renders[k + 1].gameObject.GetComponent<TileBehaviour>().tileType = (TileType)icons.IndexOf(renders[k + 1].sprite);

            }
        }
        if (renders.Count == 1)
        {
            renders[0].sprite = GetNewSprite(x, ySize - 1);
            renders[0].gameObject.GetComponent<TileBehaviour>().tileType = (TileType)icons.IndexOf(renders[0].sprite);
        }
    }

    private Sprite GetNewSprite(int x, int y)
    {
        List<Sprite> possibleCharacters = new List<Sprite>();
        possibleCharacters.AddRange(icons);

        //if (x > 0)
        //{
        //    possibleCharacters.Remove(tiles[x - 1, y].GetComponentInChildren<SpriteRenderer>().sprite);
        //}
        //if (x < xSize - 1)
        //{
        //    possibleCharacters.Remove(tiles[x + 1, y].GetComponentInChildren<SpriteRenderer>().sprite);
        //}
        //if (y > 0)
        //{
        //    possibleCharacters.Remove(tiles[x, y - 1].GetComponentInChildren<SpriteRenderer>().sprite);
        //}

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
    public void EasyDifficultyChosen()
    {
        if (tiles!= null)
        {
            ClearBoard();
        }
        icons.Clear();
        icons.AddRange(Mastericons);
        icons.RemoveRange(icons.Count - 3, 3);

        Vector2 offset = tile.GetComponentInChildren<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);
        GameUI.ResetGameUI();
        GameUI.SetTargetScore(20000);

        EasyButton.image.color = Color.green;
        MediumButton.image.color = Color.white;
        HardButton.image.color = Color.white;

    }
    public void MediumDifficultyChosen()
    {
        if (tiles != null)
        {
            ClearBoard();
        }
        icons.Clear();
        icons.AddRange(Mastericons);
        icons.RemoveRange(icons.Count - 1, 1);

        Vector2 offset = tile.GetComponentInChildren<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);

        GameUI.ResetGameUI();
        GameUI.SetTargetScore(25000);

        EasyButton.image.color = Color.white;
        MediumButton.image.color = Color.green;
        HardButton.image.color = Color.white;
    }
    public void HardDifficultyChosen()
    {
        if (tiles != null)
        {
            ClearBoard();
        }
        icons.Clear();
        icons.AddRange(Mastericons);

        Vector2 offset = tile.GetComponentInChildren<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);

        GameUI.ResetGameUI();
        GameUI.SetTargetScore(30000);

        EasyButton.image.color = Color.white;
        MediumButton.image.color = Color.white;
        HardButton.image.color = Color.green;
    }

}
