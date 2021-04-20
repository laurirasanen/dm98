using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class BaseCarriable : IRespawnableEntity
{
	public virtual int Weight => 0;

	public virtual string ModelPath => "models/citizen_props/roadcone01.vdml";

	[NetPredicted]
	public TimeSince TimeSincePickup { get; set; }

	[NetPredicted]
	public TimeSince TimeSinceDrop { get; set; }

	public PickupTrigger PickupTrigger { get; protected set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );

		PickupTrigger = new PickupTrigger();
		PickupTrigger.Parent = this;
		PickupTrigger.WorldPos = WorldPos;
		TimeSinceDrop = 0;
	}

	public virtual void OnCarryStart( Entity carrier )
	{
		// TODO - check whats in BaseWeapon.OnCarryStart
		if ( PickupTrigger.IsValid() )
		{
			PickupTrigger.EnableTouch = false;
		}

		TimeSincePickup = 0;
	}

	public virtual void OnCarryDrop( Entity dropper )
	{
		if ( PickupTrigger.IsValid() )
		{
			PickupTrigger.EnableTouch = true;
		}

		TimeSinceDrop = 0;
	}
}
