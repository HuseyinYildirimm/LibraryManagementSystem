using UnityEngine;
using TMPro;
using System;

public class DateController : MonoBehaviour
{
    private DateTime lastClosedTime;
    public TextMeshProUGUI timeText;

    [HideInInspector] public int minutesPassed;


    public void Awake()
    {
        if (PlayerPrefs.HasKey("LastClosedTimer"))
        {
            string savedLastClosedTime = PlayerPrefs.GetString("LastClosedTimer");
            if (!DateTime.TryParse(savedLastClosedTime, out lastClosedTime))
            {
                lastClosedTime = DateTime.Now;
            }
        }

        CalculateXValue();
    }

    void Update()
    {
        timeText.text = DateTime.Now.ToString();

    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetString("LastClosedTimer", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();
    }


    public void CalculateXValue()
    {
        TimeSpan timePassed = DateTime.Now - lastClosedTime;
        minutesPassed = (int)timePassed.TotalMinutes;
        Debug.Log(minutesPassed);
    }
}
