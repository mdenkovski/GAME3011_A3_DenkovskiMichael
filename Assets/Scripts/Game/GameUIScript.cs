using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUIScript : MonoBehaviour
{
    private int Score;
    
    private int NumMovesRemaining ;
    private int MaxNumMoves = 20;

    [SerializeField]
    private TMP_Text ScoreNumText;
    [SerializeField]
    private TMP_Text MovesNumText;
    [SerializeField]
    private TMP_Text TaregtScoreText;

    [SerializeField]
    private GameObject WinPanel;
    [SerializeField]
    private GameObject GameOverPanel;

    private AudioSource ClearedClip;

    [SerializeField]
    private int TargetScore;

    
    private void Awake()
    {
        ResetGameUI();
        ClearedClip = GetComponent<AudioSource>();
    }

    //reset use values
    public void ResetGameUI()
    {
        TileBehaviour.IsInMenu = false;

        WinPanel.SetActive(false);
        GameOverPanel.SetActive(false);

        Score = 0;
        NumMovesRemaining = MaxNumMoves;
        ScoreNumText.text = Score.ToString();
        MovesNumText.text = NumMovesRemaining.ToString();
    }
    //increase score and update ui
    public void IncreaseScore(int amount)
    {
        Score += amount;
        ScoreNumText.text = Score.ToString();
    }
    //use a move and update ui
    public void UseAMove()
    {
        NumMovesRemaining--;
        MovesNumText.text = NumMovesRemaining.ToString();

        if (NumMovesRemaining ==  0)
        {
            StartCoroutine(WaitForShifting());
        }
    }

    
    //make sure the game is finished checking all matches before proceeding
    private IEnumerator WaitForShifting()
    {
        yield return new WaitUntil(() => !BoardManager.Instance.IsShifting);
        yield return new WaitForSeconds(1.5f);

        if (Score >= TargetScore)
        {
            GameWin();
        }
        else
        {
            GameOver();
        }
    }

    //enable game over
    private void GameOver()
    {
        //game over stuff here
        GameOverPanel.SetActive(true);
        //prevent input in the tiles
        TileBehaviour.IsInMenu = true;
    }

    //enable game win
    private void GameWin()
    {
        //game win stuff here
        WinPanel.SetActive(true);
        //prevent input in the tiles
        TileBehaviour.IsInMenu = true;
    }
    public void PlayMatchSound()
    {
        ClearedClip.Play();
    }
    //set the target score amount and update ui
    public void SetTargetScore(int target)
    {
        TargetScore = target;
        TaregtScoreText.text = TargetScore.ToString();
    }
    //set the max moves amount and update ui
    public void SetMaxNumMoves(int num)
    {
        MaxNumMoves = num;
        NumMovesRemaining = MaxNumMoves;
        MovesNumText.text = NumMovesRemaining.ToString();
    }

}
