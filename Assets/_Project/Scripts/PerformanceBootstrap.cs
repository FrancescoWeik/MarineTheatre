using UnityEngine;

/// <summary>
/// Rimuove il cap di frame rate di default (30 fps) che Unity applica su Android
/// quando vSync è disattivato. Da eseguire una sola volta all'avvio, senza
/// bisogno di piazzarlo su un GameObject in scena.
/// </summary>
public static class PerformanceBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // vSync off: lasciamo che sia targetFrameRate a decidere il cap.
        QualitySettings.vSyncCount = 0;
        // 60 fps su mobile (schermi a 60 Hz). Se in futuro punti a device
        // 90/120 Hz, usa Screen.currentResolution.refreshRateRatio.
        Application.targetFrameRate = 60;
    }
}
