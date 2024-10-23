using Godot;
using System;

public partial class HitFiller : Node2D
{
	[Export] public Godot.Collections.Array<Polygon2D> Polygons { get; set; }

	public void Set(params int[] activeSpots)
	{
		foreach (Polygon2D p in Polygons)
		{
			p.Color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		}

		foreach (int i in activeSpots)
		{
			Polygons[ i ].Color = new Color(1.0f, 1.0f, 1.0f);
		}
	}
}
