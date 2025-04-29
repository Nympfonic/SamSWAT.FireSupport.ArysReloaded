using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using SamSWAT.FireSupport.ArysReloaded.Database;
using SamSWAT.FireSupport.ArysReloaded.Patches;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SamSWAT.FireSupport.ArysReloaded;

[BepInPlugin("com.SamSWAT.FireSupport.ArysReloaded", "SamSWAT's FireSupport: Arys Reloaded", "2.3.0")]
public class FireSupportPlugin : BaseUnityPlugin
{
	private readonly List<UpdatableComponentBase> _componentsToUpdate = [];
	
	public static FireSupportPlugin Instance { get; private set; }
	
	internal static string Directory { get; private set; }
	internal static ManualLogSource LogSource { get; private set; }
	
	internal static ConfigEntry<bool> Enabled { get; private set; }
	internal static ConfigEntry<int> AmountOfStrafeRequests { get; private set; }
	internal static ConfigEntry<int> AmountOfExtractionRequests { get; private set; }
	internal static ConfigEntry<int> HelicopterWaitTime { get; private set; }
	internal static ConfigEntry<float> HelicopterExtractTime { get; private set; }
	internal static ConfigEntry<float> HelicopterSpeedMultiplier { get; private set; }
	internal static ConfigEntry<int> RequestCooldown { get; private set; }
	internal static ConfigEntry<int> VoiceoverVolume { get; private set; }
	
	private void Awake()
	{
		Instance = this;
		LogSource = Logger;
		Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		
		new GesturesMenuPatch().Enable();
		new AddItemToDatabasePatch().Enable();
		new AddLocaleToDatabasePatch().Enable();
		new InputManagerUtil().Enable();
		
		InitializeConfigBindings();
	}
	
	private void Update()
	{
		UpdateComponents();
	}
	
	public void RegisterComponent(UpdatableComponentBase component)
	{
		_componentsToUpdate.Add(component);
	}
	
	private void InitializeConfigBindings()
	{
		Enabled = Config.Bind(
			"",
			"Plugin state",
			true,
			new ConfigDescription("Enables/disables plugin"));
		
		AmountOfStrafeRequests = Config.Bind(
			"Main Settings",
			"Amount of autocannon strafe requests",
			2,
			new ConfigDescription("",
				new AcceptableValueRange<int>(0, 10)));
		AmountOfExtractionRequests = Config.Bind(
			"Main Settings",
			"Amount of helicopter extraction requests",
			1,
			new ConfigDescription("",
				new AcceptableValueRange<int>(0, 10)));
		RequestCooldown = Config.Bind(
			"Main Settings",
			"Cooldown between support requests",
			300,
			new ConfigDescription("Seconds",
				new AcceptableValueRange<int>(60, 3600)));
		
		HelicopterWaitTime = Config.Bind(
			"Helicopter Extraction Settings",
			"Helicopter wait time",
			30,
			new ConfigDescription("Helicopter wait time on extraction location (seconds)",
				new AcceptableValueRange<int>(10, 300)));
		HelicopterExtractTime = Config.Bind(
			"Helicopter Extraction Settings",
			"Extraction time",
			10f,
			new ConfigDescription("How long you will need to stay in the exfil zone before extraction (seconds)",
				new AcceptableValueRange<float>(1f, 30f)));
		HelicopterSpeedMultiplier = Config.Bind(
			"Helicopter Extraction Settings",
			"Helicopter speed multiplier",
			1f,
			new ConfigDescription("How fast the helicopter arrival animation will be played",
				new AcceptableValueRange<float>(0.8f, 1.5f)));
		
		VoiceoverVolume = Config.Bind(
			"Sound Settings",
			"Voiceover volume",
			90,
			new ConfigDescription("",
				new AcceptableValueRange<int>(0, 100)));
	}
	
	private void UpdateComponents()
	{
		if (_componentsToUpdate.Count == 0)
		{
			return;
		}
		
		_componentsToUpdate.RemoveAll(x => x.IsMarkedForRemoval());

		int count = _componentsToUpdate.Count;
		for (var i = 0; i < count; i++)
		{
			UpdatableComponentBase component = _componentsToUpdate[i];
			
			if (component.IsMarkedForRemoval() || !component.HasFinishedInitialization)
			{
				continue;
			}
			
			component.ManualUpdate();
		}
	}
}