using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PresetColorOption
{
    Random,
    Manual
}
public class CarCustomization
{
    public Color mainColor;
    public Color secondaryColor; // SkyCarComponents and SkyCarMudGuards
    public Color wheelColor;
    public Color headlighColor;
    
    public void SetupRandomColors()
    {
        mainColor = PlayerStats.RandomColor();
        secondaryColor = PlayerStats.RandomColor();
        wheelColor = PlayerStats.RandomColor();
    }

    public void SetupColors(Color main, Color secondary, Color wheel)
    {
        mainColor = main;
        secondaryColor = secondary;
        wheelColor = wheel;
    }
}
