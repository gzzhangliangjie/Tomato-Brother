using ScriptObj;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace System
{
    public class Menu : MonoBehaviour
    {
        public Bag bag;
        public GameObject settingUI;
        public GameStatus gameStatus;
        public PlayerStatus playerStatus;

        private void Awake()
        {
            // vSyncCount 非 0 时 targetFrameRate 不生效，需一并关闭
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            settingUI.SetActive(false);
        }

        public void StartGame()
        {
            bag.Init();
            gameStatus.Init();
            playerStatus.Init();
            Time.timeScale = 1;
            SceneLoader.Instance.Load("Select");
        }

        public void OpenSetting()
        {
            settingUI.SetActive(true);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}