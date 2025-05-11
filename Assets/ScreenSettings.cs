using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class ScreenSettings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;
    public Toggle vsyncToggle;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++) {
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolutions[i].ToString()));
        }
    
        Resolution currentResolution = Screen.currentResolution;

        int currentIndex = PlayerPrefs.GetInt("resolution", -1);
        if (currentIndex == -1) {
            currentIndex = Array.IndexOf(resolutions, currentResolution);
        }

        resolutionDropdown.value = Array.IndexOf(resolutions, currentResolution);

        if (PlayerPrefs.HasKey("vsync")) {
            bool enabled = PlayerPrefs.GetInt("vsync") == 1;
            vsyncToggle.isOn = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;
        }
        else {
            vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetResolution() {
        int currentIndex = resolutionDropdown.value;
        Resolution rez = resolutions[currentIndex];
        Screen.SetResolution(rez.width, rez.height, Screen.fullScreen);
        PlayerPrefs.SetInt("resolution", currentIndex);
    }

    public void SetVSync(bool enabled) {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        PlayerPrefs.SetInt("vsync", enabled ? 1 : 0);
    }
}
