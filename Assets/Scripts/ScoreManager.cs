using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] int pointsPerGem = 10;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI comboText;

    private int currentScore;
    private int finalScore;
    private Coroutine resetCombo;

    [SerializeField] float bonusMultiplier;
    private int currentCombo;

    private void Start()
    {
        scoreText.text = currentScore.ToString();
        comboText.text = "";
    }
    public void UpdateScore(List<GameObject> matchLists)
    {
        if (resetCombo != null)
            StopCoroutine(resetCombo);
        currentCombo++;
        currentScore += CalculateScore(matchLists.Count, currentCombo);
        scoreText.text = currentScore.ToString();
        resetCombo = StartCoroutine(ResetComboCounter());
    }
    
    private IEnumerator ResetComboCounter()
    {
        yield return new WaitForSeconds(2.5f);
        comboText.text = "";
        currentCombo = 0;
    }

    public void ForceResetCombo()
    {
        comboText.text = "";
        currentCombo = 0;
    }

    private int CalculateScore(int numOfGem, int combo)
    {
        int pointsFromGems = numOfGem * pointsPerGem;
        int pointsFromCurrentList = pointsFromGems;

        if (combo >= 3)
        {
            float bonusPoints = (combo * bonusMultiplier) * pointsFromGems;
            pointsFromCurrentList = (int)bonusPoints;
            comboText.text = combo + "x COMBO";
            return pointsFromCurrentList;
        }

        return pointsFromCurrentList;

    }

}
