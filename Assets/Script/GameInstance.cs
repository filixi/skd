using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance
{
    private static GameInstance instance = new GameInstance();
    public static GameInstance GetInstance()
    {
        return instance;
    }

    public bool hard_mode
    {
        get;
        set;
    } = false;

    public bool slow_mode
    {
        get;
        set;
    } = false;

    public string bgm_event
    {
        get;
        set;
    } = "";

    public string fx_event
    {
        get;
        set;
    } = "";

    public float GameDifficultyCoef()
    {
        if (hard_mode)
            return 2;
        return 1;
    }

    public float GlobalSpeedCoef()
    {
        if (slow_mode)
            return 0.3f;
        return 1;
    }
}
