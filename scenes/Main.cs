using Godot;
using System;

public partial class Main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var testNumber = 10;
		var testArray = new string[] { "hi", "how", "are", "you" };
		TestMethod();
		GD.Print("Hello world!");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void TestMethod()
	{
		GD.Print("Enter test methoed");
	}
}
