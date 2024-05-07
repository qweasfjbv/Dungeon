using System.Collections;
using UnityEngine;

public static class Settings
{

    #region SettingValues
    public static float WIDTH = 1920;
    public static float HEIGHT = 1080;

    // ÇöÀç width/1920, 1080
    public static float xScale = 1f;
    public static float yScale = 1f;

    public static float DIAGONAL = Mathf.Sqrt(WIDTH * WIDTH + HEIGHT * HEIGHT);
    public static float MAXCOS = HEIGHT / DIAGONAL;
    public static float MAXSIN = WIDTH / DIAGONAL;



    #endregion



}
