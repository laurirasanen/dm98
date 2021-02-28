using Sandbox;
using Sandbox.UI;
using System.Linq;

public class ScoreboardEntry : Panel
{
	public PlayerInfo.Entry Entry;
	public Label PlayerName;
	public Label Kills;
	public Label Deaths;
	public Label Ping;

	public ScoreboardEntry( Panel parent )
	{
		Parent = parent;
		AddClass( "entry" );
		PlayerName = Add.Label( "Terry", "name" );
		Kills = Add.Label( "0", "kills" );
		Deaths = Add.Label( "0", "deaths" );
		Ping = Add.Label( "0", "ping" );
	}

	public virtual void UpdateFrom( PlayerInfo.Entry entry )
	{
		Entry = entry;

		PlayerName.Text = entry.GetString( "name" );
		Kills.Text = entry.Get<int>( "kills", 0 ).ToString();
		Deaths.Text = entry.Get<int>( "deaths", 0 ).ToString();
		Ping.Text = entry.Get<int>( "ping", 12 ).ToString();
	}
}
