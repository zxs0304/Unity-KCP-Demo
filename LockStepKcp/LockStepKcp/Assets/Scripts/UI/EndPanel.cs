using LockstepTutorial;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndPanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Text winText;

    public Button homeBtn;

    private void Start()
    {
        homeBtn.onClick.AddListener(OnHomeBtnClick);
    }
    private void OnDestroy()
    {
        homeBtn.onClick.RemoveListener(OnHomeBtnClick);
    }
    public void SetWinText(bool win)
    {

        transform.SetAsLastSibling();
        if (win)
        {
            winText.text = "You Win !";
        }
        else
        {
            winText.text = "You Lose !";
        }
        StartCoroutine(ShowWinText());
    }

    public IEnumerator ShowWinText()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

    }

    public void OnHomeBtnClick()
    {
        GameManager.Instance.isExit = true;
        GameManager.Instance.ExitGame();
    }
}
