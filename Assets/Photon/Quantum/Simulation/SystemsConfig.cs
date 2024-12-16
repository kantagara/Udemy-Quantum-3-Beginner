namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  /// <summary>
  /// A Quantum configuration asset that will create and start Quantum systems in a data-driven way when starting the simulation.
  /// Can be assigned to <see cref="RuntimeConfig"/>. 
  /// If no config is assigned then a default selection of build-in systems is used (<see cref="DeterministicSystemSetup.CreateSystems(RuntimeConfig, SimulationConfig, SystemsConfig)"/>.
  /// The systems to be used can always be changed by code inside <see cref="DeterministicSystemSetup.AddSystemsUser(ICollection{SystemBase}, RuntimeConfig, SimulationConfig, SystemsConfig)"/>.
  /// </summary>
  [Serializable]
#if QUANTUM_UNITY
  [UnityEngine.CreateAssetMenu(menuName = "Quantum/Configurations/SystemsConfig", order = -897)]
#endif
  public partial class SystemsConfig : AssetObject {

    /// <summary>
    /// System that will be instatiated on simulation start.
    /// </summary>
    [Serializable]
    public abstract class SystemEntryBase {
      /// <summary>
      /// System type name. Use typeof(SystemBase).FullName to get a valid name programmatically. E.g. Quantum.Core.SystemSignalsOnly.
      /// </summary>
      [SerializableType(WarnIfNoPreserveAttribute = true)]
      public SerializableType<SystemBase> SystemType;
      /// <summary>
      /// Optional System name. If set, then the SystemType class needs to have a matching contructor.
      /// </summary>
      public string SystemName;
      /// <summary>
      /// Start system disabled.
      /// Set <see cref="SystemBase.StartEnabled"/> accordingly. The value is inversed to have a better default value in Unity inspectors.
      /// </summary>
      public bool StartDisabled;

      public abstract IReadOnlyList<SystemEntryBase> GetChildren();
    }

    public abstract class SystemEntryBase<T> : SystemEntryBase where T : SystemEntryBase, new() {
      
      public List<T> Children = new List<T>();
      public override IReadOnlyList<SystemEntryBase> GetChildren() => Children;
      
      public T AddSystem<TSystem>(string name = null, bool enabled = true) where TSystem : SystemBase {
        var entry = new T() {
          SystemType = typeof(TSystem),
          StartDisabled = !enabled,
          SystemName = name
        };
        Children.Add(entry);
        return entry;
      }
    }

    [Serializable]
    public class SystemEntry : SystemEntryBase<SubSystemEntry> {}
    
    [Serializable]
    public class SubSystemEntry : SystemEntryBase<SubSubSystemEntry> {}
    
    [Serializable]
    public class SubSubSystemEntry : SystemEntryBase {
      public override IReadOnlyList<SystemEntryBase> GetChildren() {
        return Array.Empty<SystemEntryBase>();
      }
    }

    /// <summary>
    /// System entries to be instantiated on simulation start.
    /// </summary>
    public List<SystemEntry> Entries = new();

    /// <summary>
    /// Converts the systems configuration into a list of system objects while calling the matching (Name, Children) contructors.
    /// This method throws AssertionExceptions on any invalid system configuration.
    /// 
    ///                                      SystemBase   
    ///        __________________________________|___________________________________________________________
    ///       |                 |                |                    |                     |                |
    /// SystemGroup     SystemMainThread  SystemArrayComponent  SystemArrayFilter  SystemSignalsOnly  SystemThreadedFilter
    ///  children (SystemBase)  |
    ///               __________|__________
    ///              |                     |
    ///   SystemMainThreadGroup  SystemMainThreadFilter
    ///        children (SystemMainThread)
    /// </summary>
    public static List<SystemBase> CreateSystems(SystemsConfig config) {
      Assert.Always(config != null, "SystemsConfig is invalid.");

      var result = new List<SystemBase>();

      for (int i = 0; i < config.Entries.Count; i++) {
        try {
          result.Add(CreateSystems<SystemBase>(config.Entries[i]));
        } catch (Exception e) {
          Log.Error($"Creating system failed from asset '{config.Path}' at index {i} with error: {e.Message}");
        }
      }

      return result;
    }

    private static SystemBase CreateSystems<RequiredBaseType>(SystemEntryBase entry) {
      if (entry.SystemType.AssemblyQualifiedName.Contains(", Quantum.Game, Version")) {
        throw new Exception("The assembly 'Quantum.Game' is not supported anymore, edit the SystemsConfig file and replace 'Quantum.Game' with 'Quantum.Simulation'");
      }

      var type = entry.SystemType.Value;

      Assert.Always(type != null, "SystemType not set");
      Assert.Always(type.IsAbstract == false, "Cannot create abstract SystemType {0}", type);
      Assert.Always(typeof(RequiredBaseType).IsAssignableFrom(type), "System type {0} must be derived from {1}", type, typeof(RequiredBaseType).Name);

      var result = default(SystemBase);
      var childrenEntries = entry.GetChildren();

      if (typeof(SystemGroup).IsAssignableFrom(type)) {
        Assert.Always(childrenEntries != null, "SystemType {0} is derived from SystemGroup and requires the Children parameter to be not null.", type);; 
        var children = new List<SystemBase>(childrenEntries.Count);
        for (int i = 0; i < childrenEntries.Count; i++) {
          children.Add(CreateSystems<SystemBase>(childrenEntries[i]) as SystemBase);
        }
        result = Create(type, entry.SystemName, children.ToArray());
      } else if (typeof(SystemMainThreadGroup).IsAssignableFrom(type) ) {
        Assert.Always(childrenEntries != null, "SystemType {0} is derived from SystemMainThreadGroup and requires the Children parameter to be not null.", type); ;
        var children = new List<SystemMainThread>(childrenEntries.Count);
        for (int i = 0; i < childrenEntries.Count; i++) {
          children.Add(CreateSystems<SystemMainThread>(childrenEntries[i]) as SystemMainThread);
        }
        result = Create(type, entry.SystemName, children.ToArray());
      } else {
        result = Create<SystemBase>(type, entry.SystemName, null);
      }

      result.StartEnabled = !entry.StartDisabled;
      return result;
    }

    private static SystemBase Create<ChildrenType>(Type type, string name, ChildrenType[] children) where ChildrenType : SystemBase{
      // Conventions are: (), (name), (name, children)
      if (string.IsNullOrEmpty(name) == false && children != null) {
        Assert.Always(type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(string), typeof(ChildrenType[]) }, null) != null, 
          "SystemType {0} does not have the contructor for (string, {1}).", type, typeof(ChildrenType));
        return Activator.CreateInstance(type, name, children) as SystemBase;
      }
      else if (string.IsNullOrEmpty(name) == false){
        Assert.Always(type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(string) }, null) != null,
          "SystemType {0} does not have the contructor for (string).", type);
        return Activator.CreateInstance(type, name) as SystemBase;
      }
      else {
        Assert.Always(type.GetConstructor(Type.EmptyTypes) != null,
          "SystemType {0} does not have a default contructor", type);
        return Activator.CreateInstance(type) as SystemBase;
      }
    }

    public SystemEntry AddSystem<T>(string name = null, bool enabled = true) where T : SystemBase {
      return AddSystem(typeof(T), name, enabled);
    }
    
    public SystemEntry AddSystem(Type systemType, string name = null, bool enabled = true) {
      if (systemType == null) throw new ArgumentNullException(nameof(systemType));
      
      var entry = new SystemEntry() {
        SystemType = systemType,
        StartDisabled = !enabled,
        SystemName = name
      };
      Entries.Add(entry);
      return entry;
    }
    
#if QUANTUM_UNITY
    public override void Reset() {
      AddSystem<Core.CullingSystem2D>();
      AddSystem<Core.CullingSystem3D>();
      AddSystem<Core.PhysicsSystem2D>();
      AddSystem<Core.PhysicsSystem3D>();
      AddSystem<Core.NavigationSystem>();
      AddSystem<Core.EntityPrototypeSystem>();
      AddSystem<Core.PlayerConnectedSystem>();
    }
#endif
  }
}