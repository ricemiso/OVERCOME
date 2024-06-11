using Godot;
using System;
//using System.IO; 

//ステージ
public class StageData
{
	public int[] Enemies;
	public int	 Boss;
	
	public StageData(int[] enemies, int boss){
		Enemies = enemies;
		Boss = boss;
	}
}

//ギミック（障害物）
public class GimmicksData
{
	public int[] Gimmicks;

	public GimmicksData(int[] gimmicks){
		Gimmicks = gimmicks;
	}
}

public partial class Main : Node2D
{
	[Export]
	public PackedScene 		Enemy_1;
	[Export]
	public PackedScene 		Enemy_2;
	[Export]
	public PackedScene 		Enemy_3;
	[Export]
	public PackedScene 		Enemy_4;
	[Export]
	public PackedScene 		Enemy_5;
	[Export]
	public PackedScene 		Enemy_6;
	[Export]
	public PackedScene 		PoewrCube;
	[Export]
	public PackedScene 		Meteo;
	[Export]
	public PackedScene 		Anim_Player;
	[Export]
	public PackedScene 		Anim_Enemy_1;
	[Export]
	public PackedScene 		Anim_Enemy_2;
	[Export]
	public PackedScene 		Anim_Enemy_3;
	[Export]
	public PackedScene 		Anim_Enemy_4;
	[Export]
	public PackedScene 		Anim_Enemy_5;
	[Export]
	public PackedScene 		Anim_Enemy_6;
	[Export]
	public PackedScene 		Smoke;
	
	public PackedScene[] 	Enemies;			//敵の配列
	
	public StageData[]		Stages;  			//ステージの配列
	
	public PackedScene[] 	Gimmicks;			//ギミックの配列

	public GimmicksData[]   GimmicksStages;		//ギミックステージの配列

	public int				StageNum;			//ステージ数
	public int			 	ScoreNum;			//スコア
	static int 			 	HighScore;			//ハイスコア
	public int				PlayerRest = 3;		//残機
	public bool			 	AnimTime = false;	//タイトルアニメーションを行うか
	public bool 		 	IsClear  = false;	//ゲームクリアしたか
	public bool          	IsDeath  = false;	//死んだか
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<AudioStreamPlayer2D>("BGM").Play();

		GetNode<Timer>("AnimTimer").Start();

		//敵の配列
		Enemies = new PackedScene[7]{
			Enemy_1,
			Enemy_2,
			Enemy_3,
			Enemy_4,
			Enemy_5,
			Enemy_6,
			PoewrCube,
		};

		//ステージ構成
		Stages = new StageData[7]{
			new StageData(new int[1] {0},   3),
			new StageData(new int[1] {1},    3),
			new StageData(new int[2] {0,1},    3),
			new StageData(new int[1] {2},    3),
			new StageData(new int[2] {0,4},    3),
			new StageData(new int[4] {0,2,1,4},    3),
			new StageData(new int[1] {0},    5),
		};

		Gimmicks = new PackedScene[1]{
			Meteo,
		};

		GimmicksStages = new GimmicksData[1]{
			new GimmicksData(new int [1] {0}),
		};

		GD.Randomize();

		//プレイヤー・特殊能力UI非表示
		GetPlayer().Hide();
		GetHUD().GetNode<TextureProgressBar>("BarrierBar").Hide();
		GetHUD().GetNode<TextureProgressBar>("BarrierBarFrame").Hide();
		GetHUD().GetNode<Label>("Barrier").Hide();
		GetHUD().GetNode<TextureProgressBar>("SpecialBar").Hide();
		GetHUD().GetNode<TextureProgressBar>("SpecialBarFrame").Hide();
		GetHUD().GetNode<Label>("Cannon").Hide();
		GetClear().Hide();

		FilePath();
	}

	//ゲームスタート時
	public void NewGame()
	{
		IsClear  = false;
		AnimTime = true;
		EraseAllEnemy();

		//プレイヤーを開始位置へ
		//プレイヤーを取得
		var Player = GetPlayer();
		//開始位置を取得
		var StartPosition = GetNode<Node2D>("StartPosition");

		//プレイヤーの座標を開始位置の座標にする
		Player.Position = StartPosition.Position;
		Player.Initialize();
		Player.Show();
		var Hp = Player.GetHP();

		Player.Special = false;
		Player.Guard = false;
		Player.PlayerLevel = 0;
		Player.PlayerRest = 3;
		Player.WideShoot = false;
		Player.WideShoot1 = false;
		Player.Accel = 1.0f;

		//メッセージの表示
		var Title = GetTitle();
		Title.ShowMessage("Get Ready!");
		Title.ShowScore("Score:");
		Title.ShowStageLabel("Stage:");
		
		//敵の発生タイマーを開始
		GetNode<Timer>("EnemyTimer").Start();
		GetNode<Timer>("BossTimer").Start();
		GetNode<Timer>("GimmicksTimer").Start();

		//残機数の初期化
		var PlayerRest = GetPlayer().GetPlayerRest();
		GetTitle().UpdatePlayerRest(PlayerRest);

		//ステージの初期化
		StageNum = 1;
		GetTitle().UpdateStageNum(StageNum);
		
		//スコアの初期化
		ScoreNum = 0;
		GetTitle().UpdateScoreNum(ScoreNum);

		//残機数の初期化
		PlayerRest = 3;
		GetTitle().UpdatePlayerRest(PlayerRest);
		
		var PlayerLevel = GetPlayer().GetPlayerLevel();

		//UI表示・非表示
		var Hud = GetHUD();
		Hud.GetNode<TextureProgressBar>("HPBarFrame").Show();
		Hud.GetNode<TextureProgressBar>("HPBar").Show();
		Hud.GetNode<TextureProgressBar>("BarrierBar").Hide();
		Hud.GetNode<TextureProgressBar>("BarrierBarFrame").Hide();
		Hud.GetNode<Label>("Barrier").Hide();
		Hud.GetNode<Label>("Cannon").Hide();
		Hud.GetNode<TextureProgressBar>("SpecialBar").Hide();
		Hud.GetNode<TextureProgressBar>("SpecialBarFrame").Hide();
		Hud.Show();
		Hud.GetHP(Hp);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){}

	//プレイヤーを取得
	public Player GetPlayer()
	{
		return GetNode<Player>("Player");
	}

	//HUDシーンを取得
	public Hud GetHUD()
	{
		return GetNode<Hud>("HUD");
	}

	//タイトルシーンを取得
	public Title GetTitle()
	{
		return GetNode<Title>("Title");
	}
	
	//ゲームクリアシーンを取得
	public Game_clear GetClear()
	{
		return GetNode<Game_clear>("GameClear");
	}

	//雑魚敵を生成
	public Enemy CreateEnemy(int kind, Vector2 pos)
	{
		if(kind < 0 || kind >= Enemies.Length){
			return null;
		}
		
		var Enemy = (Enemy)Enemies[kind].Instantiate();
		
		if(kind  == 4) {
			pos.X = -100.0f;
		}
		Enemy.Position = pos;
		AddChild(Enemy);
		
		return Enemy;
	}

	//敵生成タイマー
	private void _on_enemy_timer_timeout()
	{
		
		var StageData = Stages[StageNum - 1];
		int Len = StageData.Enemies.Length;
		
		//敵の種類を乱数で選択する
		int Kind = StageData.Enemies[GD.RandRange(0,Len - 1)];
		
		//指定したはんいの乱数を発生させて位置を計算する
		var Enemy = CreateEnemy(Kind ,new Vector2(1250,GD.RandRange(50,650)));
		Enemy.SetLevel(StageNum);
	}

	//障害物を生成
	public Meteo CreateGimmicks(int kind, Vector2 pos)
	{
		if(kind < 0 || kind >= Gimmicks.Length){
			return null;
		}

		var Meteo = (Meteo)Gimmicks[kind].Instantiate();
		Meteo.Position = pos;
		AddChild(Meteo);

		return Meteo;
	}

	//障害物生成タイマー
	private void _on_gimmicks_timer_timeout()
	{
		var GimmicksData = GimmicksStages[0];
		int len = GimmicksData.Gimmicks.Length;

		//敵の種類を乱数で選択する
		int kind = GimmicksData.Gimmicks[GD.RandRange(0,len - 1)];

		//指定したはんいの乱数を発生させて位置を計算する
		var Gimmick = CreateGimmicks(kind ,new Vector2(1250,GD.RandRange(50,650)));
		Gimmick.SetLevel(StageNum);
	}

	//プレイヤーリスポーン
	public void RestartPlayer()
	{
		var Player = GetPlayer();
		var StartPosition = GetNode<Node2D>("StartPosition");

		Player.Position = StartPosition.Position;
		IsDeath = true;
		Player.Initialize();
	}

	//スコア加算
	public void AddScore(int val)
	{
		ScoreNum += val;
		GetTitle().UpdateScoreNum(ScoreNum);
		
		if(ScoreNum>HighScore){
			HighScore = ScoreNum;
			GetTitle().UpdateHighScoreNum(ScoreNum);
		}
	}

	//ゲームオーバー処理
	public void GameOver()
	{
		GetNode<AudioStreamPlayer2D>("BossBGM").Stop();
		GetNode<AudioStreamPlayer2D>("BGM").Play();
		HideUI();

		//敵の発生を止める
		GetNode<Timer>("EnemyTimer").Stop();
		GetNode<Timer>("GimmicksTimer").Stop();

		//ゲームオーバー表示
		var Title = GetTitle();
		Title.ShowGameover();

		//プレイヤーを非表示
		GetPlayer().Hide();
		EraseAllEnemy();
		FileWrite();
	}

	//敵を全て消去
	public void EraseAllEnemy()
	{
		foreach(Node child in GetChildren()){
			if(child.IsInGroup("enemy")){
				child.QueueFree();
			}
		}
	}

	
	private void _on_boss_timer_timeout()
	{
		var StageData = Stages[StageNum - 1];
		var Kind = StageData.Boss;

		
		var Boss = CreateEnemy(Kind,new Vector2(1500,350));
		GetNode<Timer>("EnemyTimer").Stop();
		GetNode<Timer>("GimmicksTimer").Stop();

		
		Boss.Destroy += () => Input.StartJoyVibration(0,0,1,1);
		Boss.Destroy += () => AddScore(1000);
		Boss.Destroy += () => StageClear();
		Boss.Destroy += () => BombSound();

		if(StageNum != 7){
			Boss.Destroy += () => CreateEnemy(6,new Vector2(1250,GD.RandRange(50,650)));
		}
		Boss.SetLevel(StageNum);
	}

	//爆発音
	public void BombSound()
	{
		GetNode<AudioStreamPlayer2D>("ExplosionSound").Play();
	}

	//爆発
	public void Explode(Vector2 pos)
	{
		var Explosion = (Smoke)Smoke.Instantiate();
		Explosion.Position = pos;
		AddChild(Explosion);
	}

	//ステージクリア処理
	 public void StageClear()
	{
		StageNum++;

		if(StageNum == 7) {
			GetNode<AudioStreamPlayer2D>("BGM").Stop();
			GetNode<AudioStreamPlayer2D>("BossBGM").Play();
		}

		if(StageNum > 7) {
			EraseAllEnemy();
			GameClear();
			return;
		}

		var Title = GetTitle();
		Title.ShowMessage("Stage" + (StageNum - 1).ToString() 
			+ "\r\nClear!");

		if(StageNum > Stages.Length){
			StageNum = Stages.Length;
		}

		Title.UpdateStageNum(StageNum);
		GetNode<Timer>("EnemyTimer").Start();
		GetNode<Timer>("BossTimer").Start();
		GetNode<Timer>("GimmicksTimer").Start();
	}

	//ゲームクリア処理
	public void GameClear()
	{
		//残機数によってボーナススコア
		int Bounus = 500 * PlayerRest;
		AddScore(Bounus);

		GetNode<AudioStreamPlayer2D>("BossBGM").Stop();
		GetNode<AudioStreamPlayer2D>("ClearBGM").Play();

		HideUI();
		GetClear().Show();
		GetClear().GetNode<Timer>("CreditTimer").Start();
		GetClear().readScore(ScoreNum);
		GetPlayer().MoveVec = new Vector2(0.0f, 0.0f);
		IsClear = true;
		GetPlayer().GetNode<Timer>("ClearMoveTimer").Start();
		FileWrite();
		GetNode<Timer>("EnemyTimer").Stop();
		GetNode<Timer>("BossTimer").Stop();
		GetNode<Timer>("GimmicksTimer").Stop();

		EraseAllEnemy();
	}

	//クレジットスクロールが終わったとき
	async private void _on_game_clear_gone_credit()
	{
		GetClear().GetNode<Label>("Message").Show();

		var ClearTimer = GetNode<Timer>("ClearTimer");
		ClearTimer.Start();
		await ToSignal(ClearTimer, "timeout");

		await ToSignal(GetTree().CreateTimer(2), "timeout");

		GetClear().creditShow();
		GetClear().Hide();
		GetTitle().ShowTitleLabel();
		
		GetNode<AudioStreamPlayer2D>("ClearBGM").Stop();
		GetNode<AudioStreamPlayer2D>("BGM").Play();
	}

	//ClearTimerが終わったとき
	private void _on_clear_timer_timeout()
	{
		GetClear().GetNode<Label>("Message").Hide();
	}

	//UIを隠す
	public void HideUI()
	{
		GetHUD().Hide();
		GetTitle().GetNode<Label>("RestNumber").Hide();
		GetTitle().GetNode<Label>("Score").Hide();
		GetTitle().GetNode<Label>("ScoreNumber").Hide();
		GetTitle().GetNode<Label>("StageLabel").Hide();
		GetTitle().GetNode<Label>("StageNumber").Hide();
		}


	//ハイスコア読み込み
	public void FilePath()
	{
		string Path = "res://highscore.txt";
		using var File = FileAccess.Open(Path,FileAccess.ModeFlags.Read);
		string High = File.GetAsText();
		File.Close();
		HighScore = int.Parse(High);
		GetTitle().UpdateHighScoreNum(HighScore);
	}

	//ハイスコア書き込み
	public void FileWrite()
	{
		string Path = "res://highscore.txt";
		using var File = FileAccess.Open(Path,FileAccess.ModeFlags.Write);
		string Scr = HighScore.ToString();
		File.StoreString(Scr);
		File.Close();
	}

	//タイトルアニメーション
	public void AnimPlayer(Vector2 pos)
	{
		var Objx = (Anim_player)Anim_Player.Instantiate();
		Objx.Position = pos;
		AddChild(Objx);
	}

	private Vector2 AnimPlayerPos()
	{
		var Muzzle = GetNode<Node2D>("Anim_Player");
		var MuzzlePos = Muzzle.Position;
		return MuzzlePos;
	} 

	private Vector2 AnimPlayerPos2()
	{
		var Muzzle = GetNode<Node2D>("Anim_Player2");
		var MuzzlePos = Muzzle.Position;
		return MuzzlePos;
	} 

	private Vector2 AnimEnemy1Pos()
	{
		var Muzzle = GetNode<Node2D>("Anim_enemy_1");
		var MuzzlePos = Muzzle.Position;
		return MuzzlePos;
	} 

	public void AnimEnemy1(Vector2 pos)
	{
		var Objx = (Anim_enemy_1)Anim_Enemy_1.Instantiate();
		Objx.Position = pos;
		AddChild(Objx);
	}

	private Vector2 AnimEnemy2Pos()
	{
		var Muzzle = GetNode<Node2D>("Anim_enemy_2");
		var MuzzlePos = Muzzle.Position;
		return MuzzlePos;
	} 

	public void AnimEnemy2(Vector2 pos)
	{
		var Objx = (Anim_enemy_2)Anim_Enemy_2.Instantiate();
		Objx.Position = pos;
		AddChild(Objx);
	}

	private Vector2 AnimEnemy3Pos()
	{
		var Muzzle = GetNode<Node2D>("Anim_enemy_3");
		var MuzzlePos = Muzzle.Position;
		return MuzzlePos;
	} 

	public void AnimEnemy3(Vector2 pos)
	{
		var Objx = (Anim_enemy_3)Anim_Enemy_3.Instantiate();
		Objx.Position = pos;
		AddChild(Objx);
	}

	private Vector2 AnimEnemy4Pos()
	{
		var Muzzle = GetNode<Node2D>("Anim_enemy_4");
		var MuzzlePos = Muzzle.Position;
		return MuzzlePos;
	} 

	public void AnimEnemy4(Vector2 pos)
	{
		var Objx = (Anim_enemy_4)Anim_Enemy_4.Instantiate();
		Objx.Position = pos;
		AddChild(Objx);
	}

	private Vector2 AnimEnemy5Pos()
	{
		var Muzzle = GetNode<Node2D>("Anim_enemy_5");
		var MuzzlePos = Muzzle.Position;
		return MuzzlePos;
	} 

	public void AnimEnemy5(Vector2 pos)
	{
		var Objx = (Anim_enemy_5)Anim_Enemy_5.Instantiate();
		Objx.Position = pos;
		AddChild(Objx);
	}

	private Vector2 AnimEnemy6Pos()
	{
		var Muzzle = GetNode<Node2D>("Anim_enemy_6");
		var MuzzlePos = Muzzle.Position;
		return MuzzlePos ;
	} 

	public void AnimEnemy6(Vector2 pos)
	{
		var Objx = (Anim_enemy_6)Anim_Enemy_6.Instantiate();
		Objx.Position = pos;
		AddChild(Objx);
	}

	private void _on_anim_timer_timeout()
	{
		GetNode<Timer>("AnimTimer").Stop();
		if(!AnimTime){
			AnimPlayer(AnimPlayerPos());
		}
	}

	private void _on_anim_timer_2_timeout()
	{
		AnimEnemy6(AnimEnemy6Pos());
		AnimEnemy5(AnimEnemy5Pos());
		AnimEnemy4(AnimEnemy4Pos());
		AnimEnemy3(AnimEnemy3Pos());
		AnimEnemy2(AnimEnemy2Pos());
		AnimEnemy1(AnimEnemy1Pos());
		AnimPlayer(AnimPlayerPos2());
		GetNode<Timer>("AnimTimer2").Stop();
	}
}
