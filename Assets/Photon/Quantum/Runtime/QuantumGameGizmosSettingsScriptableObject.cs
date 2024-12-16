namespace Quantum {
  using UnityEditor;
  using UnityEngine;

  [CreateAssetMenu(menuName = "Quantum/Configurations/GameGizmoSettings", fileName = "QuantumGameGizmoSettings",
    order = EditorDefines.AssetMenuPriorityConfigurations + 31)]
  [QuantumGlobalScriptableObject(DefaultPath)]
  public class QuantumGameGizmosSettingsScriptableObject : QuantumGlobalScriptableObject<QuantumGameGizmosSettingsScriptableObject> {
    public const string DefaultPath = "Assets/QuantumUser/Editor/QuantumGameGizmosSettings.asset";

    /// <summary>
    /// Open the overlay for the Quantum gizmos.
    /// </summary>
    [EditorButton]
    public void OpenOverlay() {
#if UNITY_EDITOR
      // get scene view 
      var sceneView = SceneView.lastActiveSceneView;
      if (sceneView == null) {
        sceneView = SceneView.sceneViews.Count > 0 ? (SceneView) SceneView.sceneViews[0] : null;
      }
      
      if (sceneView == null) {
        Debug.LogError("No scene view found.");
        return;
      }
      
      // get overlay
      if(sceneView.TryGetOverlay(QuantumGameGizmosSettings.ID, out var overlay)) {
        overlay.Undock();
        overlay.displayed = true;
        overlay.collapsed = false;
      } else {
        Debug.LogError("No overlay found.");
      }
#endif
    }

    /// <summary>
    /// The global and default settings for Quantum gizmos.
    /// </summary>
    [DrawInline] public QuantumGameGizmosSettings Settings;
  }
}