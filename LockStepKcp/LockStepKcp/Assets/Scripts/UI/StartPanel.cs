
using LockstepTutorial;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : MonoBehaviour
{
    public Button startGameBtn;


    void Start()
    {
        startGameBtn.onClick.AddListener(OnStartGameBtnClick);
    }

    private void OnDestroy()
    {
        startGameBtn.onClick.RemoveListener(OnStartGameBtnClick);
    }



    public void OnStartGameBtnClick()
    {
        StartCoroutine(StartMatch());
    }

    public IEnumerator StartMatch()
    {
        startGameBtn.GetComponentInChildren<Text>().text = "∆•≈‰÷–...";

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1f));

        GameManager.Instance.StartConnect();
    }


    public void OnMatchSuccess()
    {
        startGameBtn.GetComponentInChildren<Text>().text = "∆•≈‰≥…π¶";
    }

}
