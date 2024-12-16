namespace Quantum {
  using System;
  using System.Runtime.InteropServices;
  using Photon.Deterministic;
  using Quantum.Core;
  using Quantum.Allocator;
#if QUANTUM_UNITY
  using UnityEditor;
  using UnityEngine;
  using HideInInspector = UnityEngine.HideInInspector;
#endif

  /// <summary>
  /// The SimulationConfig holds parameters used in the ECS layer and inside core systems like physics and navigation.
  /// </summary>
  public partial class SimulationConfig : AssetObject {
    [Obsolete("Don't use the hard coded guids instead reference the simulation config used in the RuntimeConfig")]
    public const long DEFAULT_ID = (long)DefaultAssetGuids.SimulationConfig;

    public enum AutoLoadSceneFromMapMode {
      Disabled,
      Legacy,
      UnloadPreviousSceneThenLoad,
      LoadThenUnloadPreviousScene
    }

    /// <summary>
    /// Global entities configuration
    /// </summary>
    [Space, InlineHelp]
    public FrameBase.EntitiesConfig Entities;
    
    /// <summary>
    /// Global physics configurations.
    /// </summary>
    [Space, InlineHelp]
    public PhysicsCommon.Config Physics;
    
    /// <summary>
    /// Global navmesh configurations.
    /// </summary>
    [Space, InlineHelp]
    public Navigation.Config Navigation;

    /// <summary>
    /// This option will trigger a Unity scene load during the Quantum start sequence.\n
    /// This might be convenient to start with but once the starting sequence is customized disable it and implement the scene loading by yourself.
    /// "Previous Scene" refers to a scene name in Quantum Map.
    /// </summary>
    [Space, InlineHelp]
    public AutoLoadSceneFromMapMode AutoLoadSceneFromMap = AutoLoadSceneFromMapMode.UnloadPreviousSceneThenLoad;

    /// <summary>
    /// Configure how the client tracks the time to progress the Quantum simulation from the QuantumRunner class.
    /// </summary>
    [Obsolete("Set on SessionRunner.Arguments.DeltaTimeType instead")]
    [HideInInspector]
    public SimulationUpdateTime DeltaTimeType = SimulationUpdateTime.Default;

    /// <summary>
    /// Override the number of threads used internally. Default is 2.
    /// </summary>
    [InlineHelp]
    public int ThreadCount = 2;

    /// <summary>
    /// How long to store checksumed verified frames. The are used to generate a frame dump in case of a checksum error happening. Not used in Replay and Local mode. Default is 3.
    /// </summary>
    [InlineHelp]
    public FP ChecksumSnapshotHistoryLengthSeconds = 3;

    /// <summary>
    /// Additional options for checksum dumps, if the default settings don't provide a clear picture. 
    /// </summary>
    [InlineHelp]
    public SimulationConfigChecksumErrorDumpOptions ChecksumErrorDumpOptions;

    /// <summary>
    /// If and to which extent allocations in the Frame Heap should be tracked when in Debug mode.
    /// Recommended modes for development is `DetectLeaks`.
    /// While actively debugging a memory leak,`TraceAllocations` mode can be enabled (warning: tracing is very slow).
    /// </summary>
    [Header("Frame Heap Settings")]
    [InlineHelp]
    public HeapTrackingMode HeapTrackingMode = HeapTrackingMode.DetectLeaks;

    /// <summary>
    /// Define the max heap size for one page of memory the frame class uses for custom allocations like QList for example. The default is 15.
    /// </summary>
    /// <remarks>2^15 = 32.768 bytes</remarks>
    /// <remarks><code>TotalHeapSizeInBytes = (1 &lt;&lt; HeapPageShift) * HeapPageCount</code></remarks>
    [InlineHelp]
    public int HeapPageShift = 15;

    /// <summary>
    /// Define the max heap page count for memory the frame class uses for custom allocations like QList for example. Default is 256.
    /// </summary>
    /// <remarks><code>TotalHeapSizeInBytes = (1 &lt;&lt; HeapPageShift) * HeapPageCount</code></remarks>
    [InlineHelp]
    public int HeapPageCount = 256;

    /// <summary>
    /// Sets extra heaps to allocate for a session in case you need to
    /// create 'auxiliary' frames than actually required for the simulation itself.
    /// Default is 0.
    /// </summary>
    [InlineHelp]
    public int HeapExtraCount = 0;


    public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator) {
      Physics.PenetrationCorrection = FPMath.Clamp01(Physics.PenetrationCorrection);
    }
    
#if QUANTUM_UNITY
    public override void Reset() {
      Physics    = new PhysicsCommon.Config();
      Navigation = new Navigation.Config();

      ImportLayersFromUnity(PhysicsType.Physics3D);
    }

    void ImportLayersFromUnity3D() {
      ImportLayersFromUnity(PhysicsType.Physics3D);
#if UNITY_EDITOR
      EditorUtility.SetDirty(this);
#endif
    }
    
    void ImportLayersFromUnity2D() {
      ImportLayersFromUnity(PhysicsType.Physics2D);
#if UNITY_EDITOR
      EditorUtility.SetDirty(this);
#endif
    }
    
    public enum PhysicsType {
      Physics3D,
      Physics2D
    }
    
    public void ImportLayersFromUnity(PhysicsType physicsType = PhysicsType.Physics3D) {
      Physics.Layers      = GetUnityLayerNameArray();
      Physics.LayerMatrix = GetUnityLayerMatrix(physicsType);
    }
    
    public static String[] GetUnityLayerNameArray() {
      var layers = new String[32];

      for (Int32 i = 0; i < layers.Length; ++i) {
        try {
          layers[i] = UnityEngine.LayerMask.LayerToName(i);
        } catch {
          // just eat exceptions
        }
      }

      return layers;
    }

    public static Int32[] GetUnityLayerMatrix(PhysicsType physicsType) {
      var matrix = new Int32[32];

      for (Int32 a = 0; a < 32; ++a) {
        for (Int32 b = 0; b < 32; ++b) {
          bool ignoreLayerCollision = false;
          
          switch (physicsType) {
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
            case PhysicsType.Physics3D:
              ignoreLayerCollision = UnityEngine.Physics.GetIgnoreLayerCollision(a, b);
              break;
#endif
#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
            case PhysicsType.Physics2D:
              ignoreLayerCollision = UnityEngine.Physics2D.GetIgnoreLayerCollision(a, b);
              break;
#endif
            default:
              break;
          }

          if (ignoreLayerCollision == false) {
            matrix[a] |= (1 << b);
            matrix[b] |= (1 << a);
          }
        }
      }

      return matrix;
    }    
#endif
  }

  [Flags]
  public enum SimulationConfigChecksumErrorDumpOptions {
    SendAssetDBChecksums = 1 << 0,
    ReadableDynamicDB    = 1 << 1,
    RawFPValues          = 1 << 2,
    ComponentChecksums   = 1 << 3,
  }

}
