using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TileType
{
    Blue,
    Green,
    Red,
    Yellow,
    Violet,
    Grey

}
public class TileBehaviour : MonoBehaviour
{
    private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
    private static TileBehaviour previousSelected = null;

    private SpriteRenderer SpriteImage;
    private bool isSelected = false;

    private Vector3[] adjacentDirections = new Vector3[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
    private float raycastSize;

    private bool matchFound = false;


    public static GameUIScript GameUI;
    public static bool IsInMenu;

    public TileType tileType;

    [SerializeField]
    Animator Animator;
    

    void Awake()
    {
        SpriteImage = GetComponentInChildren<SpriteRenderer>();
        raycastSize = SpriteImage.bounds.size.x;
        Animator.speed = Random.Range(0.8f, 1.2f);
        Animator.SetTrigger("Spawn");
    }

    public void SpawnInAnimation()
    {
        Animator.SetTrigger("Spawn");
    }

    public void MoveDownAnimation()
    {
        Animator.SetTrigger("MoveDown");
    }
    private void Select()
    {
        isSelected = true;
        SpriteImage.color = selectedColor;
        previousSelected = gameObject.GetComponent<TileBehaviour>();
    }

    private void Deselect()
    {
        isSelected = false;
        SpriteImage.color = Color.white;
        previousSelected = null;
    }
    void OnMouseDown()
    {
        if (SpriteImage.sprite == null || BoardManager.Instance.IsShifting 
            || IsInMenu || tileType == TileType.Grey)
        {
            return;
        }

        if (isSelected)
        { 
            Deselect();
        }
        else
        {
            if (previousSelected == null)
            { 
                Select();
            }
            else
            {
                if (GetAllAdjacentTiles().Contains(previousSelected.gameObject))
                {
                    SwapSprite(previousSelected.SpriteImage);
                    previousSelected.ClearAllMatches();
                    previousSelected.Deselect();
                    ClearAllMatches();
                    GameUI.UseAMove();
                }
                else
                { 
                    previousSelected.GetComponent<TileBehaviour>().Deselect();
                    Select();
                }
            }

        }
    }

    public void SwapSprite(SpriteRenderer render2)
    { 
        if (SpriteImage.sprite == render2.sprite)
        { 
            return;
        }

        Sprite tempSprite = render2.sprite; 
        render2.sprite = SpriteImage.sprite; 
        SpriteImage.sprite = tempSprite; 
    }

    private GameObject GetAdjacent(Vector3 castDir)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, castDir);
        if (Physics.Raycast(ray, out hit, raycastSize)) 
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    private List<GameObject> GetAllAdjacentTiles()
    {
        List<GameObject> adjacentTiles = new List<GameObject>();
        for (int i = 0; i < adjacentDirections.Length; i++)
        {
            adjacentTiles.Add(GetAdjacent(adjacentDirections[i]));
        }
        return adjacentTiles;
    }

    private List<GameObject> FindMatch(Vector3 castDir)
    { 
        List<GameObject> matchingTiles = new List<GameObject>(); 
        RaycastHit hit;
        Ray ray = new Ray(transform.position, castDir);
        Physics.Raycast(ray, out hit, raycastSize);
        while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == SpriteImage.sprite)
        { 
            matchingTiles.Add(hit.collider.gameObject);
            Ray newRay = new Ray(hit.collider.transform.position, castDir);
            Physics.Raycast(newRay, out hit, raycastSize);
        }
        return matchingTiles; 
    }

    private void ClearMatch(Vector3[] paths) 
    {
        List<GameObject> matchingTiles = new List<GameObject>(); 
        for (int i = 0; i < paths.Length; i++) 
        {
            matchingTiles.AddRange(FindMatch(paths[i]));
        }
        if (matchingTiles.Count >= 2) 
        {
            for (int i = 0; i < matchingTiles.Count; i++)
            {
                matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
            }
            matchFound = true;

            GameUI.IncreaseScore(50 * matchingTiles.Count);
        }
    }

  

    public void ClearAllMatches()
    {
        if (SpriteImage.sprite == null)
            return;

        ClearMatch(new Vector3[2] { Vector3.left, Vector3.right });
        ClearMatch(new Vector3[2] { Vector3.up, Vector3.down });
        if (matchFound)
        {
            SpriteImage.sprite = null;
            matchFound = false;


            BoardManager.Instance.FindEmptyTiles();

            GameUI.PlayMatchSound();
        }
    }


}
