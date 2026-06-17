using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.GameData;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using System.Security;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;
using static System.Collections.Specialized.BitVector32;

public partial class Overview : BaseSubScene
{
	public const string SpriteBasePath = "res://Sprites//Buttons//Overview//";

	List<TextureButton> StationButtons = new List<TextureButton>();
	List<TextureButton> IOSButtons = new List<TextureButton>();
	List<TextureButton> SCGButtons = new List<TextureButton>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				var curIndex = i * 8 + j;
				StationButtons.Add(GetNode<TextureButton>("Stations/Col" + i.ToString() + "/Station0" + j.ToString()));
				StationButtons.Last().Pressed += () => Station_Pressed(curIndex);

				IOSButtons.Add(GetNode<TextureButton>("IOS/Col" + i.ToString() + "/IOS0" + j.ToString()));
				IOSButtons.Last().Pressed += () => IOS_Pressed(curIndex);

				SCGButtons.Add(GetNode<TextureButton>("SCG/Col" + i.ToString() + "/SCG0" + j.ToString()));
				SCGButtons.Last().Pressed += () => SCG_Pressed(curIndex);

			}
		}

		UpdateState();

		base._Ready();
	}

	private void Overview_Pressed()
	{
		throw new NotImplementedException();
	}

	private void Station_Pressed(int buttonPressed)
	{
		//var planet = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[(StellarBodies)Enum.Parse(typeof(StellarBodies),StationButtons[buttonPressed].GetMeta("planetid").ToString())];
        var planet = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[(StellarBodies)StationButtons[buttonPressed].GetMeta("planetid").AsInt32()];

        if (!planet.Station.Built) return;

		GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet = planet.PlanetId;

		var sceneVariables = new List<SceneVariables>();
		sceneVariables.Add(Enums.SceneVariables.Orbit);

		//Underscores in scene names represent a folder
		Deuteros.Code.GameCore.SingletonInstance.ChangeScene(Enums.Scenes.Station, sceneVariables);
	}

	private void IOS_Pressed(int buttonPressed)
	{
		//show ios

        GameCore.SingletonInstance.ShipSelected = Guid.Parse(IOSButtons[buttonPressed].GetMeta("shipid").ToString());
		GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Single<IShip>(s => s.ShipID == GameCore.SingletonInstance.ShipSelected).PlanetLocation;

        var sceneVariables = new List<SceneVariables>();
		sceneVariables.Add(Enums.SceneVariables.Orbit);

		//Underscores in scene names represent a folder
		Deuteros.Code.GameCore.SingletonInstance.ChangeScene(Enums.Scenes.ShipInterior, sceneVariables);
	}

	private void SCG_Pressed(int buttonPressed)
	{
        //show scg
        GameCore.SingletonInstance.ShipSelected = Guid.Parse(SCGButtons[buttonPressed].GetMeta("shipid").ToString());
		GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Single<IShip>(s => s.ShipID == GameCore.SingletonInstance.ShipSelected).PlanetLocation;


        //Underscores in scene names represent a folder
        Deuteros.Code.GameCore.SingletonInstance.ChangeScene(Enums.Scenes.ShipInterior, new List<SceneVariables>());
	}


	//Triggered from gamecore
	protected override void DayTick(uint previousDay, uint currentDay)
	{
		UpdateState();
	}
	
	private void UpdateState()
	{
		var stationList = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Select(p => p.Station).Where(s => s.BuildParts > 0).OrderBy(s => s.StationOrdinal).ToList();
		var iosList = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Where(T => T.ShipType == Ship_Types.IOS).ToList();
		var scgList = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Where(T => T.ShipType == Ship_Types.SCG).ToList();

		StationButtons.ForEach(T => T.Visible = false);
		IOSButtons.ForEach(T => T.Visible = false);
		SCGButtons.ForEach(T => T.Visible = false);

		var stationCount = 0;
		var iosCount = 0;
		var scgCount = 0;

		foreach (var station in stationList)
		{
			var curStationButton = StationButtons[((stationCount & 1) * 8)+(stationCount >> 1)];

			if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[station.PlanetId].ActiveMethanoid)
			{

				curStationButton.Visible = true;
				curStationButton.SetMeta("planetid",Variant.From<int>((Int32)station.PlanetId));


                if (!station.Built)
					curStationButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Station_UnderConstruction.png");
				else
					curStationButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Station_" + Math.Floor((decimal)(station.Factory.ProdCycle / 2)) + ".png");

				stationCount++;
			}
		}

		foreach (InterStellarShip ios in iosList)
		{
			var curIOSButton = IOSButtons[((iosCount & 1) * 8) + (iosCount >> 1)];

            curIOSButton.Visible = true;
            curIOSButton.SetMeta("shipid", ios.ShipID.ToString());

            if (ios.ShipState == Ship_States.Docking)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Docking.png");
			else if(ios.ShipState == Ship_States.Launching)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Launching.png");
			else if(ios.ShipState == Ship_States.InTransit && ios.StarLocation != ios.DestinationStarLocation)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_InTransit_InterStellar.png");
			else if (ios.ShipState == Ship_States.InTransit && ios.StarLocation == ios.DestinationStarLocation)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_InTransit.png");
			else if (ios.ShipState == Ship_States.Docked)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Docked.png");
			else if (ios.ShipState == Ship_States.UnDocked && ios.Scanning)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Scanning.png");
			else if (ios.ShipState == Ship_States.UnDocked && ios.Mining)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Mining.png");
			else if (ios.ShipState == Ship_States.UnDocked && ios.DFCC)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_UnDocked_DFCC.png");
			else if (ios.ShipState == Ship_States.UnDocked && !ios.DFCC)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_UnDocked.png");

			iosCount++;
		}

		foreach (InterStellarShip scg in scgList)
		{
			var curSCGButton = SCGButtons[((scgCount & 1) * 8) + (scgCount >> 1)];

            curSCGButton.Visible = true;
            curSCGButton.SetMeta("shipid", scg.ShipID.ToString());

            if (scg.ShipState == Ship_States.Docking)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Docking.png");
			else if (scg.ShipState == Ship_States.Launching)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Launching.png");
			else if (scg.ShipState == Ship_States.InTransit && scg.StarLocation != scg.DestinationStarLocation)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_InTransit_InterStellar.png");
			else if (scg.ShipState == Ship_States.InTransit && scg.StarLocation == scg.DestinationStarLocation)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_InTransit.png");
			else if (scg.ShipState == Ship_States.Docked)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Docked.png");
			else if (scg.ShipState == Ship_States.UnDocked && scg.Scanning)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Scanning.png");
			else if (scg.ShipState == Ship_States.UnDocked && scg.Mining)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Mining.png");
			else if (scg.ShipState == Ship_States.UnDocked && scg.DFCC)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_UnDocked_DFCC.png");
			else if (scg.ShipState == Ship_States.UnDocked && !scg.DFCC)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_UnDocked.png");

			scgCount++;
		}
	}
}
