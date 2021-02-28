
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Vitals : Panel
{
	public Label Health;
	public Label Armor;

	public Vitals()
	{
		Health = Add.Label( "100", "health" );
		Armor = Add.Label( "100", "armor" );
	}

	public override void Tick()
	{
		var player = Player.Local;
		if ( player == null ) return;

		Health.Text = $"{player.Health:n0}";
		Health.SetClass( "danger", player.Health < 40.0f );
		Armor.Text = $"{player.Armor:n0}";
		Armor.SetClass("danger", player.Armor < 20.0f);
	}
}