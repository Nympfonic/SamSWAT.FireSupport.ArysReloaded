using Cysharp.Text;
using Cysharp.Threading.Tasks;
using EFT.InputSystem;
using EFT.UI;
using EFT.UI.Gestures;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using System.Threading;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

/// <summary>
/// Entry point for Fire Support in-raid logic.
/// </summary>
public class FireSupportController : UIInputNode
{
	[NonSerialized] private CancellationTokenSource _cancellationTokenSource;
	[NonSerialized] private FireSupportAudio _audio;
	[NonSerialized] private FireSupportUI _ui;
	[NonSerialized] private FireSupportSpotter _spotter;
	[NonSerialized] private GesturesMenu _gesturesMenu;
	
	[NonSerialized] private readonly FireSupportServiceMappings _services = new(new SupportTypeComparer());
	
	public static FireSupportController Instance { get; private set; }
	
	public static async UniTask<FireSupportController> Create(GesturesMenu gesturesMenu)
	{
		Instance = new GameObject("FireSupportController").AddComponent<FireSupportController>();
		await Instance.Initialize(gesturesMenu);
		return Instance;
	}
	
	private async UniTask Initialize(GesturesMenu gesturesMenu)
	{
		_cancellationTokenSource = new CancellationTokenSource();
		_gesturesMenu = gesturesMenu;
		_audio = await FireSupportAudio.Create();
		_spotter = await FireSupportSpotter.Load();
		
		var heliExfil = new HeliExfiltrationService(
			_spotter,
			_cancellationTokenSource.Token,
			FireSupportPlugin.AmountOfExtractionRequests.Value);
		_services.Add(heliExfil.SupportType, heliExfil);
		
		var jetStrafe = new JetStrafeService(
			_spotter,
			_cancellationTokenSource.Token,
			FireSupportPlugin.AmountOfStrafeRequests.Value);
		_services.Add(jetStrafe.SupportType, jetStrafe);
		
		_ui = await FireSupportUI.Load(_services, gesturesMenu);
		_ui.SupportRequested += OnSupportRequested;
		
		await FireSupportPoolManager.Initialize(10);
		
		_audio.PlayVoiceover(EVoiceoverType.StationReminder);
	}
	
	private void OnDestroy()
	{
		_ui.SupportRequested -= OnSupportRequested;
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource.Dispose();
		AssetLoader.UnloadAllBundles();
	}
	
	private void OnSupportRequested(ESupportType supportType)
	{
		try
		{
			if (!_services.TryGetValue(supportType, out IFireSupportService service))
			{
				throw new ArgumentException($"No service registered for support type {supportType}");
			}
			
			if (!service.IsRequestAvailable())
			{
				FireSupportPlugin.LogSource.LogWarning("Should not be able to reach this line, bad logic somewhere...");
				return;
			}
			
			_gesturesMenu.Close();
			service.PlanRequest().Forget();
		}
		catch (OperationCanceledException) {}
		catch (Exception ex)
		{
			FireSupportPlugin.LogSource.LogError(ex);
		}
	}
	
	public async UniTaskVoid StartCooldown(float time, CancellationToken cancellationToken, Action callback = null)
	{
		_ui.timerText.enabled = true;
		
		while (time > 0)
		{
			time -= Time.deltaTime;
			if (time < 0)
			{
				time = 0;
			}
			
			float minutes = Mathf.FloorToInt(time / 60);
			float seconds = Mathf.FloorToInt(time % 60);
			
			using (Utf16ValueStringBuilder sb = ZString.CreateStringBuilder())
			{
				sb.AppendFormat("{0:00}.{1:00}", minutes, seconds);
				_ui.timerText.text = sb.ToString();
			}
			
			await UniTask.NextFrame(cancellationToken);
		}
		
		_ui.timerText.enabled = false;
		
		if (_services.AnyAvailableRequests())
		{
			FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationAvailable);
		}
		
		callback?.Invoke();
	}
	
	public override ETranslateResult TranslateCommand(ECommand command)
	{
		return ETranslateResult.Ignore;
	}
	
	public override void TranslateAxes(ref float[] axes) {}
	
	public override ECursorResult ShouldLockCursor()
	{
		return ECursorResult.Ignore;
	}
}