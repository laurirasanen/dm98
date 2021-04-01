using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

enum Phase
{
	WaitingForPlayers,
	Warmup,
	RoundFreezeTime,
	RoundActive,
	RoundOver,
	GameOver,
}

enum Team : int
{
	Spectator,
	Cops,
	Robbers,
}

partial class GameRobbery : Game
{
	private float timeToFreezeTime;
	private float timeToEndRound;
	private float timeToWarmup;
	private float roundOverTime;
	private float gameOverTime;

	public float TimeToEndRound => timeToEndRound;
	public bool PlayerCanSpawn = (Phase == Phase.RoundFreezeTime || Phase == Phase.WaitingForPlayers || Phase == Phase.Warmup);

	[Replicate]
	public Phase Phase { get; set; }

	[Replicate]
	public int Round { get; set; }

	[Replicate]
	public int RoundTimerSeconds { get; set; }

	[ReplicatedVar( Help = "Duration of a round" )]
	public static float round_time { get; set; } = 120.0f;

	[ReplicatedVar( Help = "Duration of freeze time at round start" )]
	private static float round_freeze_time { get; set; } = 5.0f;

	[ReplicatedVar( Help = "Duration of buy time at round start" )]
	private static float round_buy_time { get; set; } = 15.0f;

	[ReplicatedVar( Help = "Duration of warmup time after all players have connected" )]
	private static float warmup_time { get; set; } = 30.0f;

	[ReplicatedVar( Help = "Duration of round over" )]
	private static float round_over_time { get; set; } = 5.0f;

	[ReplicatedVar( Help = "Duration of game over" )]
	private static float game_over_time { get; set; } = 15.0f;

	[ReplicatedVar( Help = "How long to wait for players to connect" )]
	private static float players_wait_time { get; set; } = 120.0f;

	[ReplicatedVar( Help = "How many players needed to start a round" )]
	public static int players_needed { get; set; } = 8;

	protected override void Init()
	{
		if ( IsServer )
		{
			Phase = Phase.WaitingForPlayers;
			Round = 0;
			RoundTimerSeconds = 0;
			timeToWarmup = Time.Now + players_wait_time;
		}
	}

	protected void ServerTick()
	{
		switch ( Phase )
		{
			case Phase.WaitingForPlayers:
				TickWaitingForPlayers();
				break;

			case Phase.Warmup:
				TickWarmup();
				break;

			case Phase.RoundFreezeTime:
				TickRoundFreezeTime();
				break;

			case Phase.RoundActive:
				TickRoundActive();
				break;

			case Phase.RoundOver:
				TickRoundOver();
				break;

			case Phase.GameOver:
				TickGameOver();
				break;

			default:
				throw new NotSupportedException();
		}
	}

	protected void SwitchPhase( Phase phase )
	{
		if ( Phase == phase )
			return;

		Phase = phase;

		switch ( Phase )
		{
			case Phase.RoundFreezeTime:
				SpawnDeadPlayers();
				break;

			default:
				break;
		}
	}

	protected void TickWaitingForPlayers()
	{
		bool allConnected = PlayerCount >= players_needed;

		if ( allConnected )
		{
			timeToWarmup = Time.Now;
			SwitchPhase( Phase.Warmup );
		}
		else
		{
			if ( Time.Now > timeToWarmup )
			{
				// TODO:
				// Ran out of waiting time, cancel match?
				SwitchPhase( Phase.Warmup );
			}
		}
	}

	protected void TickWarmup()
	{
		if ( Time.Now < timeToWarmup )
		{
			return;
		}

		Hud.Current.BroadcastMessage( $"Players connected, game starting in {warmup_time} seconds" );

		timeToFreezeTime = Time.Now + warmup_time;
		SwitchPhase( Phase.RoundFreezeTime );
	}

	protected void TickRoundFreezeTime()
	{
		if ( Time.Now < timeToFreezeTime )
		{
			return;
		}

		Round++;

		BroadcastRoundStarted();

		timeToEndRound = Time.Now + round_time;
		SwitchPhase( Phase.RoundActive );
	}

	protected void TickRoundActive()
	{
		RoundTimerSeconds = Math.RoundToInt( timeToEndRound - Time.Now );

		var roundActive = Time.Now < timeToEndRound;

		if ( !roundActive )
		{
			Hud.Current.BroadcastMessage( "Cops win!" );
		}
		else
		{
			var copsLeft = PlayerInfo.All.Count(x => x.Get<Team>("team") == Team.Cops);
			var robbersLeft = PlayerInfo.All.Count(x => x.Get<Team>("team") == Team.Robbers);

			if ( robbersLeft == 0 )
			{
				Hud.Current.BroadcastMessage( "Cops win!" );
			}
			else if ( copsLeft == 0 )
			{
				Hud.Current.BroadcastMessage( "Robbers win!" );
			}

			// TODO: check loot
		}

		BroadcastRoundOver();

		roundOverTime = Time.Now + round_over_time;
		SwitchPhase( Phase.RoundOver );
	}

	protected void TickRoundOver()
	{
		if ( Time.Now < roundOverTime )
		{
			return;
		}

		foreach ( var player in PlayerInfo.All )
		{
			if ( player == null )
				continue;

			if ( player.Controlling != null && player.Controlling.IsValid )
			{
				player.Controlling.Destroy();
			}

			RespawnPlayer( player );
		}

		if ( Round < MaxRounds )
		{
			SwitchPhase( Phase.RoundFreezeTime );
		}
		else
		{
			gameOverTime = Time.Now + game_over_time;
			SwitchPhase( Phase.GameOver );
		}
	}

	protected void TickGameOver()
	{
		// TODO: show scoreboard, etc..

		if ( Time.Now > gameOverTime )
		{
			// TODO: next map
			LoadMap( "cr_bank" );
		}
	}

	protected void SpawnDeadPlayers()
	{
		if ( !Authority )
			return;

		foreach ( var player in PlayerInfo.All )
		{
			if ( player == null )
				continue;

			if ( player.IsDead() && player.Team != Team.Spectator )
				RespawnPlayer( player );
		}
	}

	[Multicast]
	protected void BroadcastRoundStarted()
	{
		timeToEndRound = Time.Now + round_time;

		Hud.Current.Chatbox?.AddMessage( "", $"Round has started! ({round_time}) second round", Color.White );
	}

	[Multicast]
	protected void BroadcastRoundOver()
	{
		World.PlaySound2D( "Sounds/ambient/alarms/klaxon1.wav" );
	}

	public override void OnPlayerJoined( PlayerInfo.Entry player )
	{
		var cops = PlayerInfo.All.Count(x => x.Get<Team>("team") == Team.Cops);
		var robbers = PlayerInfo.All.Count(x => x.Get<Team>("team") == Team.Robbers);
		if ( cops < robbers )
		{
			player.Team = ( int )Team.Cops;
		}
		else
		{
			player.Team = ( int )Team.Robbers;
		}

		if ( Authority )
		{
			if ( PlayerCanSpawn )
			{
				RespawnPlayer( player );
			}
			else
			{
				// TODO: team-only spec
			}
		}
	}

	public override void RespawnPlayer( Player player )
	{
		Log.Assert( Authority );

		var spawnPoint = FindSpawnPoint();

		if ( spawnPoint != null )
		{
			var spawnangles = spawnPoint.Rotation.ToAngles();
			spawnangles = spawnangles.WithZ( 0 ).WithY( spawnangles.Y + 180.0f );

			var eyeAngles = Quaternion.FromAngles(spawnangles);

			var team = (Team)player.Team;
			var heightOffset = TeamSpawnOffset(team);

			var position = spawnPoint.Position + Vector3.Up * heightOffset;
			controllable.Position = position;
			controllable.Teleport( position );
			controllable.ClientLocation = position;
			controllable.EyeAngles = eyeAngles;
			controllable.ClientEyeAngles = eyeAngles;
		}
		else
		{
			Log.Warning( "Player {0} couldn't find spawn point", player );
		}
	}

	async void RespawnPlayerLater( Player player, DeathCamera deathCamera, double delay )
	{
		await Delay( TimeSpan.FromSeconds( delay ) );

		if ( Phase != Phase.WaitingForPlayers || Phase.Warmup )
		{
			return;
		}

		RespawnPlayer( player );
	}
}
