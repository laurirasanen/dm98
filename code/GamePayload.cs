﻿using Sandbox;

/// <summary>
/// This is the heart of the gamemode. It's responsible
/// for creating the player and stuff.
/// </summary>
[ClassLibrary( "payload", Title = "Payload" )]
partial class GamePayload : Game
{
	public GamePayload()
	{
		//
		// Create the HUD entity. This is always broadcast to all clients
		// and will create the UI panels clientside. It's accessible 
		// globally via Hud.Current, so we don't need to store it.
		//
		if ( IsServer )
		{
			new DeathmatchHud();
		}
	}

	/// <summary>
	/// Called when a player joins and wants a player entity. We create
	/// our own class so we can control what happens.
	/// </summary>
	public override Player CreatePlayer() => new DeathmatchPlayer();

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		ItemRespawn.Init();

		Init();
	}

	protected override void Tick()
	{
		base.Tick();

		if ( Authority )
		{
			ServerTick();
		}
	}
}