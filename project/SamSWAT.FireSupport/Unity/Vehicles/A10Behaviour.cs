using Comfort.Common;
using Cysharp.Threading.Tasks;
using EFT;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public sealed class A10Behaviour : FireSupportBehaviour
{
	public AudioSource engineSource;
	public AudioClip[] engineSounds;
	
	[SerializeField] private AudioClip[] gau8Sound;
	[SerializeField] private AudioClip[] gau8ExpSounds;
	[SerializeField] private Transform gau8Transform;
	[SerializeField] private GameObject gau8Particles;
	[SerializeField] private GameObject flareCountermeasure;
	
	private GameObject _flareCountermeasureInstance;
	
	private BetterAudio _betterAudio;
	private FireSupportAudio _fireSupportAudio;
	
	private VehicleWeapon _weapon;
	private Player _player;
	
	public override ESupportType SupportType => ESupportType.Strafe;
	
	public override void ProcessRequest(
		Vector3 position,
		Vector3 direction,
		Vector3 rotation,
		CancellationToken cancellationToken)
	{
		Vector3 a10StartPos = position + 2650 * direction + 320 * Vector3.up;
		Vector3 a10Heading = position - a10StartPos;
		
		float a10YAngle = Mathf.Atan2(a10Heading.x, a10Heading.z) * Mathf.Rad2Deg;
		Quaternion a10Rotation = Quaternion.Euler(0, a10YAngle, 0);
		
		transform.SetPositionAndRotation(a10StartPos, a10Rotation);
		_flareCountermeasureInstance = Instantiate(flareCountermeasure, null);
		FlySequence(position, cancellationToken).Forget();
	}
	
	public override void ManualUpdate()
	{
		if (_flareCountermeasureInstance == null)
		{
			return;
		}
		
		Transform t = transform;
		_flareCountermeasureInstance.transform.position = t.position - t.forward * 6.5f;
		_flareCountermeasureInstance.transform.eulerAngles = new Vector3(90, t.eulerAngles.y, 0);
		transform.Translate(0, 0, 148 * Time.deltaTime, Space.Self);
	}
	
	protected override void OnAwake()
	{
		_fireSupportAudio = FireSupportAudio.Instance;
		_betterAudio = Singleton<BetterAudio>.Instance;
		_player = Singleton<GameWorld>.Instance.MainPlayer;
		_weapon = new VehicleWeapon(_player.ProfileId, ItemConstants.GAU8_WEAPON_TPL, ItemConstants.GAU8_AMMO_TPL);
		
		HasFinishedInitialization = true;
	}
	
	// My main motto for the next 2 methods is: if it works - it works (ツ)
	private async UniTaskVoid FlySequence(Vector3 strafePos, CancellationToken cancellationToken)
	{
		await UniTask.WaitForSeconds(3f, cancellationToken: cancellationToken);
		
		// Play engine sound
		engineSource.clip = GetRandomClip(engineSounds);
		engineSource.outputAudioMixerGroup = _betterAudio.OutEnvironment;
		engineSource.Play();
		await UniTask.WaitForSeconds(1f, cancellationToken: cancellationToken);
		
		// Disable flares
		_flareCountermeasureInstance.SetActive(false);
		await UniTask.WaitForSeconds(3f, cancellationToken: cancellationToken);
		
		// Enable gun particles
		gau8Particles.SetActive(true);
		// Play jet firing voiceover
		_fireSupportAudio.PlayVoiceover(EVoiceoverType.JetFiring);
		await UniTask.WaitForSeconds(1f, cancellationToken: cancellationToken);
		
		// Fire GAU8
		Gau8Sequence(strafePos, cancellationToken).Forget();
		await UniTask.WaitForSeconds(2f, cancellationToken: cancellationToken);
		
		if (!PlayerHelper.IsMainPlayerAlive())
		{
			return;
		}
		
		// Play explosion sfx
		// TODO: This should be the sfx for the actual projectile instead of manually being played here
		_betterAudio.PlayAtPoint(
			strafePos,
			GetRandomClip(gau8ExpSounds),
			Vector3.Distance(_player.CameraPosition.position, strafePos),
			BetterAudio.AudioSourceGroupType.Gunshots,
			1200,
			1,
			EOcclusionTest.Regular
		);
		gau8Particles.SetActive(false);
		await UniTask.WaitForSeconds(3.5f, cancellationToken: cancellationToken);
		
		if (!PlayerHelper.IsMainPlayerAlive())
		{
			return;
		}
		
		// Play GAU8 BRRRT sfx
		_betterAudio.PlayAtPoint(
			gau8Transform.position - gau8Transform.forward * 100 - gau8Transform.up * 100,
			GetRandomClip(gau8Sound),
			Vector3.Distance(_player.CameraPosition.position, gau8Transform.position),
			BetterAudio.AudioSourceGroupType.Gunshots,
			3200,
			2
		);
		await UniTask.WaitForSeconds(1.5f, cancellationToken: cancellationToken);
		
		// Enable flares
		_flareCountermeasureInstance.SetActive(true);
		await UniTask.WaitForSeconds(8f, cancellationToken: cancellationToken);
		
		// Play jet leaving voiceover
		_fireSupportAudio.PlayVoiceover(EVoiceoverType.JetLeaving);
		await UniTask.WaitForSeconds(4f, cancellationToken: cancellationToken);
		
		// Play strafe over voiceover
		_fireSupportAudio.PlayVoiceover(EVoiceoverType.StationStrafeEnd);
		await UniTask.WaitForSeconds(4f, cancellationToken: cancellationToken);
		
		ReturnToPool();
	}
	
	private async UniTaskVoid Gau8Sequence(Vector3 strafePos, CancellationToken cancellationToken)
	{
		Vector3 gau8Pos = gau8Transform.position + gau8Transform.forward * 515;
		Vector3 gau8Dir = Vector3.Normalize(strafePos - gau8Pos);
		Vector3 gau8LeftDir = Vector3.Cross(gau8Dir, Vector3.up).normalized;
		
		var ammoCounter = 50;
		while (ammoCounter > 0)
		{
			if (!PlayerHelper.IsMainPlayerAlive())
			{
				break;
			}
			
			Vector3 leftRightSpread = gau8LeftDir * Random.Range(-0.007f, 0.007f);
			gau8Dir = Vector3.Normalize(gau8Dir + new Vector3(0, 0.00037f, 0));
			Vector3 projectileDir = Vector3.Normalize(gau8Dir + leftRightSpread);
			
			_weapon.FireProjectile(gau8Pos, projectileDir);
			ammoCounter--;
			await UniTask.WaitForSeconds(_weapon.timeBetweenShots, cancellationToken: cancellationToken);
		}
	}
	
	private static AudioClip GetRandomClip(AudioClip[] audioClips)
	{
		int randomIndex = Random.Range(0, audioClips.Length);
		return audioClips[randomIndex];
	}
}