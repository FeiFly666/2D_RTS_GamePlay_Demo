using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
    float timer;
    int frames;
    int fps;

    string text;

    void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= 1f)
        {
            fps = frames;
            frames = 0;
            timer = 0;

            text =  $"FPS: {fps}\n" +
                    $"Humans: {GameManager.Instance.liveHumanUnits.Count}\n" +
                    $"Buildings: {GameManager.Instance.buildings.Count}\n" +
                    $"GC: {System.GC.GetTotalMemory(false) / 1024 / 1024} MB";
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 100), text);
    }
}