using Godot;
using System;

public partial class CharacterAnimation : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		float randomN = GD.Randf() * Mathf.Tau;
		((ShaderMaterial) Material).SetShaderParameter("startOffset", randomN);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        
    }

	public void SetColour(Color colour)
	{
        ((ShaderMaterial) Material).SetShaderParameter("colour", colour);
    }
}
