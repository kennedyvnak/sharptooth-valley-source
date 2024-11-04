using System.Collections;
using UnityEngine;

namespace NFHGame.Screens {
    public interface IScreen {
        bool screenActive { get; internal set; }
        bool dontSelectOnActive { get; }
        bool poppedByInput { get; }

        GameObject selectOnOpen { get; }

        IEnumerator OpenScreen();
        IEnumerator CloseScreen();
    }
}