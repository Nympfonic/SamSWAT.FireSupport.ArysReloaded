using Comfort.Common;
using Cysharp.Threading.Tasks;
using EFT;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Threading;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public class FireSupportSpotter : ScriptableObject
{
	[SerializeField] private GameObject[] spotterParticles;
	
	private bool _requestCancelled;
	
	private GameObject _inputManager;
	private Player _player;
	private LayerMask _layerMask;
	
	private ColliderReporter _colliderCheckerObj;
	private GameObject _spotterPositionObj;
	private GameObject _spotterRotationObj;
	private GameObject _spotterConfirmationObj;
	private Transform _spotterDirectionStartTransform;
	private Transform _spotterDirectionEndTransform;
	
	public static async UniTask<FireSupportSpotter> Load()
	{
		var instance = await AssetLoader.LoadAssetAsync<FireSupportSpotter>("assets/content/ui/firesupport_spotter.bundle");
		
		while (InputManagerUtil.GetInputManager() == null)
		{
			await UniTask.Yield();
		}
		
		instance.Initialize();
		
		return instance;
	}
	
	private void Initialize()
	{
		_inputManager = InputManagerUtil.GetInputManager().gameObject;
		_player = Singleton<GameWorld>.Instance.MainPlayer;
		//_layerMask = LayerMask.GetMask("Terrain", "LowPolyCollider");
		_layerMask = 1 << LayerMask.NameToLayer("Terrain") | 1 << LayerMask.NameToLayer("LowPolyCollider");
		
		_spotterPositionObj = Instantiate(spotterParticles[0]);
		_colliderCheckerObj = _spotterPositionObj.GetComponentInChildren<ColliderReporter>();
		_spotterPositionObj.SetActive(false);
		
		_spotterRotationObj = Instantiate(spotterParticles[1]);
		_spotterDirectionStartTransform = _spotterRotationObj.transform.Find("Spotter Arrow Core (6)");
		_spotterDirectionEndTransform = _spotterRotationObj.transform.Find("Spotter Arrow Core (1)");
		_spotterRotationObj.SetActive(false);
		
		_spotterConfirmationObj = Instantiate(spotterParticles[2]);
		_spotterConfirmationObj.SetActive(false);
	}
	
	public async UniTask<Vector3> SetLocation(bool checkSpace, CancellationToken cancellationToken)
	{
		_requestCancelled = false;
		await UniTask.WaitForSeconds(0.1f, cancellationToken: cancellationToken);
		
		_spotterPositionObj.SetActive(true);
		
		while (!Input.GetMouseButtonDown(0))
		{
			cancellationToken.ThrowIfCancellationRequested();
			
			if (IsRequestCancelled())
			{
				_requestCancelled = true;
				_spotterPositionObj.SetActive(false);
				FireSupportUI.Instance.SpotterNotice.SetActive(false);
				FireSupportUI.Instance.SpotterHeliNotice.SetActive(false);
				return Vector3.zero;
			}
			
			Transform cameraT = _player.CameraPosition;
			Physics.Raycast(
				cameraT.position + cameraT.forward,
				cameraT.forward,
				out RaycastHit hitInfo,
				500,
				_layerMask);
			FireSupportUI.Instance.SpotterNotice.SetActive(hitInfo.point.Equals(Vector3.zero));
			if (checkSpace && !hitInfo.point.Equals(Vector3.zero))
			{
				FireSupportUI.Instance.SpotterHeliNotice.SetActive(_colliderCheckerObj.HasCollision);
				
				if (_colliderCheckerObj.HasCollision)
				{
					Transform transform = _colliderCheckerObj.transform;
					transform.Rotate(Vector3.up, 5f);
				}
			}
			
			_spotterPositionObj.transform.position = hitInfo.point;
			await UniTask.NextFrame();
		}
		
		if (_spotterPositionObj.transform.position.Equals(Vector3.zero) || checkSpace && _colliderCheckerObj.HasCollision)
		{
			_requestCancelled = true;
			FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationDoesNotHear);
			FireSupportUI.Instance.SpotterNotice.SetActive(false);
			FireSupportUI.Instance.SpotterHeliNotice.SetActive(false);
		}
		
		_spotterPositionObj.SetActive(false);
		return _spotterPositionObj.transform.position;
	}
	
	public async UniTask<(Vector3 startPosition, Vector3 endPosition, Quaternion rotation)> SetSupportDirection(
		CancellationToken cancellationToken)
	{
		if (_requestCancelled)
		{
			return (Vector3.zero, Vector3.zero, Quaternion.identity);
		}
		
		await UniTask.WaitForSeconds(0.1f, cancellationToken: cancellationToken);

		_spotterRotationObj.transform.SetPositionAndRotation(_spotterPositionObj.transform.position, Quaternion.identity);
		_spotterRotationObj.SetActive(true);
		_inputManager.SetActive(false);
		
		while (!Input.GetMouseButtonDown(0))
		{
			cancellationToken.ThrowIfCancellationRequested();
			
			if (IsRequestCancelled())
			{
				_requestCancelled = true;
				_spotterRotationObj.SetActive(false);
				_inputManager.SetActive(true);
				return (Vector3.zero, Vector3.zero, Quaternion.identity);
			}
			
			float xAxisRotation = Input.GetAxis("Mouse X") * 5;
			_spotterRotationObj.transform.Rotate(Vector3.down, xAxisRotation);
			
			await UniTask.NextFrame();
		}
		
		_inputManager.SetActive(true);
		_spotterRotationObj.SetActive(false);
		return (
			_spotterDirectionStartTransform.position,
			_spotterDirectionEndTransform.position,
			_spotterRotationObj.transform.rotation);
	}
	
	public async UniTask ConfirmLocation(CancellationToken cancellationToken)
	{
		if (_requestCancelled)
		{
			return;
		}
		
		_spotterConfirmationObj.transform.SetPositionAndRotation(_spotterPositionObj.transform.position + Vector3.up,
			Quaternion.identity);
		_spotterConfirmationObj.SetActive(true);
		await UniTask.WaitForSeconds(0.8f, cancellationToken: cancellationToken);
		
		_spotterConfirmationObj.SetActive(false);
	}
	
	private bool IsRequestCancelled()
	{
		bool isCancelRequestInput = Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftAlt);
		bool hasRangefinder =
			_player != null &&
			_player.HandsController.Item?.TemplateId == ItemConstants.RANGEFINDER_TPL;
		
		return isCancelRequestInput || !hasRangefinder;
	}
}