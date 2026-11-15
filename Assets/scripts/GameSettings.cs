using UnityEngine;

public static class GameSettings
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string PlayerColorKey = "PlayerColor";

    public static float MusicVolume => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
    public static int PlayerColorIndex => PlayerPrefs.GetInt(PlayerColorKey, 0);

    public static readonly Color[] PlayerColors =
    {
        Color.white,
        new Color(1f, 0.35f, 0.35f),
        new Color(0.3f, 0.8f, 1f),
        new Color(0.4f, 1f, 0.45f),
        new Color(0.8f, 0.45f, 1f)
    };

    public static Color PlayerColor => PlayerColors[Mathf.Clamp(PlayerColorIndex, 0, PlayerColors.Length - 1)];

    public static void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    public static void SetPlayerColor(int index)
    {
        PlayerPrefs.SetInt(PlayerColorKey, Mathf.Clamp(index, 0, PlayerColors.Length - 1));
        PlayerPrefs.Save();
    }
}
