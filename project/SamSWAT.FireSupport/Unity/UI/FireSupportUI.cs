using Comfort.Common;
using Cysharp.Threading.Tasks;
using EFT;
using EFT.UI;
using EFT.UI.Gestures;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public class FireSupportUI : UpdatableComponentBase, IPointerEnterHandler, IPointerExitHandler
{
	public GameObject SpotterNotice;
	public GameObject SpotterHeliNotice;
	public Text timerText;
	
	[SerializeField] private FireSupportUIElement[] supportOptions;
	[SerializeField] private HoverTooltipArea tooltip;
	
	private Player _player;
	private float _menuOffset;
	
	private FireSupportServiceMappings _services;
	
	private bool _isUnderPointer;
	private readonly Color _enabledColor = new(1, 1, 1, 1);
	private readonly Color _disabledColor = new(1, 1, 1, 0.4f);

	public static FireSupportUI Instance { get; private set; }
	
	public event Action<ESupportType> SupportRequested;
	
	public static async UniTask<FireSupportUI> Load(
		FireSupportServiceMappings services,
		GesturesMenu gesturesMenu)
	{
		Instance = Instantiate(await AssetLoader.LoadAssetAsync("assets/content/ui/firesupport_ui.bundle")).GetComponent<FireSupportUI>();
		Instance.Initialize(services, gesturesMenu);
		return Instance;
	}
	
	public override void ManualUpdate()
	{
		bool rangefinderInHands = HasRangefinderEquipped();
		tooltip.SetUnlockStatus(rangefinderInHands);
		if (rangefinderInHands)
		{
			RenderUI();
			HandleInput();
		}
	}
	
	private void RenderUI()
	{
		if (_services == null || _services.Count == 0)
		{
			return;
		}
		
		foreach (IFireSupportService service in _services.Values)
		{
			RenderButton(service);
		}
	}
	
	private void RenderButton(IFireSupportService service)
	{
		FireSupportUIElement uiElement = supportOptions[(int)service.SupportType];
		
		if (service.IsRequestAvailable())
		{
			uiElement.AmountText.color = _enabledColor;
			uiElement.Icon.color = _enabledColor;
		}
		else
		{
			uiElement.IsUnderPointer = false;
			uiElement.AmountText.color = _disabledColor;
			uiElement.Icon.color = _disabledColor;
		}
		
		uiElement.AmountText.text = service.AvailableRequests.ToString();
	}
	
	private void HandleInput()
	{
		if (!_isUnderPointer)
		{
			return;
		}
		
		float angle = CalculateAngle();
		var selectedSupportOption = ESupportType.None;
		
		for (var i = 0; i < supportOptions.Length; i++)
		{
			FireSupportUIElement uiElement = supportOptions[i];
			
			if (_services.AnyAvailableRequests() &&
				angle > i * 45 &&
				angle < (i + 1) * 45)
			{
				uiElement.IsUnderPointer = true;
				selectedSupportOption = (ESupportType)i;
			}
			else
			{
				uiElement.IsUnderPointer = false;
			}
		}
		
		if (Input.GetMouseButtonDown(0))
		{
			SupportRequested?.Invoke(selectedSupportOption);
		}
	}
	
	private void Initialize(FireSupportServiceMappings services, GesturesMenu gesturesMenu)
	{
		_player = Singleton<GameWorld>.Instance.MainPlayer;
		_services = services;
		
		Transform fireSupportUiT = transform;
		fireSupportUiT.parent = gesturesMenu.transform;
		fireSupportUiT.localPosition = new Vector3(0, -255, 0);
		fireSupportUiT.localScale = new Vector3(1.4f, 1.4f, 1);
		_menuOffset = Screen.height / 2f - fireSupportUiT.position.y;
		
		Transform infoPanelTransform = SpotterNotice.transform.parent;
		infoPanelTransform.parent = Singleton<GameUI>.Instance.transform;
		infoPanelTransform.localPosition = new Vector3(0, -370f, 0);
		infoPanelTransform.localScale = Vector3.one;
		
		HasFinishedInitialization = true;
	}
	
	private bool HasRangefinderEquipped()
	{
		return _player.HandsController.Item?.TemplateId == ItemConstants.RANGEFINDER_TPL;
	}
	
	private float CalculateAngle()
	{
		Vector2 mouse;
		mouse.x = Input.mousePosition.x - (Screen.width / 2f);
		mouse.y = Input.mousePosition.y - (Screen.height / 2f) + _menuOffset;
		mouse.Normalize();
		
		if (mouse == Vector2.zero)
		{
			return 0;
		}
		
		float angle = Mathf.Atan2(mouse.y, -mouse.x) / Mathf.PI;
		angle *= 180;
		angle += 111;
		
		if (angle < 0)
		{
			angle += 360;
		}
		
		return angle;
	}
	
	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		_isUnderPointer = true;
	}
	
	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		_isUnderPointer = false;
		
		foreach (FireSupportUIElement uiElement in supportOptions)
		{
			uiElement.IsUnderPointer = false;
		}
	}
}