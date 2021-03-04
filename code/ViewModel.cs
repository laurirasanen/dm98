using Sandbox;
using System;
using System.Linq;

partial class DmViewModel : BaseViewModel
{
	float walkBob = 0;
	Rotation lastRot = Rotation.Identity;

	public override void UpdateCamera( Camera camera )
	{
		base.UpdateCamera( camera );

		if ( lastRot == Rotation.Identity )
			lastRot = WorldRot;

		AddCameraEffects( camera );
	}

	private void AddCameraEffects( Camera camera )
	{
		//
		// If the angle of the viewmodel right now and the angle if wasnt to be is higher than 
		// allowance degrees then snap it to the edge of that circle.
		// If it's less, then slowly drift to the center
		//
		var angleDiff = Rotation.Difference( lastRot, WorldRot );
		var angleDiffDegrees = angleDiff.Angle();
		var allowance = 10.0f;

		if ( angleDiffDegrees > allowance )
		{
			// We could have a function that clamps a rotation to within x degrees of another rotation?
			lastRot = Rotation.Lerp( lastRot, WorldRot, 1.0f - (allowance / angleDiffDegrees) );
		}
		else
		{
			lastRot = Rotation.Lerp( lastRot, WorldRot, Time.Delta * 0.2f * angleDiffDegrees );
		}

		
		WorldRot = lastRot;

		//
		// Bob up and down based on our walk movement
		//
		var speed = Owner.Velocity.Length.LerpInverse( 0, 320 );
		var left = camera.Rot.Left;
		var up = camera.Rot.Up;

		if ( Owner.GroundEntity != null )
		{
			walkBob += Time.Delta * 25.0f * speed;
		}

		WorldPos += up * MathF.Sin( walkBob ) * speed * -1;
		WorldPos += left * MathF.Sin( walkBob * 0.6f ) * speed * -0.5f;

	}
}
