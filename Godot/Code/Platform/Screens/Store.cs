using Deuteros.Code.Platform.Base;
using Deuteros.Code.Objects;
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Objects.Interfaces;
using System.Resources;

namespace Deuteros.Code.Platform.Screens
{
	public partial class Store : BaseSubScene
	{
		public List<StoreButton> Buttons { get; set; }
		public StoreButton SelectedButton { get; set; }
		public Button SwitchStoreType { get; set; }
		public bool ViewTypeToggle { get; set; }
		public Texture2D BlueArrow { get; set; }
		public RichTextLabel ResourceListLabel { get; set; }
		public Label BuildAmountLabel { get; set; }
		public GridContainer StoreButtonsNode { get; set; }

		public override void _Ready()
		{
			SwitchStoreType = (Button)GetNode("SwitchStoreImage/SwitchStoreType");
			SwitchStoreType.Connect("button_up", new Callable(this, nameof(SwitchStoreType_ButtonUp)));

			ResourceListLabel = (RichTextLabel)GetNode("ResourceList");
			BuildAmountLabel = (Label)GetNode("Recipe");

			BlueArrow = (Texture2D)ResourceLoader.Load("res://Sprites/Buttons/BlueArrowRight.fw.png");

			SelectedButton = null;

			StoreButtonsNode = GetNode<GridContainer>("StoreButtons");

			RefreshButtons();

			DrawData();

			base._Ready();
		}

		private void StoreButton_Clicked(int index)
		{
			var clickedButton = Buttons.Single(T => T.ObjectData != null && T.ObjectData.Research.ResearchOrder == index);

			if (!clickedButton.ObjectData.Locked)
			{
				if (SelectedButton != null)
					SelectedButton.Selected = false;

				SelectedButton = clickedButton;
				SelectedButton.Selected = true;

				DrawData();
			}
		}

		private void SwitchStoreType_ButtonUp()
		{
			ViewTypeToggle = !ViewTypeToggle;
			RefreshButtons();

			DrawData();
		}

		protected override void ResearchFinished(Objects.ResearchItem researchItem)
		{
			Item selectedItem = null;

			if (SelectedButton != null)
				selectedItem = SelectedButton.ObjectData;

			RefreshButtons();

			if (selectedItem != null)
			{
				SelectedButton = Buttons.Single(T => T.ObjectData != null && T.ObjectData.ItemType == selectedItem.ItemType);
				SelectedButton.Selected = true;
			}

			DrawData();
		}

		public void RefreshButtons()
		{
			Buttons = Utility.Buttons.CreateButtons<StoreButton, Item>(StoreButtonsNode,
				GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && !ViewTypeToggle && T.Research.Researched && !T.AutoProduce).Select(T => T).OrderBy(T => T.Research.ResearchOrder).ToDictionary(obj => obj.Research.ResearchOrder),
				this,
				nameof(StoreButton_Clicked),
				"/Code/Platform/StoreButton.cs",
				"StoreButton");
		}

		// Called every update.
		public override void _Draw()
		{
		}

		protected override void DayTick(uint previousDay, uint currentDay)
		{
			var currentPlanet = Deuteros.Code.GameCore.SingletonInstance.GetCurrentPlanet();
			
			if (ViewTypeToggle && currentPlanet.Station.MtxInstalled && !GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Mass_Tranceiver))
			{
				GameCore.SingletonInstance.TriggerAlienTechDiscovery(Enums.ItemTypes.m__t__x);
			}

			DrawData();
		}

		public void DrawData()
		{
			var currentPlanet = Deuteros.Code.GameCore.SingletonInstance.GetCurrentPlanet();

			ResourceListLabel.Text = "";

			var currentStore = SceneVariables.Contains(Enums.SceneVariables.Ground) ? currentPlanet.PlanetResources.Stores : currentPlanet.Station.Resources.Stores;

			int padlength = 1;

			foreach (var item in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => !T.Locked && (T.ItemCategory == Enums.ItemCategory.item) == ViewTypeToggle))
			{
				padlength = Math.Max(padlength, currentStore[item.ItemType].ToString().Length);
			}

			foreach (var item in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => !T.Locked && (T.ItemCategory == Enums.ItemCategory.item) == ViewTypeToggle))
			{
				if (SelectedButton != null)
				{
					var recipeItem = SelectedButton.ObjectData as Item;
					if (recipeItem.BuildRequirements.Count(T => T.ItemType == item.ItemType)>0 )
					{
						var material = recipeItem.BuildRequirements.First(T => T.ItemType == item.ItemType);
						if (material.ItemCount <= currentStore[material.ItemType])
						{
							ResourceListLabel.Text += (ViewTypeToggle ? "[color=#ffff00]" : "[color=#008800]") + currentStore[item.ItemType].ToString().PadLeft(padlength) + " [color=#ffffff]" + item.ShortName + "\n";
						}
						else
						{
							ResourceListLabel.Text += (ViewTypeToggle ? "[color=#ffff00]" : "[color=#ff0000]") + currentStore[item.ItemType].ToString().PadLeft(padlength) + " [color=#ffffff]" + item.ShortName + "\n";
						}
					}
					else
					{
						ResourceListLabel.Text += (ViewTypeToggle ? "[color=#ffff00]" : "[color=#556633]") + currentStore[item.ItemType].ToString().PadLeft(padlength) + " [color=#ffffff]" + item.ShortName + "\n";
					}
				}
				else
				{
					ResourceListLabel.Text += (ViewTypeToggle ? "[color=#ffff00]" : "[color=#556633]") + currentStore[item.ItemType].ToString().PadLeft(padlength) + " [color=#ffffff]" + item.ShortName + "\n";
				}
			}


			if (SelectedButton != null)
			{
				var recipeText = "Enough supplies for {0} {1}s";
				var maxCount = 200;
				var recipeItem = SelectedButton.ObjectData as Item;

				foreach (var material in recipeItem.BuildRequirements)
				{
					var maxProd = currentStore[material.ItemType] / material.ItemCount;
					if (maxProd < maxCount)
						maxCount = maxProd;
				}

				BuildAmountLabel.Text = string.Format(recipeText, maxCount, recipeItem.FullName);
			}
		}
	}
}
