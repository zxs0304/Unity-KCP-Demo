
using LockstepTutorial;
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
        GameManager.Instance.StartConnect();
    }

}
