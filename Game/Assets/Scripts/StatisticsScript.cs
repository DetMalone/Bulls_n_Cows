using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsScript : MonoBehaviour
{
    public GameObject StatisticsText;

    public void Start()
    {
        var statistics = new Dictionary<StatisticsFieldName, float>();
        foreach (StatisticsFieldName e in Enum.GetValues(typeof(StatisticsFieldName)))
            statistics.Add(e, Load(e));
        StatisticsText.GetComponent<Text>().text = string.Format("Count of games: {0}\nWin: {1}(avg score: {4:f2})\nLose: {2}(avg score: {5:f2})\nDraw: {3}(avg score: {6:f2})",
                                                                 statistics[StatisticsFieldName.GameCount],                                                 
                                                                 statistics[StatisticsFieldName.WinCount],                                                 
                                                                 statistics[StatisticsFieldName.LoseCount],                                                 
                                                                 statistics[StatisticsFieldName.DrawCount],
                                                                 statistics[StatisticsFieldName.WinAvgScore],
                                                                 statistics[StatisticsFieldName.LoseAvgScore],
                                                                 statistics[StatisticsFieldName.DrawAvgScore]);
    }

    private float Load(StatisticsFieldName name)
    {
        float loadingValue;
        if (PlayerPrefs.HasKey(SaveTag.GetTag(name)))
        {
            loadingValue = PlayerPrefs.GetInt(SaveTag.GetTag(name));
            if (loadingValue == 0)
                loadingValue = PlayerPrefs.GetFloat(SaveTag.GetTag(name));
            return loadingValue;
        }
        else
        {
            return 0f;
        }
    }
    public void ResetButtonOnClick()
    {
        SaveTag.DeleteAllTags();
        Start();
    }
}

public static class SaveTag
{
    public static string GetTag(StatisticsFieldName name) => Enum.GetName(typeof(StatisticsFieldName), name);
    public static void DeleteAllTags()
    {
        foreach (StatisticsFieldName e in Enum.GetValues(typeof(StatisticsFieldName)))
            PlayerPrefs.DeleteKey(GetTag(e));
    }
}

public enum StatisticsFieldName
{
    GameCount,
    WinCount,
    WinAvgScore,
    LoseCount,
    LoseAvgScore,
    DrawCount,
    DrawAvgScore
}
