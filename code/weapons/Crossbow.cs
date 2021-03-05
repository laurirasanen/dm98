using Sandbox;

[ClassLibrary( "dm_crossbow" )]
partial class Crossbow : BaseDmWeapon, IPlayerCamera, IPlayerInput
{ 
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 1;

	[NetPredicted]
	public bool Zoomed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override void AttackPrimary( Player owner )
	{
		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}

		if ( IsServer )
		using ( Prediction.Off() )
		{
			var bolt = new CrossbowBolt();
			bolt.Pos = owner.EyePos;
			bolt.Rot = owner.EyeRot;
			bolt.Owner = owner;
			bolt.Velocity = Owner.EyeRot.Forward * 100;
		}
	}

	public override void OnPlayerControlTick( Player owner )
	{
		base.OnPlayerControlTick( owner );

		Zoomed = owner.Input.Down( InputButton.Attack2 );
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
}
