using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class LevelManager : MonoBehaviour
{
    public static LevelManager manage;
    [SerializeField]
    private int currentLevelId;
    [SerializeField]
    private TMP_Text finalText;
    [SerializeField]
    private Image background;
    // Start is called before the first frame update
    void Start()
    {
        finalText.gameObject.SetActive(false);
        manage = this;
        Color col = finalText.color;
        col.a = 0;
        finalText.color = col;

        Color colorBack = background.color;
        colorBack.a = 0;
        background.color = colorBack;
    }

    public void gameOver()
    {
        StartCoroutine(OverAnim(2, currentLevelId, "Game over",Color.red));
    }

    public void Win()
    {

        StartCoroutine(OverAnim(2,currentLevelId,"You won!!!", Color.green));
        Score.scoreToWin *= 1.2f;
        SceneManager.LoadScene(currentLevelId);
    }

    IEnumerator OverAnim(float time, int sceneId, string text, Color color)
    {
        color.a = 0;

        finalText.gameObject.SetActive(true);
        finalText.text = text;
        finalText.color = color;
        float coef = 0;
        do
        {
            coef += Time.fixedDeltaTime / time;
            Color col = finalText.color;
            col.a = Mathf.Lerp(0,1,easings.easeOutQuad(coef));
            finalText.color = col;

            Color colorBack = background.color;
            colorBack.a = col.a;
            background.color = colorBack;
            yield return new WaitForFixedUpdate();

        } while (coef < 1);
        SceneManager.LoadScene(sceneId);
    }
}
