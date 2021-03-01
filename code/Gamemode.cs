using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Random = Sandbox.Random;
using Math = Sandbox.Math;

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

[ClassLibrary]
class DeathmatchGamemode : BaseGamemode
{
	public static DeathmatchGamemode Current { get; set; }

	[Replicate]
	public Phase Phase { get; set; }

	[Replicate]
	public int Round { get; set; }

	private double _timeToFreezeTime;
	private double _timeToEndRound;
	private double _timeToWarmup;
	private double _roundOverTime;
	private double _gameOverTime;

	public double TimeToEndRound => _timeToEndRound;

	public bool PlayerCanSpawn = (Phase == Phase.RoundFreezeTime || Phase == Phase.WaitingForPlayers || Phase == Phase.Warmup);

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

	protected List<Player> _players;
	public int PlayerCount => _players.Count;

	[Replicate]
	public int PlayersWaiting { get; set; } = 0;

	public static Color TeamColor( Team team )
	{
		switch ( team )
		{
			case Team.Cops:
				return Color.Blue;
			case Team.Robbers:
				return Color.Red;
			case Team.Spectator:
				return new Color( 0.7, 0.7, 0.7 );
			default:
				return Color.White;
		}
	}

	public static double TeamSpawnOffset( Team team )
	{
		switch ( team )
		{
			case Team.Cops:
				return 70;
			case Team.Robbers:
				return 70;
			case Team.Spectator:
				return 140;
			default:
				return 0;
		}
	}

	protected override void Initialize()
	{
		base.Initialize();

		Current = this;

		if ( Server )
		{
			Phase = Phase.WaitingForPlayers;
			Round = 0;
			RoundTimerSeconds = 0;
			PlayersWaiting = 0;

			_timeToWarmup = Time.Now + players_wait_time;

			_players = new List<Player>();
		}

		PreloadAssets();
	}

	// Ghetto asset preloading until we do this properly
	protected static void PreloadAssets()
	{
		foreach ( var taunt in BarrelControllable.Taunts )
		{
			Sound.Library.Get( taunt, false );
		}

		Sound.Library.Get( "Sounds/weapons/explode3.wav" );
		Sound.Library.Get( "Sounds/weapons/explode4.wav" );
		Sound.Library.Get( "Sounds/weapons/explode5.wav" );
		Sound.Library.Get( "Sounds/buttons/button17.wav" );
		Sound.Library.Get( "Sounds/buttons/button17.wav" );
		Sound.Library.Get( "Sounds/weapons/cguard/charging.wav" );
		Sound.Library.Get( "Sounds/Weapons/pistol/pistol_fire2.wav", false );
		SkeletalModel.Library.Get( "models/player/mossman.mdl", false );
		Model.Library.Get( "models/props_c17/oildrum001_explosive.mdl", false );
		Material.Library.Get( "Particles/spark.mat" );
		Material.Library.Get( "Particles/poof.mat" );
	}

	protected override void Tick()
	{
		base.Tick();

		if ( Authority )
		{
			ServerTick();
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
		PlayersWaiting = PlayerCount;

		bool allConnected = PlayerCount >= players_needed;

		if ( allConnected )
		{
			_timeToWarmup = Time.Now;
			SwitchPhase( Phase.Warmup );
		}
		else
		{
			if ( Time.Now > _timeToWarmup )
			{
				// TODO:
				// Ran out of waiting time, cancel match?
				SwitchPhase( Phase.Warmup );
			}
		}
	}

	protected void TickWarmup()
	{
		if ( Time.Now < _timeToWarmup )
		{
			return;
		}

		Hud.Current.BroadcastMessage( $"Players connected, game starting in {warmup_time} seconds" );

		_timeToFreezeTime = Time.Now + warmup_time;
		SwitchPhase( Phase.RoundFreezeTime );
	}

	protected void TickRoundFreezeTime()
	{
		if ( Time.Now < _timeToFreezeTime )
		{
			return;
		}

		Round++;

		BroadcastRoundStarted();

		_timeToEndRound = Time.Now + round_time;
		SwitchPhase( Phase.RoundActive );
	}

	protected void TickRoundActive()
	{
		RoundTimerSeconds = Math.RoundToInt( _timeToEndRound - Time.Now );

		var roundActive = Time.Now < _timeToEndRound;

		if ( !roundActive )
		{
			Hud.Current.BroadcastMessage( "Cops win!" );
		}
		else
		{
			var copsLeft = Enumerable.Count(_players.Where(x => x.Team == (int)Team.Cops));
			var robbersLeft = Enumerable.Count(_players.Where(x => x.Team == (int)Team.Robbers));

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

		_roundOverTime = Time.Now + round_over_time;
		SwitchPhase( Phase.RoundOver );
	}

	protected void TickRoundOver()
	{
		if ( Time.Now < _roundOverTime )
		{
			return;
		}

		foreach ( var player in _players )
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
			_gameOverTime = Time.Now + game_over_time;
			SwitchPhase( Phase.GameOver );
		}
	}

	protected void TickGameOver()
	{
		// TODO: show scoreboard, etc..

		if ( Time.Now > _gameOverTime )
		{
			// TODO: next map
			LoadMap( "cr_bank" );
		}
	}

	protected void SpawnDeadPlayers()
	{
		if ( !Authority )
			return;

		foreach ( var player in _players )
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
		_timeToEndRound = Time.Now + round_time;

		Hud.Current.Chatbox?.AddMessage( "", $"Round has started! ({round_time}) second round", Color.White );
	}

	[Multicast]
	protected void BroadcastRoundOver()
	{
		World.PlaySound2D( "Sounds/ambient/alarms/klaxon1.wav" );
	}

	public override Controllable CreateControllable( Player player )
	{
		switch ( ( Team )player.Team )
		{
			case Team.Spectator:
				return new SpectatorControllable();
			case Team.Cops:
				return new BarrelControllable();
			case Team.Robbers:
				return new HumanControllable();
			default:
				return null;
		}
	}

	public override void LoadMap( string name )
	{
		DefaultPostProcess.MotionBlurAmount = 0;

		base.LoadMap( name );
	}

	public override void OnPlayerJoined( Player player )
	{
		var cops = Enumerable.Count(_players.Where(x => x.Team == (int)Team.Cops));
		var robbers = Enumerable.Count(_players.Where(x => x.Team == (int)Team.Robbers));
		if ( cops < robbers )
		{
			player.Team = ( int )Team.Cops;
		}
		else
		{
			player.Team = ( int )Team.Robbers;
		}

		_players.Add( player );

		Hud.Current.BroadcastMessage( $"{player.Name} joined the game" );

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

	public override void OnPlayerLeave( Player player )
	{
		base.OnPlayerLeave( player );

		_players.Remove( player );

		Hud.Current.BroadcastMessage( $"{player.Name} left the game" );
	}

	public override void OnPlayerDied( Player player, Controllable controllable )
	{
		if ( player == null )
		{
			return;
		}

		Hud.Current.BroadcastMessage( $"{player.Name} has died" );

		if ( Authority )
		{
			var position = controllable.Position;
			var eyeAngles = controllable.EyeAngles;

			var deathCamera = new DeathCamera();
			deathCamera.Spawn();
			player.Controlling = deathCamera;

			deathCamera.Position = position;
			deathCamera.Teleport( position );

			deathCamera.EyeAngles = eyeAngles;
			deathCamera.ClientEyeAngles = eyeAngles;

			if ( Phase == Phase.Warmup || Phase == Phase.WaitingForPlayers )
			{
				RespawnPlayerLater( player, deathCamera, 3.0 );
			}
			else
			{
				// TODO: team spec cam
			}
		}
	}

	public override void OnPlayerMessage( string playerName, int team, string message )
	{
		var color = TeamColor((Team)team);
		Hud.Current?.Chatbox?.AddMessage( playerName, message, color );
	}

	public override bool AllowPlayerMessage( Player sender, Player receiver, string message )
	{
		return true;
	}

	public override void RespawnPlayer( Player player )
	{
		Log.Assert( Authority );

		if ( player.Controlling is DeathCamera deathCamera &&
			deathCamera.IsValid )
		{
			deathCamera.ClientClearTarget();
		}

		player.Controlling?.Destroy();

		var controllable = CreateControllable(player);
		controllable.Spawn();
		player.Controlling = controllable;

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

		controllable.OnRespawned();
	}

	async void RespawnPlayerLater( Player player, DeathCamera deathCamera, double delay )
	{
		await Delay( TimeSpan.FromSeconds( delay ) );

		while ( deathCamera.IsValid && !deathCamera.WantsToRespawn )
		{
			await Task.Yield();
		}

		if ( Phase != Phase.WaitingForPlayers || Phase.Warmup )
		{
			return;
		}

		if ( deathCamera != null && deathCamera.IsValid )
		{
			deathCamera.ClientClearTarget();
			deathCamera.Destroy();
		}

		RespawnPlayer( player );
	}

	public override void OnLocalInput()
	{
		base.OnLocalInput();
	}
}
