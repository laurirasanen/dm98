﻿using Sandbox;

[Library( "dm_crossbow", Title = "Crossbow" )]
partial class Crossbow : BaseDmWeapon, IPlayerCamera, IPlayerInput
{ 
	public override string ViewModelPath => "weapons/rust_crossbow/v_rust_crossbow.vmdl";

	public override float PrimaryRate => 1;
	public override int Bucket => 3;

	[NetPredicted]
	public bool Zoomed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_crossbow/rust_crossbow.vmdl" );
	}

	public override void AttackPrimary()
	{
		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}

		ShootEffects();

		if ( IsServer )
		using ( Prediction.Off() )
		{
			var bolt = new CrossbowBolt();
			bolt.WorldPos = Owner.EyePos;
			bolt.WorldRot = Owner.EyeRot;
			bolt.Owner = Owner;
			bolt.Velocity = Owner.EyeRot.Forward * 100;
		}
	}

	public override void OnPlayerControlTick( Player player )
	{
		base.OnPlayerControlTick( player );

		Zoomed = Owner.Input.Down( InputButton.Attack2 );
	}

	public virtual void ModifyCamera( Camera cam )
	{
		if ( Zoomed )
		{
			cam.FieldOfView = 20;
		}
	}

	public virtual void BuildInput( ClientInput owner )
	{
		if ( Zoomed )
		{
			owner.ViewAngles = Angles.Lerp( owner.LastViewAngles, owner.ViewAngles, 0.2f );
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		if ( Owner == Player.Local )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );
		}

		ViewModelEntity?.SetAnimParam( "fire", true );
		CrosshairPanel?.OnEvent( "fire" );
	}
}
