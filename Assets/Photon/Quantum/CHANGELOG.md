# 3.0.0

**Breaking Changes**

- The Quantum SDK is now a unitypackage.
- Renamed PhotonDeterministic.dll to `Quantum.Deterministic.dll` and QuantumCore.dll to `Quantum.Engine.dll`.
- Upgraded network libraries to Photon Realtime 5 (see Realtime changelog separately).
- All Unity-only scripts have been put in `Quantum` namespace and have been given `Quantum` prefix (e.g. `MapData` -> `QuantumMapData`,  `EntityPrototype` -> `QuantumEntityPrototype`).
- The `PhysicsCollider` 2D and 3D components now have a field called `Layer Source` which defines where the layer info should come from. This might require colliders layers to be set again;
- The `AssetBase` class is no longer functional.
- `AssetObject` now derive from `UnityEngine.ScriptableObject`. This means that there is no longer the need for `AssetBase` wrapper and all the partial `AssetBase` extensions need to be moved to `AssetObject` definitions. Tools that migrate asset data without data loss are provided on our website.
- `AssetObjects` need to be created with `AssetObject.Create<T>` or `T.Create`.
- To add a new list of assets (e.g. with Addressables), mark a method with `[QuantumGlobalScriptableObjectLoaderMethod]` attribute.
- `IAssetSerializer` interface changed, it also deals with `RuntimePlayer` and `RuntimeConfig` serialization
- `AssetObjects` referencing other `AssetObjects` directly (not by `AssetRef<T>`) are supported, but will no longer be fully serializable by the default `IAssetSerializer`. This should be a concern for when any non-Unity runner is used or such assets are used in the `DynamicDB`.
- More consistent `DynamicAssetDB` behaviour - assets can be added and disposed only during verified frames. Also, `AssetObject.Disposed` is called whenever a dynamic asset is disposed. Previous behaviour can be restored with `QuantumGameFlags.EnableLegacyDynamicDBMode` game flag
- Unless `QuantumGameFlags.EnableLegacyDynamicDBMode` is used, all assets in the `DynamicAssetDB` will get disposed upon shutting down simulation.
- Removed "standalone assets" / `QPrefabs`. They have been fully replaced with standalone prototypes. All `_data` files should be removed.
- Removed built-in support for AssetBundles.
- `RuntimeConfig` and `RuntimePlayer` are serialized with Json when send to the server.
- Added the `$type` property to Json serialized `RuntimeConfig` and `RuntimePlayer` to be able to deserialize the file outside of Unity, use `QuantumUnityJsonSerializer.SerializeConfig(Stream stream, IRuntimeConfig config)` to export RuntimePlayer and RuntimeConfig or add `"$type":"Quantum.RuntimeConfig, Quantum.Simulation"` or `"$type":"Quantum.RuntimePlayer, Quantum.Simulation"` to the respective Jsons.
- `MapDataBakeCallbacks` implemented in a custom assembly it has to be made known by adding this `[assembly: QuantumMapBakeAssembly]` to the script.
- Changed the timing around the `GameStarted` callback. It is now only called once per started Quantum session and, when waiting for a snapshot, it will be called after the snapshot has arrived with `isResync` = true.
- Collections used in events are now passed by their appropriate pointer type (e.g. `list<T>` -> `QListPtr<T>`). Was: `Ptr`
- `Frame.Heap` is no longer a `Heap*` but instead a reference to a managed `FrameHeap` instance, which shares the same API and adds allocation tracking capabilities. Direct usages of `frame.Heap->` can be replaced by `frame.Heap.` instead.
- The default backing type for `flags` in DSL is now Int32 (was: Int64)
- Added  the `isResync` parameter to the `GameStarted` callback (which is true, when the callback is invoked after the game has been re-synced for example after a late-join).
- Changed `QuantumNavMesh.ImportSettings.LinkErrorCorrection` type from `bool` to `float` (representing a distance).
- `NavMeshRegionMask.HasValidRegions()` now returns `true` for the "MainArea", use `HasValidNoneMainRegion` instead to only query for non-MainArea regions
- AppSettings.RealtimeAppId was replaced by AppSettings.QuantumAppId, the Unity inspector will automatically swap them when the PhotonServerSettings asset is inspected.
- Changed the replay file format and renamed it to `QuantumReplayFile`, marking the old name obsolete. Asset DB, RuntimeConfig and InputHistory are now saved with a Json friendly wrapper that works around how verbose the Unity Json tool saves byte arrays, reducing the replay file size when saved in Unity a lot,
- The Quantum3 server will automatically block all non-protocol messages and all Photon Realtime player properties. Unblock them using the Photon dashboard and set `BlockNonProtocolMessages` and `BlockPlayerProperties` to `false`.
- Removed `DeterministicGameMode.Spectating` (use `Multiplay`) because all online Quantum simulation are started in spectating mode by default until a player is added.
- Also check out other references 
  - Migration Guide: `https://doc.photonengine.com/quantum/v3/getting-started/migration-guide`
  - What's New: `https://doc.photonengine.com/quantum/v3/getting-started/whats-new`
  - Photon Realtime 5 changelogs (`Assets\Photon\PhotonLibs\changes-library.txt`, `Assets\Photon\PhotonRealtime\Code\changes-realtime.txt`)

**What's New**

- Added input delta compression.
- Increasing minimum Unity version to 2021 LTS.
- New start online protocol to support adding and removing players at runtime.
- Added Quantum webhooks for the Photon Public Cloud.
- Increased maximum player count to 128.
- Added support for up to 512 components (use `#pragma max_components 512` to enable)
- Added predicted commands.
- Added support for capsule shapes.
- Added snapshot interpolation mode for entities.
- Added `QuantumHud` window that displays onboarding information and installs Quantum user scripts,  assets and demos.
- Added a new graphical demo menu that can be installed with the QuantumHub and includes MPPM support for Unity 6.
- Added the Asteroid game sample that can be installed with the QuantumHub.
- Added a new GUI to toggle Quantum gizmos.
- Added async support for connection handling.
- Added support for assets that are neither a Resource nor Addressable. Direct references to such assets are stored in `QuantumUnityDB`.
- `QuantumUnityDB.Get[Global]AssetGuids` - ability to iterate over GUIDs, based on asset types.
- Added support for `netstandard2.1`.
- Added full support for Odin inspector.
- Polished Quantum Unity inspector and added code documentation foldouts.
- Added a Newtonsoft-powered Json deserializer can now read `[SerializeReferences]` (uses the  `Quantum.Json.dll` dependency for non-Unity platforms).
- Added a data-driven way of adding systems (see `SystemsConfig.asset`).
- Added `AllowedLobbyProperties` dashboard variable to restrict lobby property usage for Photon matchmaking.
- Added `Quantum.Log.dll` dependency, which introduces the `Quantum.LogType` and can clash with `UnityEngine.LogType` when migrating.
- Added Quantum debug dll toggle to switch between Debug and Release Quantum dlls in Unity
- Added position and rotation teleport support for Transform2D, Transform3D and QuantumEntityView.
- Increased max navmesh region count to 128.
- Added `Frame.PlayerConnectedCount`.
- Added `FrameThreadSafe.GetGlobal()` extension method.
- Added `EntityViewComponent`s, a quick way to add view logic to the Quantum entity views.
- Added entity view pooling (add `QuantumEntityViewPool` to the `EntityViewUpdater` game object)
- Added dotnet Quantum dll build (see `QuantumDotnetBuildSettings` asset).
- Added a dotnet console replay runner app project that is generated to the non-Unity simulation project.
- Added a tool to quickly convert Unity colliders to Quantum colliders `GameObject/Quantum/Convert Colliders`.
- Added Quantum Unity script templates.
- Added `DynamicMap` core type, a specialization of Map as a Dynamic Asset capable of triggering internal updates when modifying static colliders and of serializing runtime mesh data.

**Changes**

- Unified CodeGen tools into one that is used from the UnityEditor.
- Unified QuantumRunner and SessionContainer into the `SessionRunner` class and moved it to the `QuantumGame` project.
- Component prototype suffix has been changed from `_Prototype` to `Prototype`.
- Component prototype wrapper prefix changed from `EntityComponent` to `QPrototype` (e.g. `EntityComponentTransform2D` -> `QPrototypeTransform2D`).
- `Frame.Assets` is now obsolete. Use `Frame.FindAsset<T>` instead.
- `AssetObjectConfigAttribute` is obsoleted and no longer functional.
- `AssetObject` is no longer restricted to being in the same assembly as Quantum simulation code. If the simulation code needs to access such assets, it can either use a base class or an interface. This allows `AssetObjects` to be extended with any Unity-specific data.
- `AssetRefT` generated types are now obsolete, use `AssetRef<T>` instead.
- `asset T;` and `import asset T;` are no longer needed in .qtn files. Any type derived from `AssetObject` is already a fully functional Quantum asset now and can be used with `AssetRef<T>` fields.
- `AssetObject.Guid` is now deterministic by default, based on Unity's GUID and `fileId`. When needed can be overridden (e.g. for assets imported from Quantum 2.1) - such overrides are stored in `QuantumEditorSettings`. Turning `AssetGuids` deterministic speeds up `QuantumUnityDB` by an order of magnitude.
- `UnityDB` has been obsoleted and `AssetResourceContainer` has been removed. `QuantumUnityDB` replaced both and uses more consistent method naming.
- Prototype assets for new prefabs are created as standalone assets, with `EntityPrototype` suffix and `qprototype` extension. This fully decouples loading prototypes during simulation from their source prefab, improving asset load times and avoiding deadlock issues with Unity's job system.
- All Quantum assets located in asset search paths are now marked with `QuantumAsset` label.
- Moved NavMesh baking to the `Quantum Simulation` project to bake parts deterministically (not the Unity navmesh export).
- Removed the `MovementType.DynamicBody` and `MovementType.CharacterController2D` from the NavmeshAgentConfig. Select `MovementType.Callback` instead and perform movement during the `ISignalOnNavMeshMoveAgent` callback. The removed options were only good for prototyping and led to a lot of questions.
- Changed `QuantumNavMesh.BakeData` and related data structures to only use fixed point vectors.
- Replaced non-deterministic code (Triangle Normal Computation) in `BakeNavMesh()` with fixed point math.
- Moved navmesh link detection and error correction from `BakeNavMesh()` to `ImportFromUnity()`, `StartTriangle` and `EndTriangle` now are set on `BakeData.Links`.
- The MainArea of the navmesh now has its own valid navmesh region (always index 0) and can be toggled on/off and can be properly used for navmesh queries like `LineOfSight()` or `FindClosestTriangle()`.
- Changed `BakeNavMesh` signature to only use relevant data types.
- Replaced the signal `OnPlayerDataSet(PlayerRef player)` with `OnPlayerAdded(PlayerRef player, bool firstTime)` where `firstTime` signals this is the first time that this player was assigned..
- Added `OnPlayerRemoved(PlayerRef player)` signal.
- Added a parameter to the `GameStarted` callback (`isResync`: true, when the callback is invoked after the game has been re-synced for example after a late-join).
- Removed the `ReleaseProfiler` configuration from the code solutions and replacing it with a flag in `StartParameters.GameFlags` called `QuantumGameFlags.EnableTaskProfiler`.
- Removed API that was deprecated in Quantum 2.1.
- All Quantum Unity MonoBehaviours now derive from `QuantumMonoBehaviour`.
- All Quantum Unity ScriptableObjects now derive from `QuantumScriptableObject`.
- The state inspector now shows all relevant configs when selecting the Runner.
- Restructured the Quantum Unity menus.
- `Navigation.Raycast2D()` provides information about the border index that generated the closest hit.
- Reduced the navmesh binary asset size, control the serialization options under `NavMesh.SerializeType`. By default meta data is now recomputed during runtime, set to `NavMeshSerializeType.FullWithMetaData` to disable it.
- Drawing debug shapes for development builds must now be opted in by defining the `QUANTUM_DRAW_SHAPES` define (`QuantumEditorSettings` has a shortcut to toggle it).
- Quantum runtime logging can now be toggled by a `LogLevel` inside the `EditorSettings`, the default is `WARN`.
- The context `LayerInfo` is now exposed in `FrameThreadSafe.Layers` property, similarly to `Frame.Layers`.
- `Quantum.TypeUtils` is now obsolete

## RC

### Build 1514 (Jun 18, 2024)

- Initial release

