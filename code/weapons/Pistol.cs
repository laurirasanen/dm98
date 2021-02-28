using Sandbox;


[ClassLibrary( "dm_pistol" )]
partial class Pistol : BaseDmWeapon
{ 
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 15.0f;
	public override float SecondaryRate => 1.0f;
	public override float ReloadTime => 3.0f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		AmmoClip = 10;
		AmmoReserve = 20;
	}

	public override bool CanPrimaryAttack( Player owner )
	{
		return base.CanPrimaryAttack( owner ) && owner.Input.Pressed( InputButton.Attack1 );
	}

	public override void AttackPrimary( Player owner )
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}


		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();

		//
		// Shoot the bullets
		//
		ShootBullet( 0.05f, 1.5f, 5.0f, 3.0f );

	}
}
