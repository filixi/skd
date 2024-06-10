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

    public Dictionary<string, int> success_count = new Dictionary<string, int>();
    public void Success()
    {
        if (!success_count.ContainsKey(level_name))
            success_count.Add(level_name, 0);
        ++success_count[level_name];
    }
    public bool Succeed()
    {
        return success_count.Count > 0;
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

    public string level_name
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
