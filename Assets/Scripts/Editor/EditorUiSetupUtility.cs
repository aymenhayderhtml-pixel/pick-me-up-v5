using UnityEngine;
using UnityEngine.EventSystems;

public static class EditorUiSetupUtility
{
    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<StandaloneInputModule>();
    }
}
