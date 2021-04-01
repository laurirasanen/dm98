
using Sandbox;
using Sandbox.UI;

public partial class KillFeed : Panel
{
	public static KillFeed Current;

	public KillFeed()
	{
		Current = this;

		StyleSheet = StyleSheet.FromFile( "/ui/KillFeed.scss" );
	}

	[ClientRpc]
	public static void AddEntry( ulong lsteamid, string left, ulong rsteamid, string right, string method )
	{
		if ( Current == null )
			return;

		Log.Info( $"{left} killed {right} using {method}" );

		var e = Current.AddChild<KillFeedEntry>();

		e.AddClass( method );

		e.Left.Text = left;
		e.Left.SetClass( "me", lsteamid == ( Player.Local?.SteamId ) );

		e.Right.Text = right;
		e.Right.SetClass( "me", rsteamid == ( Player.Local?.SteamId ) );
	}

	public static void OnPlayerKilled( Player player )
	{
		if ( player.LastAttacker != null )
		{
			if ( player.LastAttacker is Player attackPlayer )
			{
				KillFeed.AddEntry( attackPlayer.SteamId, attackPlayer.Name, player.SteamId, player.Name, player.LastAttackerWeapon?.ClassInfo?.Name );
			}
			else
			{
				KillFeed.AddEntry( ( ulong )player.LastAttacker.NetworkIdent, player.LastAttacker.ToString(), player.SteamId, player.Name, "killed" );
			}
		}
		else
		{
			KillFeed.AddEntry( ( ulong )0, "", player.SteamId, player.Name, "died" );
		}
	}
}
