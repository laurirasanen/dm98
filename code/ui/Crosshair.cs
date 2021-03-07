
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

public class Crosshair : Panel
{
	public Label Health;

	int fireCounter;

	public Crosshair()
	{
		StyleSheet = StyleSheet.FromFile( "/ui/Crosshair.scss" );

		for( int i=0; i<5; i++ )
		{
			var p = Add.Panel( "element" );
			p.AddClass( $"el{i}" );
		}
	}

	public override void Tick()
	{
		base.Tick();
		PositionFromLocalPlayer();

		SetClass( "fire", fireCounter > 0 );

		if ( fireCounter > 0 )
			fireCounter--;
	}

	void PositionFromLocalPlayer()
	{
		var player = Player.Local;
		if ( player.IsValid() )
		{
			PositionFrom( player.EyePos, player.EyeRot, player );
		}
	}

	Vector3 lastPos;

	private void PositionFrom( Vector3 eyePos, Rotation eyeRot, Entity source )
	{
		var tr = Trace.Ray( eyePos, eyePos + eyeRot.Forward * 1000 )
						.Size( 1.0f )
						.Ignore( source )
						.UseHitboxes()
						.Run();

		var screenpos = tr.EndPos.ToScreen();

		if ( lastPos == default )
			lastPos = screenpos;

		if ( screenpos.z < 0 )
			return;

		lastPos = Vector3.Lerp( lastPos, screenpos, Time.Delta * 1000.0f * lastPos.Distance( screenpos ) );

		Style.Left = Length.Fraction( lastPos.x );
		Style.Top = Length.Fraction( lastPos.y );
		Style.Dirty();
	}

	public override void OnEvent( string eventName )
	{
		if ( eventName == "fire" )
		{
			// this is a hack until we have animation or TriggerClass support
			fireCounter += 2;
		}

		base.OnEvent( eventName );
	}
}
