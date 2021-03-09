using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

public partial class HitIndicator : Panel
{
	public static HitIndicator Current;

	public HitIndicator()
	{
		Current = this;
		StyleSheet = StyleSheet.FromFile( "/ui/HitIndicator.scss" );
	}

	TimeSince timeSinceHit;

	public override void Tick()
	{
		base.Tick();
		SetClass( "hit", timeSinceHit < 0.1f );
	}

	public void OnHit( Vector3 pos, float amount )
	{
		timeSinceHit = 0;

		var p = new HitPoint( amount, pos );
		p.Parent = this;
	}

	public class HitPoint : Panel
	{
		public Vector3 WorldPos;
		Vector3 Velocity;

		public HitPoint( float amount, Vector3 pos )
		{
			WorldPos = pos;
			Add.Label( $"-{amount}" );
			Velocity = (Vector3.Random + Vector3.Up ) * Rand.Float( 140, 160 );

			_ = Lifetime();
		}

		public override void Tick()
		{
			base.Tick();

			this.PositionAtWorld( WorldPos );

			WorldPos += Velocity * Time.Delta;

			Velocity += Vector3.Down * Time.Delta * 500.0f;
		}

		async Task Lifetime()
		{
			await Task.Delay( 1000 );
			Delete( true );
		}


	}
}


