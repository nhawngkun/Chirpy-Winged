using UnityEngine;

public class uihowtoplay_PolarRescue : UICanvas_ChirpyWinged
{
    public void back()
    {
        // Đóng How To Play
        UIManager_ChirpyWinged.Instance.EnableHowToPlay(false);
        
        // Mở Home (Setup sẽ tự động bật lại buttons)
        UIManager_ChirpyWinged.Instance.EnableHome(true);
    }

    void Start()
    {

    }

    void Update()
    {

    }
}