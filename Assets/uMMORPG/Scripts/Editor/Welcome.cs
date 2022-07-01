// Shows either a welcome message, or warning about a recommended Unity version.
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DOTSNET.Editor
{
    static class Welcome
    {
        [InitializeOnLoadMethod]
        static void OnInitializeOnLoad()
        {
            // InitializeOnLoad is called on start and after each rebuild,
            // but we only want to show this once per editor session.
            if (!SessionState.GetBool("UMMORPG_WELCOME", false))
            {
                SessionState.SetBool("UMMORPG_WELCOME", true);

#if UNITY_2021_2
                Debug.Log("uMMORPG 2D | u3d.as/YUh | https://discord.gg/2gNKN78");
#else
                Debug.LogWarning("uMMORPG 2D works best with Unity 2021.2!");
#endif
            }
        }
    }
}
#endif