using Comfort.Common;
using Cysharp.Threading.Tasks;
using EFT;
using JetBrains.Annotations;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public sealed class UH60Behaviour : FireSupportBehaviour
{
	private static readonly int s_flySpeedMultiplier = Animator.StringToHash("FlySpeedMultiplier");
	private static readonly int s_flyAway = Animator.StringToHash("FlyAway");
	
	[SerializeField] private Animator helicopterAnimator;
	[SerializeField] private AnimationCurve volumeCurve;
	public AudioSource engineCloseSource;
	public AudioSource engineDistantSource;
	public AudioSource rotorsCloseSource;
	public AudioSource rotorsDistantSource;
	
	private CancellationToken? _cancellationToken;
	
	public override ESupportType SupportType => ESupportType.Extract;
	
	public override void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation,
		CancellationToken cancellationToken)
	{
		_cancellationToken = cancellationToken;
		
		Transform heliTransform = transform;
		heliTransform.position = position;
		heliTransform.eulerAngles = rotation;
		helicopterAnimator.SetFloat(s_flySpeedMultiplier, FireSupportPlugin.HelicopterSpeedMultiplier.Value);
	}
	
	public override void ManualUpdate()
	{
		CrossFadeAudio();
	}
	
	protected override void OnAwake()
	{
		AudioMixerGroup outputAudioMixerGroup = Singleton<BetterAudio>.Instance.OutEnvironment;
		engineCloseSource.outputAudioMixerGroup = outputAudioMixerGroup;
		engineDistantSource.outputAudioMixerGroup = outputAudioMixerGroup;
		rotorsCloseSource.outputAudioMixerGroup = outputAudioMixerGroup;
		rotorsDistantSource.outputAudioMixerGroup = outputAudioMixerGroup;
		
		HasFinishedInitialization = true;
	}
	
	private void CrossFadeAudio()
	{
		GameWorld gameWorld = Singleton<GameWorld>.Instance;
		if (!PlayerHelper.IsMainPlayerAlive())
		{
			return;
		}
		
		float distance = Vector3.Distance(gameWorld.MainPlayer.CameraPosition.position, rotorsCloseSource.transform.position);
		float volume = volumeCurve.Evaluate(distance);
		
		rotorsCloseSource.volume = Mathf.Clamp01(volume);
		engineCloseSource.volume = Mathf.Clamp01(volume - 0.2f);
		rotorsDistantSource.volume = Mathf.Clamp01(1 - volume);
		engineDistantSource.volume = Mathf.Clamp01(1 - volume);
	}
	
	[UsedImplicitly]
	private async UniTaskVoid OnHelicopterArrive()
	{
		FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliPickingUp);
		GameObject extractionPoint = CreateExfilPoint();
		float waitTime = FireSupportPlugin.HelicopterWaitTime.Value * 0.75f;
		
		await UniTask.WaitForSeconds(waitTime, cancellationToken: _cancellationToken!.Value);
		
		FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliHurry);
		
		await UniTask.WaitForSeconds(
			duration: FireSupportPlugin.HelicopterWaitTime.Value - waitTime,
			cancellationToken: _cancellationToken!.Value);
		
		helicopterAnimator.SetTrigger(s_flyAway);
		Destroy(extractionPoint);
		FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliLeavingNoPickup);
	}
	
	private async UniTask WaitForHelicopterLanding(CancellationToken cancellationToken)
	{
		
	}
	
	[UsedImplicitly]
	private void OnHelicopterLeft()
	{
		_cancellationToken = null;
		ReturnToPool();
	}
	
	private GameObject CreateExfilPoint()
	{
		var extractionPoint = new GameObject
		{
			name = "HeliExfilPoint",
			layer = 13,
			transform =
			{
				position = transform.position,
				eulerAngles = new Vector3(-90, 0, 0),
			}
		};
		var extractionCollider = extractionPoint.AddComponent<BoxCollider>();
		extractionCollider.size = new Vector3(16.5f, 20f, 15);
		extractionCollider.isTrigger = true;
		extractionPoint.AddComponent<HeliExfiltrationPoint>();
		
		return extractionPoint;
	}
}