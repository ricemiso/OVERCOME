using Godot;
using System;

public partial class Hud : CanvasLayer
{
	public bool IsPushGuard = false; //バリアが押されたか

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<TextureProgressBar>("HPBarFrame").Hide();
		GetNode<TextureProgressBar>("HPBar").Hide();
		GetNode<Popup>("Popup").Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var BarrierBar = GetNode<TextureProgressBar>("BarrierBar");
		BarrierBar.Value += 5 * delta;

		if (BarrierBar.Value >= 100){
			if(Input.IsActionJustPressed("Special") && !IsPushGuard){
				GetNode<Timer>("BarrierTimer").Start();
				IsPushGuard = true;
			}
		}

		var CannonBar = GetNode<TextureProgressBar>("SpecialBar");
		CannonBar.Value += 5 * delta; 

		if(Input.IsActionPressed("Pause")){
			PausePressed();
		}
	}

	//ゲージ量を取得
	public double BarrierGauge()
	{
		return GetNode<TextureProgressBar>("BarrierBar").Value;
	}

	//HPを取得
	public void GetHP(int value)
	{
		var HPBar = GetNode<TextureProgressBar>("HPBar");
		HPBar.Value = value;
		HPBar.Show();
	}

	//バリアが終わったとき
	private void _on_timer_timeout()
	{
		GetNode<TextureProgressBar>("BarrierBar").Value = 0;
		IsPushGuard = false;
	}

	//ポーズが押されたとき
	private void PausePressed()
	{
		GetTree().Paused = true;
		GetNode<Popup>("Popup").Show();
		GetNode<AudioStreamPlayer2D>("CancelSound").Play();
	}
}
