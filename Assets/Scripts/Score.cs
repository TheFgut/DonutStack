using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Score : MonoBehaviour
{
    public static Score instance;

    public TMP_Text scoreText;
    public TMP_Text combo;
    public Slider progressBar;

    public float comboTime;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //text = GetComponent<TextMeshPro>();
    }

    int score;
    Coroutine comboRoutine;
    float comboTimeLeft;

    int comboCoef;
    Color comboColor;

    internal static float scoreToWin = 10000f;
    public void IncreaseScore(int scoreAdd)
    {
        comboTimeLeft = comboTime;
        score += (int)(scoreAdd * (1 + comboCoef/30f));
        scoreText.text = score.ToString();
        comboCoef++;

        float value = score / scoreToWin;
        progressBar.value = value;
        if (value >= 1)
        {
            LevelManager.manage.Win();
        }

        combo.text = "x" + comboCoef.ToString();
        comboColor = Color.Lerp(Color.green, Color.red, comboCoef / 30f);
        combo.color = comboColor;
        combo.enabled = true;
        if (comboRoutine == null)
        {
            comboRoutine = StartCoroutine(comboTimer());
        }

    }

    IEnumerator comboTimer()
    {
        do
        {

            float sizeCoef = Mathf.Lerp(180,250,comboTimeLeft/ comboTime);
            combo.fontSize = sizeCoef;
            comboTimeLeft -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        } while (comboTimeLeft > 0);
        combo.enabled = false;
        comboCoef = 0;
        comboRoutine = null;
    }

}
