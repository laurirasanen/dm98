using Sandbox;
using System;
using System.Linq;
using System.Numerics;

partial class DeathmatchPlayer : BasePlayer
{
	// TODO - how is Health defined in base?
	[Net]
	public float Armor { get; set; }

	/// <summary>
	/// Number of deaths. We use the PlayerInfo system to store these variables because
	/// we want them to be available to all clients - not just the ones that can see us right now.
	/// </summary>
	public virtual int Deaths
	{
		get => GetPlayerInfo<int>( "deaths" );
		set => SetPlayerInfo( "deaths", value );
	}

	/// <summary>
	/// Number of kills. We use the PlayerInfo system to store these variables because
	/// we want them to be available to all clients - not just the ones that can see us right now.
	/// </summary>
	public virtual int Kills
	{
		get => GetPlayerInfo<int>( "kills" );
		set => SetPlayerInfo( "kills", value );
	}

	TimeSince timeSinceDropped;

	public DeathmatchPlayer()
	{
		Inventory = new DmInventory( this );
		EnableClientsideAnimation = true;
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();
		Camera = new FirstPersonCamera();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableClientsideAnimation = true;

		Dress();

		Inventory.Add( new Pistol(), true );
		Inventory.Add( new Shotgun() );
		Inventory.Add( new SMG() );
		Inventory.Add( new Crossbow() );

		base.Respawn();
	}
	public override void OnKilled()
	{
		base.OnKilled();

		//
		Inventory.DropActive();

		//
		// Delete any items we didn't drop
		//
		Inventory.DeleteContents();

		BecomeRagdollOnClient();

		// TODO - clear decals

		Controller = null;
		Camera = new SpectateRagdollCamera();

		EnableAllCollisions = false;
		EnableDrawing = false;
	}

	public override void TakeDamage( float damage )
	{
		if ( Armor > float.Epsilon )
		{
			var armorPen = 0.5f; // TODO - add to weapons
			var reduction = damage * ( 1.0f - armorPen );

			// Make armor less effective the more damaged it is
			var effectiveness = Armor / 100.0f;
			reduction *= effectiveness;

			Armor = Math.Max( 0, Armor - reduction );
			damage -= reduction;
		}

		base.TakeDamage( damage );
	}

	protected override void Tick()
	{
		base.Tick();

		if ( Input.Pressed( InputButton.Slot1 ) )
			Inventory.SetActiveSlot( 0, true );
		if ( Input.Pressed( InputButton.Slot2 ) )
			Inventory.SetActiveSlot( 1, true );
		if ( Input.Pressed( InputButton.Slot3 ) )
			Inventory.SetActiveSlot( 2, true );
		if ( Input.Pressed( InputButton.Slot4 ) )
			Inventory.SetActiveSlot( 3, true );
		if ( Input.Pressed( InputButton.Slot5 ) )
			Inventory.SetActiveSlot( 4, true );
		if ( Input.Pressed( InputButton.Slot6 ) )
			Inventory.SetActiveSlot( 5, true );
		if ( Input.Pressed( InputButton.Slot7 ) )
			Inventory.SetActiveSlot( 6, true );
		if ( Input.Pressed( InputButton.Slot8 ) )
			Inventory.SetActiveSlot( 7, true );
		if ( Input.Pressed( InputButton.Slot9 ) )
			Inventory.SetActiveSlot( 8, true );

		if ( Input.MouseWheel != 0 )
			Inventory.SwitchActiveSlot( Input.MouseWheel, true );

		if ( LifeState != LifeState.Alive )
			return;

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( Camera is ThirdPersonCamera )
			{
				Camera = new FirstPersonCamera();
			}
			else
			{
				Camera = new ThirdPersonCamera();
			}
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				timeSinceDropped = 0;
			}
		}
	}


	public override void StartTouch( Entity other )
	{
		if ( IsClient )
			return;
		// TODO - only avoid picking up the one we dropped
		if ( timeSinceDropped < 1 )
			return;

		Inventory.Add( other, Inventory.Active == null );
	}

	float walkBob = 0;
	float lean = 0;
	float fov = 0;

	public override void PostCameraSetup( Camera camera )
	{
		base.PostCameraSetup( camera );

		var speed = Velocity.Length.LerpInverse( 0, 320 );
		var forwardspeed = Velocity.Normal.Dot( camera.Rot.Forward );

		var left = camera.Rot.Left;
		var up = camera.Rot.Up;

		if ( GroundEntity != null )
		{
			walkBob += Time.Delta * 25.0f * speed;
		}

		camera.Pos += up * MathF.Sin( walkBob ) * speed * 2;
		camera.Pos += left * MathF.Sin( walkBob * 0.6f ) * speed * 1;

		// Camera lean
		lean = lean.LerpTo( Velocity.Dot( camera.Rot.Right ) * 0.06f, Time.Delta * 15.0f );

		var appliedLean = lean;
		appliedLean += MathF.Sin( walkBob ) * speed * 0.2f;
		camera.Rot *= Rotation.From( 0, 0, appliedLean );

		speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

		fov = fov.LerpTo( speed * 20 * MathF.Abs( forwardspeed ), Time.Delta * 2.0f ); 

		camera.FieldOfView += fov;

	}
}
