using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private TMP_Text _scoreText;
    private void Awake()
    {
        _scoreText = GetComponent<TMP_Text>();
    }
    
    public void UpdateScoreUI(int _score)
    {
        _scoreText.text = _score.ToString();
    }   
}
