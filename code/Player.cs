﻿using Sandbox;
using System;
using System.Linq;
using System.Numerics;

partial class DeathmatchPlayer : BasePlayer
{
	/// <summary>
	/// Number of deaths. We use the PlayerInfo system to store these variables because
	/// we want them to be available to all clients - not just the ones that can see us right now.
	/// </summary>
	public virtual int Deaths
	{
		get => GetPlayerInfo<int>( "deaths" );
		set => SetPlayerInfo( "deaths", value );
	}

	public virtual int Kills
	{
		get => GetPlayerInfo<int>( "kills" );
		set => SetPlayerInfo( "kills", value );
	}

	public virtual Team Team
	{
		get => GetPlayerInfo<int>( "team" );
		set => SetPlayerInfo( "team", value );
	}

	// TODO - how is Health defined in base?
	[Net]
	public float Armor { get; set; }


	TimeSince timeSinceDropped;

	public DeathmatchPlayer()
	{
		Inventory = new DmInventory( this );
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

		Dress();

		Inventory.Add( new Pistol(), true );
		//Inventory.Add( new Shotgun() );
		//Inventory.Add( new SMG() );
		//Inventory.Add( new Crossbow() );

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

		BecomeRagdollOnClient( LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );

		Controller = null;
		Camera = new SpectateRagdollCamera();

		EnableAllCollisions = false;
		EnableDrawing = false;
	}

	protected override void Tick()
	{
		base.Tick();

		//
		// Input requested a weapon switch
		//
		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}


		if ( LifeState != LifeState.Alive )
			return;

		TickPlayerUse();

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
				if ( dropped.PhysicsGroup != null )
				{
					dropped.PhysicsGroup.Velocity = Velocity + (EyeRot.Forward + EyeRot.Up) * 300;
				}

				timeSinceDropped = 0;
				SwitchToBestWeapon();
			}
		}
		//
		// If the current weapon is out of ammo and we last fired it over half a second ago
		// lets try to switch to a better wepaon
		//
		if ( ActiveChild is BaseDmWeapon weapon && !weapon.IsUsable() && weapon.TimeSincePrimaryAttack > 0.5f && weapon.TimeSinceSecondaryAttack > 0.5f )
		{
			SwitchToBestWeapon();
		}
	}

	public void SwitchToBestWeapon()
	{
		var best = Children.Select( x => x as BaseDmWeapon )
			.Where( x => x.IsValid() && x.IsUsable() )
			.OrderByDescending( x => x.BucketWeight )
			.FirstOrDefault();

		if ( best == null ) return;

		ActiveChild = best;
	}

	public override void StartTouch( Entity other )
	{
		// TODO - only avoid picking up the one we dropped
		if ( timeSinceDropped < 1 )
			return;

		base.StartTouch( other );
	}

	RealTimeSince timeSinceUpdatedFramerate;


	public override void PostCameraSetup( Camera camera )
	{
		base.PostCameraSetup( camera );

		if ( camera is FirstPersonCamera )
		{
			AddCameraEffects( camera );
		}

		if ( timeSinceUpdatedFramerate > 1 )
		{
			timeSinceUpdatedFramerate = 0;
			//UpdateFps( (int) (1.0f / Time.Delta) );
		}
	}

	float walkBob = 0;
	float lean = 0;
	float fov = 0;

	private void AddCameraEffects( Camera camera )
	{
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
		lean = lean.LerpTo( Velocity.Dot( camera.Rot.Right ) * 0.03f, Time.Delta * 15.0f );

		var appliedLean = lean;
		appliedLean += MathF.Sin( walkBob ) * speed * 0.2f;
		camera.Rot *= Rotation.From( 0, 0, appliedLean );

		speed = ( speed - 0.7f ).Clamp( 0, 1 ) * 3.0f;

		fov = fov.LerpTo( speed * 20 * MathF.Abs( forwardspeed ), Time.Delta * 2.0f );

		camera.FieldOfView += fov;

	//	var tx = new Sandbox.UI.PanelTransform();
	//	tx.AddRotation( 0, 0, lean * -0.1f );

	//	Hud.CurrentPanel.Style.Transform = tx;
	//	Hud.CurrentPanel.Style.Dirty();

	}

	DamageInfo LastDamage;

	public override void TakeDamage( DamageInfo info )
	{
		LastDamage = info;

		// hack - hitbox 0 is head
		// we should be able to get this from somewhere
		if ( info.HitboxIndex == 0 )
		{
			info.Damage *= 2.0f;
		}

		if ( Armor > float.Epsilon )
		{
			var armorPen = 0.5f; // TODO - add to weapons
			var reduction = info.Damage * ( 1.0f - armorPen );

			// Make armor less effective the more damaged it is
			var effectiveness = Armor / 100.0f;
			reduction *= effectiveness;

			Armor = Math.Max( 0, Armor - reduction );
			info.Damage -= reduction;
		}

		base.TakeDamage( info );

		if ( info.Attacker is DeathmatchPlayer attacker && attacker != this )
		{
			// Note - sending this only to the attacker!
			attacker.DidDamage( attacker, info.Position, info.Damage, ((float)Health).LerpInverse( 100, 0 ) );
		}

		TookDamage( this, info.Weapon.IsValid() ? info.Weapon.WorldPos : info.Attacker.WorldPos );
	}

	[ClientRpc]
	public void DidDamage( Vector3 pos, float amount, float healthinv )
	{
		Sound.FromScreen( "dm.ui_attacker" )
			.SetPitch( 1 + healthinv * 1 );

		HitIndicator.Current?.OnHit( pos, amount );
	}

	[ClientRpc]
	public void TookDamage( Vector3 pos )
	{
		//DebugOverlay.Sphere( pos, 5.0f, Color.Red, false, 50.0f );

		DamageIndicator.Current?.OnHit( pos );
	}
}
