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

		if ( weapon == null )
		{
			return false;
		}

		// Drop existing weapon of same type if carrying
		DropType( ent.GetType() );

		// Pick up
		Sound.FromWorld( "dm.pickup_weapon", ent.WorldPos );
		ItemRespawn.Taken( ent );
		return base.Add( ent, makeActive );
	}

	public bool DropType( Type t )
	{
		var wpn = List.First( x => x.GetType() == t );
		if (!wpn) return false;

		base.Drop(wpn);
		return true;
	}
}
