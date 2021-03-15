using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

public partial class DamageIndicator : Panel
{
	public static DamageIndicator Current;

	public DamageIndicator()
	{
		Current = this;
		StyleSheet = StyleSheet.FromFile( "/ui/DamageIndicator.scss" );
	}

	public void OnHit( Vector3 pos )
	{
		var p = new HitPoint( pos );
		p.Parent = this;
	}

	public class HitPoint : Panel
	{
		public Vector3 WorldPos;
		Vector3 Velocity;

		public HitPoint( Vector3 pos )
		{
			WorldPos = pos;

			_ = Lifetime();
		}

		public override void Tick()
		{
			base.Tick();

			var screenPos = WorldPos.ToScreen();

			// this is probably all wrong, it's based on the position on the screen
			// but I think the original hl1 one did kind of a top down view thing, so
			// at the top meant in front, at the back meant behind. We could probably make 
			// that mostly work the same here by rotating worldpos by our view pitch

			// TODO - we need screen width and height to make this proper accurate

			var angle = MathF.Atan2( 0.5f - screenPos.x, -1.0f * (0.5f - screenPos.y ) );

			var pt = new PanelTransform();

			pt.AddTranslateX( Length.Percent( -50.0f ) );
			pt.AddTranslateY( Length.Percent( -50.0f ) );
			pt.AddRotation( 0, 0, angle.RadianToDegree() );

			Style.Transform = pt;
			Style.Dirty();
		}

		async Task Lifetime()
		{
			await Task.Delay( 100 );
			AddClass( "dying" );
			await Task.Delay( 500 );
			Delete();
		}


	}
}


