
using LockstepTutorial;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : MonoBehaviour
{
    public Button startGameBtn;
    public Button replayBtn;
    public Toggle character_0;
    public Toggle character_1;

    void Start()
    {
        startGameBtn.onClick.AddListener(OnStartGameBtnClick);
        character_0.onValueChanged.AddListener(OnToggle1Click);
        character_1.onValueChanged.AddListener(OnToggle2Click);
        replayBtn.onClick.AddListener(OnReplayBtnClick);
    }

    private void OnDestroy()
    {
        startGameBtn.onClick.RemoveListener(OnStartGameBtnClick);
        character_0.onValueChanged.RemoveListener(OnToggle1Click);
        character_1.onValueChanged.RemoveListener(OnToggle2Click);
        replayBtn.onClick.RemoveListener(OnReplayBtnClick);
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

    public void OnToggle1Click(bool value)
    {
        GameManager.Instance.characterNumber = 0;
    }
    public void OnToggle2Click(bool value)
    {
        GameManager.Instance.characterNumber = 1;
    }


    public void OnReplayBtnClick()
    {
        GameManager.Instance.IsReplay = true;
        GameManager.Instance.StartConnect();
    }

    public void OnMatchSuccess()
    {
        startGameBtn.GetComponentInChildren<Text>().text = "∆•≈‰≥…π¶";
    }

}
