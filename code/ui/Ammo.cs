
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Ammo : Panel
{
	public Label Clip;
	public Label Reserve;

	public Ammo()
	{
		Clip = Add.Label( "0", "clip" );
		Reserve = Add.Label( "0", "reserve" );
	}

	public override void Tick()
	{
		var player = Player.Local;
		if ( player == null )
			return;

		var weapon = player.ActiveChild as BaseDmWeapon;
		SetClass( "active", weapon != null );

		if ( weapon == null )
			return;

		Clip.Text = $"{weapon.AmmoClip}";
		Clip.SetClass( "active", weapon.AmmoClip >= 0 );
		Clip.SetClass( "danger", weapon.AmmoClip <= weapon.ClipSize / 4 );

		Reserve.Text = $" / {weapon.AmmoReserve}";
		Reserve.SetClass( "active", weapon.AmmoReserve >= 0 );
		Reserve.SetClass( "danger", weapon.AmmoReserve < weapon.ClipSize );
	}
}