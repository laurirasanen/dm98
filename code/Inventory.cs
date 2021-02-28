using Sandbox;
using System;
using System.Linq;

partial class DmInventory : BaseInventory
{
	public DmInventory( Player player ) : base ( player )
	{

	}

	public override bool Add( Entity ent, bool makeActive = false )
	{
		var weapon = ent as BaseDmWeapon;

		//
		// We don't want to pick up the same weapon twice
		// But we'll take the ammo from it Winky Face
		//
		if ( weapon != null && IsCarryingType( ent.GetType() ) )
		{
			var ammo = weapon.AmmoClip;

			if ( ammo > 0 )
			{
				Sound.FromWorld( "dm.pickup_ammo", ent.WorldPos );
			}

			ItemRespawn.Taken( ent ); 

			// Despawn it
			ent.Delete();
			return false;
		}

		if ( weapon != null )
		{
			Sound.FromWorld( "dm.pickup_weapon", ent.WorldPos );
		}

		ItemRespawn.Taken( ent );
		return base.Add( ent, makeActive );
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x.GetType() == t );
	}
}
