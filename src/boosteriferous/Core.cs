using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace boosteriferous
{
    public abstract class Core : MonoBehaviour
    {
        public static Core Instance { get; protected set; }

        public event Action OnGuiUpdate = delegate { };

        public void Start()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void OnGUI()
        {
            GUI.depth = 0;
            OnGuiUpdate.Invoke();

            Action windows = delegate { };
            foreach (var window in UI.AbstractWindow.Windows.Values)
            {
                windows += window.Draw;
            }
            windows.Invoke();
        }

        public void OnDestroy()
        {
            Instance = null;
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class CoreFlight : Core
    {
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class CoreEditor : Core
    {
    }
}
