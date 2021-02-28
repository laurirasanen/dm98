using Sandbox;
using Sandbox.UI;
using System.Linq;

public class Scoreboard : Panel
{
	List<ScoreboardEntry> entries = new();

	public Scoreboard()
	{
		StyleSheet = StyleSheet.FromFile( "/ui/scoreboard/Scoreboard.scss" );
		AddClass( "scoreboard" );

		AddHeader();

		Canvas = Add.PanelWithClass( "canvas" );

		PlayerInfo.OnPlayerAdded += AddPlayer;
		PlayerInfo.OnPlayerUpdated += UpdatePlayer;
		PlayerInfo.OnPlayerRemoved += RemovePlayer;

		foreach ( var player in PlayerInfo.All )
		{
			AddPlayer( player );
		}
	}

	public void AddPlayer( PlayerInfo.Entry player )
	{
		var scoreboardEntry = new ScoreboardEntry( this );
		entries.add( scoreboardEntry );
		UpdatePlayer( player );
	}

	public void UpdatePlayer( PlayerInfo.Entry player )
	{
		var entry = entries.First(x => x.Entry == player);
		if ( !entry )
		{
			AddPlayer( player );
			return;
		}
		entry.UpdateFrom( player );
	}

	public void RemovePlayer( PlayerInfo.Entry player )
	{
		entries.Remove( x => x.Entry == player );
	}
}
