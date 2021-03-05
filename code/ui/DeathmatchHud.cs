
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

[ClassLibrary]
public partial class DeathmatchHud : Hud
{
	public DeathmatchHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet = StyleSheet.FromFile( "/ui/DeathmatchHud.scss" );

		//	RootPanel.AddChild<ChatUI>();

		RootPanel.AddChild<Vitals>();
		RootPanel.AddChild<Ammo>();

		//	health.BindToMethod( "text", () => Player.Local?.Health );

		RootPanel.AddChild<NameTags>();
		RootPanel.AddChild<CrosshairCanvas>();
		RootPanel.AddChild<InventoryBar>();

		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<Scoreboard>();
		//GameFeed = RootPanel.Add.PanelWithClass( "gamefeed" );
	}

	[ClientRpc]
	public void OnPlayerDied( string victim, string attacker = null )
	{
		Host.AssertClient();
		/*
		bool attackerIsMe = Player.Local != null && Player.Local.Name == attacker;
		bool victimIsMe = Player.Local != null && Player.Local.Name == victim;

		var panel = GameFeed.Add.PanelWithClass( "entry" );

		var a = panel.Add.Label( attacker, "attacker" );
		panel.Add.Label( "killed", "killed" );
		var v = panel.Add.Label( victim, "victim" );

		if ( attackerIsMe ) a.Class.Add( "me" );
		if ( victimIsMe ) v.Class.Add( "me" );

		panel.Class.Add( "old" );

		panel.WaitThen( 5.0f, () =>
		{
			panel.Class.Add( "deleted" );
			panel.DeleteAfterTransition();
		} );
		*/
	}

	[ClientRpc]
	public void ShowDeathScreen( string attackerName )
	{
		Host.AssertClient();

		Log.Info( "TODO SHOW DEATH SCREEN" );
	}
}
