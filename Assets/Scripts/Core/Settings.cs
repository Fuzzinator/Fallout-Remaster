using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public static Difficulty gameDifficulty = Difficulty.Normal;
    public static Difficulty combatDifficulty = Difficulty.Normal;
    public static ViolenceLvl violenceLvl = ViolenceLvl.Normal;
    public static TargetHighlight targetHighlight = TargetHighlight.On;

    public static bool combatMessages = true;
    public static bool combatTaunts = true;
    public static bool languageFilter = false;
    public static bool running = false;
    public static bool subtitles = false;
    public static bool itemHighlight = true;

    public static int combatSpeed = 1;//1-25, 1 = Normal, 25 = Fastest
    public static bool affectPlayerSpeed = true;
    public static int textDelay = 13;//1-25, 1 = Slow, 13 = Normal, 25 = Faster

    public static int masterAudioVolume = 16;//1-24, 0 = Off, 8 = Quiet, 16 = Normal, 24 = Loud
    public static int musicMovieVolume = 16;//Same
    public static int sfxVolume = 16;//Same
    public static int speechVolume = 16;//Same
    public static int brightnessLvl = 1;//1-25, 1 = Normal, 25 = Brighter
    public static int mouseSensitivity = 1;//1-25, 1 = Normal, 25 = Faster
    
    #region Enums
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    public enum ViolenceLvl
    {
        None,
        Minimal,
        Normal,
        MaximumBlood
    }

    public enum TargetHighlight
    {
        Off,
        On,
        TargetingOnly
    }
    #endregion
}
