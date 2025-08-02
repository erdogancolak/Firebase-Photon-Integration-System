using UnityEngine;

public static class GameStateManager
{
    public static string LastSceneName = "ClientScene"; 

    public static string LastRoomName = null;

    public static bool IsNoInternetPanelActive { get; set; } = false;

    public static bool IsSigningOut { get; set; } = false;
}
