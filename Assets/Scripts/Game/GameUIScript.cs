using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIScript : MonoBehaviour
{
    private int Score;
    
    private int NumMovesRemaining = 20;

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

    [SerializeField]
    private Button EasyButton;

    //[SerializeField]
    //private Button EasyButton;

    //[SerializeField]
    //private Button EasyButton;
    private void Awake()
    {

        ResetGameUI();
        ClearedClip = GetComponent<AudioSource>();
    }

    public void ResetGameUI()
    {
        TileBehaviour.IsInMenu = false;

        WinPanel.SetActive(false);
        GameOverPanel.SetActive(false);

        Score = 0;
        ScoreNumText.text = Score.ToString();
        MovesNumText.text = NumMovesRemaining.ToString();
    }
    public void IncreaseScore(int amount)
    {
        Score += amount;
        ScoreNumText.text = Score.ToString();
    }

    public void UseAMove()
    {
        NumMovesRemaining--;
        MovesNumText.text = NumMovesRemaining.ToString();

        if (NumMovesRemaining ==  0)
        {
            StartCoroutine(WaitForShifting());
        }
    }


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

    private void GameOver()
    {
        //game over stuff here
        GameOverPanel.SetActive(true);

        TileBehaviour.IsInMenu = true;
    }


    private void GameWin()
    {
        //game win stuff here
        WinPanel.SetActive(true);
        TileBehaviour.IsInMenu = true;
    }
    public void PlayMatchSound()
    {
        ClearedClip.Play();
    }

    public void SetTargetScore(int target)
    {
        TargetScore = target;
        TaregtScoreText.text = TargetScore.ToString();
    }

    
}
