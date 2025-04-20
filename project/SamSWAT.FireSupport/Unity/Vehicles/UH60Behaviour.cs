using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public class UH60Behaviour : ComponentBase, IFireSupportOption
{
	private static readonly int s_flySpeedMultiplier = Animator.StringToHash("FlySpeedMultiplier");
	private static readonly int s_flyAway = Animator.StringToHash("FlyAway");
	
	[SerializeField] private Animator helicopterAnimator;
	[SerializeField] private AnimationCurve volumeCurve;
	public AudioSource engineCloseSource;
	public AudioSource engineDistantSource;
	public AudioSource rotorsCloseSource;
	public AudioSource rotorsDistantSource;
	
	public void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation)
	{
		Transform heliTransform = transform;
		heliTransform.position = position;
		heliTransform.eulerAngles = rotation;
		helicopterAnimator.SetFloat(s_flySpeedMultiplier, FireSupportPlugin.HelicopterSpeedMultiplier.Value);
	}
	
	public override void ManualUpdate()
	{
		CrossFadeAudio();
	}
	
	public void ReturnToPool()
	{
		gameObject.SetActive(false);
	}
	
	private void OnEnable()
	{
		FireSupportPlugin.RegisterComponent(this);
	}
	
	private void OnDisable()
	{
		FireSupportPlugin.DeregisterComponent(this);
	}
	
	private void CrossFadeAudio()
	{
		GameWorld gameWorld = Singleton<GameWorld>.Instance;
		if (gameWorld == null || gameWorld.MainPlayer == null)
		{
			FireSupportPlugin.DeregisterComponent(this);
			return;
		}
		
		float distance = Vector3.Distance(gameWorld.MainPlayer.CameraPosition.position, rotorsCloseSource.transform.position);
		float volume = volumeCurve.Evaluate(distance);
		
		rotorsCloseSource.volume = volume;
		engineCloseSource.volume = volume - 0.2f;
		rotorsDistantSource.volume = 1 - volume;
		engineDistantSource.volume = 1 - volume;
	}
	
	[UsedImplicitly]
	private IEnumerator OnHelicopterArrive()
	{
		FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliPickingUp);
		GameObject extractionPoint = CreateExfilPoint();
		float waitTime = FireSupportPlugin.HelicopterWaitTime.Value * 0.75f;
		
		yield return new WaitForSeconds(waitTime);
		
		FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliHurry);
		
		yield return new WaitForSeconds(FireSupportPlugin.HelicopterWaitTime.Value - waitTime);
		
		helicopterAnimator.SetTrigger(s_flyAway);
		Destroy(extractionPoint);
		FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliLeavingNoPickup);
	}
	
	[UsedImplicitly]
	private void OnHelicopterLeft()
	{
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
				eulerAngles = new Vector3(-90,0,0),
			}
		};
		var extractionCollider = extractionPoint.AddComponent<BoxCollider>();
		extractionCollider.size = new Vector3(16.5f, 20f, 15);
		extractionCollider.isTrigger = true;
		extractionPoint.AddComponent<HeliExfiltrationPoint>();
		
		return extractionPoint;
	}
}