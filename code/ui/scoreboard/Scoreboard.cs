using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Scoreboard : Sandbox.UI.Scoreboard<Sandbox.UI.ScoreboardEntry>
{
	public Scoreboard()
	{
		StyleSheet = StyleSheet.FromFile( "/ui/scoreboard/Scoreboard.scss" );
	}
}
