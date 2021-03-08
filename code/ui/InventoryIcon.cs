
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

class InventoryIcon : Panel
{
	public BaseDmWeapon Weapon;
	public Label Label;

	public InventoryIcon( BaseDmWeapon weapon )
	{
		Weapon = weapon;
		Label = Add.Label( weapon.ClassInfo.Title, "title" );
	}

	internal void TickSelection( BaseDmWeapon selectedWeapon )
	{
		SetClass( "active", selectedWeapon == Weapon );
	}
}
