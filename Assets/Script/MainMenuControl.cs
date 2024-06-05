using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel1()
    {
        var gi = GameInstance.GetInstance();
        gi.hard_mode = IsHardMode();
        gi.slow_mode = false;
        gi.bgm_event = "event:/BGM/BGM_Level1";
        gi.fx_event = "event:/FX/FX_Level1";
        
        SceneManager.LoadScene("Level1");
    }

    public void StartLevel2()
    {
        var gi = GameInstance.GetInstance();
        gi.hard_mode = IsHardMode();
        gi.slow_mode = false;
        gi.bgm_event = "event:/BGM/BGM_Level2";
        gi.fx_event = "event:/FX/FX_Level2";

        SceneManager.LoadScene("Level2");
    }

    public void StartLevel3()
    {
        var gi = GameInstance.GetInstance();
        gi.hard_mode = IsHardMode();
        gi.slow_mode = false;
        gi.bgm_event = "event:/BGM/BGM_Level3";
        gi.fx_event = "event:/FX/FX_Level3";

        SceneManager.LoadScene("Level3");
    }

    public void StartLevel4()
    {
        var gi = GameInstance.GetInstance();
        gi.hard_mode = IsHardMode();
        gi.slow_mode = true;
        gi.bgm_event = "event:/BGM/BGM_Level3";
        gi.fx_event = "event:/FX/FX_Level3";

        SceneManager.LoadScene("Level4");
    }

    public bool IsHardMode()
    {
        var go = GameObject.Find("HardMode");
        if (go == null)
            return false;
        var toggle = go.GetComponent<Toggle>();
        if (toggle == null)
            return false;
        return toggle.isOn;
    }
}
