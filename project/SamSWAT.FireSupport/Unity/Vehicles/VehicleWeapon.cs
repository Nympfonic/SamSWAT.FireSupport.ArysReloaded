using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using System;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public class VehicleWeapon
{
	private readonly string _playerProfileId;
	private readonly BallisticsCalculator _ballisticsCalculator;
	
	private readonly Weapon _weapon;
	private readonly AmmoItemClass _ammoItem;
	
	public VehicleWeapon(string playerProfileId, string weaponTpl, string ammoTpl)
	{
		GameWorld gameWorld = Singleton<GameWorld>.Instance;
		if (gameWorld == null)
		{
			throw new NullReferenceException("GameWorld is null");
		}
		
		_playerProfileId = playerProfileId;
		ItemFactoryClass itemFactory = Singleton<ItemFactoryClass>.Instance
			?? throw new NullReferenceException("ItemFactoryClass is null");
		_ballisticsCalculator = (BallisticsCalculator)gameWorld.SharedBallisticsCalculator;
		
		_weapon = (Weapon)itemFactory.CreateItem(MongoID.Generate(), weaponTpl, null);
		_ammoItem = (AmmoItemClass)itemFactory.CreateItem(MongoID.Generate(), ammoTpl, null);
	}
	
	public void FireProjectile(Vector3 origin, Vector3 direction)
	{
		// fireIndex seems to be related to player statistics - counting the number of shots player has fired
		// Leave fireIndex at 0 because we don't want vehicle weapon shots inflating player statistics
		EftBulletClass bullet = _ballisticsCalculator.CreateShot(_ammoItem, origin, direction, 0, _playerProfileId,
			_weapon, _weapon.SpeedFactor);
		_ballisticsCalculator.Shoot(bullet);
	}
}