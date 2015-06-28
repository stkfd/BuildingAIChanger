/** "Asset Prefab AI Changer"
 * a derivative of "BuildingAIChanger"
 * 
 * @author Stefan Kaufhold
 * @author D Lue Choy
 * 
 * Copyright (c) 2015, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License
 * as published by the Free Software Foundation; either 
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 * See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser 
 * General Public License along with this library.
 **/

using ICities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using ColossalFramework.UI;
using UnityEngine;

namespace AssetPrefabAIChanger
{
	/// <summary>
	/// Building AI Changer main mod class; provides Mod Info and handles loading
	/// </summary>
	public class PrefabAIChanger : LoadingExtensionBase, IUserMod
	{
		private SelectAIPanel m_selectAI;

		public string Name {
			get { return "Asset Prefab AI Changer"; }
		}

		public string Description {
			get { return "Allows you to change the Prefab AI in the Asset Editor. incl building, vehicle, citizen, net etc."; }
		}

		public override void OnLevelLoaded(LoadMode mode) {
			if (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset) {
				m_selectAI.Insert();
			}
		}
	}
	/// <summary>
	/// Building AI Changer main mod class; provides Mod Info and handles loading
	/// </summary>
	public class SelectAIPanel : UIPanel
	{
		private ToolController m_toolController;
		private PrefabInfo m_prefabinfo;

		public UIPanel m_propPanel;
		public UIPanel m_PrefabAIPanel;
		private UIDropDown m_SelectAIDropDown;
		private UICheckBox m_check;

		private bool m_omnious = false;

		public List<muAIInfo> m_AIInfo = null;

		public class muAIInfo {
			public Type typo; //type of Prefab AI
			public string fullname; //not an actual valid fullname, but an approximation used for sorting
		//could have relied on a recursive branch discorvery, but meh.
			public bool buildingAI = false;
			public bool citizenAI = false;
			public bool netAI = false;
			public bool vehicleAI = false;
		}


		public void Insert()
		{
			m_toolController = ToolsModifierControl.toolController;
			var view = UIView.GetAView();
			var uiContainer = view.FindUIComponent("FullScreenContainer");
			m_propPanel = uiContainer.Find<UIPanel>("DecorationProperties");
			var propPanelPanel = m_propPanel.Find<UIScrollablePanel>("Container");

			m_PrefabAIPanel = propPanelPanel.AddUIComponent<SelectAIPanel>();
			m_PrefabAIPanel.width = 393;
			m_PrefabAIPanel.height = 25;

			m_prefabinfo = m_toolController.m_editPrefabInfo;

			UILabel m_label = (UILabel) m_PrefabAIPanel.AddUIComponent<UILabel>();
			m_label.text = "PrefabAI";
			m_label.tooltip = "Determines the behaviour and availble properties of the prefab.";
			m_label.width = 181;
			m_label.height = 18;
			m_label.position = new Vector3(0.0f,-4.0f); //buggy invert from unknown source.
			m_label.autoSize = false;
			m_label.textColor = new Color32(125,185,255, 255);
			m_label.disabledTextColor = new Color32(255, 255, 255, 255);

			m_check = (UICheckBox) m_PrefabAIPanel.AddUIComponent<UICheckBox>();
			m_check.tooltip = "";
			m_check.width = 18;
			m_check.height = 18;
			m_check.position = new Vector3(190.0f,0.0f);

			UISprite m_check_uncheck = (UISprite) m_check.AddUIComponent<UISprite>();
		//UITextureAtlas "Ingame"
			m_check_uncheck.atlas = m_propPanel.Find<UISlicedSprite>("Caption").atlas;
			m_check_uncheck.spriteName = "check-unchecked";
			m_check_uncheck.width = 16;
			m_check_uncheck.height = 16;
			m_check_uncheck.position = new Vector3(0.0f,0.0f);

			UISprite m_check_check = (UISprite) m_check_uncheck.AddUIComponent<UISprite>();
			m_check_check.atlas = m_check_uncheck.atlas;
			m_check_check.spriteName = "check-checked";
			m_check_check.width = 16;
			m_check_check.height = 16;
			m_check_check.position = new Vector3(0.0f,0.0f);

			m_check.checkedBoxObject = m_check_check;

			m_SelectAIDropDown = (UIDropDown) m_PrefabAIPanel.AddUIComponent(typeof(UIDropDown));

			m_SelectAIDropDown.name = "SelectAIDropDown";
			m_SelectAIDropDown.size = new Vector2(171.0f, 20.0f);
			m_SelectAIDropDown.itemHeight = 25;
			m_SelectAIDropDown.itemHover = "ListItemHover";
			m_SelectAIDropDown.itemHighlight = "ListItemHighlight";
			m_SelectAIDropDown.normalBgSprite = "InfoDisplay";
			m_SelectAIDropDown.disabledBgSprite = "SubBarButtonBaseDisabled";
			m_SelectAIDropDown.listBackground = "InfoDisplay";//"GenericPanelLight";
			m_SelectAIDropDown.listWidth = 171;
			m_SelectAIDropDown.listHeight = 400;
			m_SelectAIDropDown.listPosition = ColossalFramework.UI.UIDropDown.PopupListPosition.Above;
			m_SelectAIDropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
			m_SelectAIDropDown.popupColor = new Color32(255, 255, 255, 255);
			m_SelectAIDropDown.popupTextColor = new Color32(12, 21, 22, 255);
			m_SelectAIDropDown.color = new Color32(255, 255, 255, 255);
			m_SelectAIDropDown.textColor = new Color32(12, 21, 22, 255);
			m_SelectAIDropDown.useOutline = true;
			m_SelectAIDropDown.outlineColor = new Color32(255, 255, 255, 64);
			m_SelectAIDropDown.bottomColor = new Color32(255, 255, 255, 255);
			m_SelectAIDropDown.verticalAlignment = UIVerticalAlignment.Middle;
			m_SelectAIDropDown.horizontalAlignment = UIHorizontalAlignment.Center;

			//build list of Prefab derived AI classes/types and then categorize them

			//get all subclasses of PrefabAI 
			m_AIInfo = new List<muAIInfo>();

			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
				Type[] assemblyTypes = a.GetTypes();
				for (int j = 0; j < assemblyTypes.Length; j++) {
					if (assemblyTypes[j].IsSubclassOf(typeof (PrefabAI))) {
						muAIInfo temp = new muAIInfo();
						temp.typo = assemblyTypes[j];
						temp.fullname = assemblyTypes[j].FullName;
						var derived = assemblyTypes[j];
						do { 
							derived = derived.BaseType;
							if (derived != null) {
//										temp.fullname.Insert(0,derived.FullName);
								temp.fullname = String.Concat(derived.FullName,".",temp.fullname);
							}
						} while (derived != null);
						if(assemblyTypes[j].IsSubclassOf(typeof (BuildingAI))) { temp.buildingAI = true; }
						if(assemblyTypes[j].IsSubclassOf(typeof (CitizenAI)))  { temp.citizenAI = true; }
						if(assemblyTypes[j].IsSubclassOf(typeof (NetAI)))      { temp.netAI = true; }
						if(assemblyTypes[j].IsSubclassOf(typeof (VehicleAI)))  { temp.vehicleAI = true; }
						m_AIInfo.Add(temp);
					}
				}
			}

			var tempo = new muAIInfo();
			tempo.typo = typeof (PrefabAI);
			tempo.fullname = tempo.typo.FullName;
			var derivedo = tempo.typo;
			do { 
				derivedo = derivedo.BaseType;
				if (derivedo != null) {
					tempo.fullname = String.Concat(derivedo.FullName,".",tempo.fullname);
				}
			} while (derivedo != null);
			m_AIInfo.Add(tempo);

//					Originally planned to sort/order by asset type. Deemed unnecessary.
			/*if(null != Singleton<ToolManager>.instance.m_properties.m_editPrefabInfo as BuildingInfo) {
				AIInfosort = m_AIInfo.OrderBy(s => s.buildingAI).ThenBy(s => s.citizenAI).ThenBy(s => s.netAI).ThenBy(s => s.vehicleAI).ThenBy(s => s.typo.FullName);
			} else if(null != Singleton<ToolManager>.instance.m_properties.m_editPrefabInfo as NetInfo) {
				AIInfosort = m_AIInfo.OrderBy(s => s.netAI).ThenBy(s => s.buildingAI).ThenBy(s => s.citizenAI).ThenBy(s => s.vehicleAI).ThenBy(s => s.typo.FullName);
			} else if(null != Singleton<ToolManager>.instance.m_properties.m_editPrefabInfo as VehicleInfo) {
				AIInfosort = m_AIInfo.OrderBy(s => s.vehicleAI).ThenBy(s => s.netAI).ThenBy(s => s.buildingAI).ThenBy(s => s.citizenAI).ThenBy(s => s.typo.FullName);
			} else if(null != Singleton<ToolManager>.instance.m_properties.m_editPrefabInfo as CitizenInfo) {
				AIInfosort = m_AIInfo.OrderBy(s => s.citizenAI).ThenBy(s => s.vehicleAI).ThenBy(s => s.netAI).ThenBy(s => s.buildingAI).ThenBy(s => s.typo.FullName);
			}*/
								var AIInfosort = m_AIInfo.OrderBy(s => s.fullname);

/*	//	might be useful for display purposes.
			int trimcomp = Math.Min(AIInfosort.First().fullname.Length,AIInfosort.Last().fullname.Length);
			int trindex = 0;
			for (int i = 0; i < trimcomp; i ++) {
				if (AIInfosort.First().fullname[i] != AIInfosort.Last().fullname[i]){
					trindex = i;
					i = trimcomp;
				}
			}
*/
			foreach(muAIInfo mew in AIInfosort) {
			/*
				string nextcat = string.Format("{0}{1}{2}{3}{4}",mew.fullname.Substring(trindex),mew.buildingAI?",b":"",mew.citizenAI?",c":"",mew.netAI?",n":"",mew.vehicleAI?",v":"");
				m_SelectAIDropDown.AddItem(nextcat);
			*/
				m_SelectAIDropDown.AddItem(mew.typo.FullName);
			}

//			m_SelectAIDropDown.AddItem("(null)"); //Default to PrefabAI because I am lazy.

			m_SelectAIDropDown.selectedIndex = 0; // Some how I can't help but feel that this is an invitation to disaster.

			m_SelectAIDropDown.position = new Vector3(210.0f, 0.0f);

			UIButton m_SelectAIDropDownButton = (UIButton) m_SelectAIDropDown.AddUIComponent<UIButton>();
			m_SelectAIDropDown.triggerButton = m_SelectAIDropDownButton;

			m_SelectAIDropDownButton.size = m_SelectAIDropDown.size;
			m_SelectAIDropDownButton.relativePosition = new Vector3(0.0f, 0.0f);
			m_SelectAIDropDownButton.textVerticalAlignment = UIVerticalAlignment.Middle;
			m_SelectAIDropDownButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
			m_SelectAIDropDownButton.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
			m_SelectAIDropDownButton.horizontalAlignment = UIHorizontalAlignment.Right;
			m_SelectAIDropDownButton.verticalAlignment = UIVerticalAlignment.Middle;

//		This does not realibly find the correct texture atlas on its own.
			m_SelectAIDropDownButton.normalFgSprite = "IconUpArrow";
			m_SelectAIDropDownButton.hoveredFgSprite = "IconUpArrowHovered";
			m_SelectAIDropDownButton.pressedFgSprite = "IconUpArrowPressed";
			m_SelectAIDropDownButton.focusedFgSprite = "IconUpArrowFocused";
			m_SelectAIDropDownButton.disabledFgSprite = "IconUpArrowDisabled";

//	so let's steal from one of the appropriate style. 
			var widthButton = m_propPanel.Find<UIPanel>("Size").Find<UIDropDown>("CellWidth").Find<UIButton>("Button");
			m_SelectAIDropDownButton.atlas = widthButton.atlas;
			m_SelectAIDropDownButton.normalFgSprite = widthButton.normalFgSprite;//"IconUpArrow";
//	It will get cloned and calibrated automatically when the UIListBox pops up anyways.
			m_SelectAIDropDown.listScrollbar = m_propPanel.Find<UIScrollbar>("Scrollbar");

			m_toolController.eventEditPrefabChanged += new ToolController.EditPrefabChanged(OmniousEditPrefabChanged);
			m_SelectAIDropDown.eventSelectedIndexChanged += new PropertyChangedEventHandler<int>(OmniousSelectedIndexChanged);
			m_check.eventCheckChanged += new PropertyChangedEventHandler<bool>(OmniousCheckChanged);
		}


		/// <summary>
		/// When the checkbox is set attempts to set the Prefab AI to the indicated type (if appropriate)
		/// </summary>
		/// <param name="comp">UICheckBox : UIComponent</param>
		/// <param name="value">isChecked value</param>
		private void OmniousCheckChanged(UIComponent comp, bool value) {
			if(!m_omnious) {
				m_omnious = true;
				if(value && m_prefabinfo.GetAI().GetType().FullName != m_SelectAIDropDown.selectedValue) {
					int vIndex = m_AIInfo.FindIndex(x => x.typo.FullName == m_SelectAIDropDown.selectedValue);
					//m_AIInfo[vIndex].typo; can be presumed to be "good"

					// remove old ai
					var oldAI = m_prefabinfo.gameObject.GetComponent<PrefabAI>();
					UnityEngine.Object.DestroyImmediate(oldAI);

					// add new ai
					var newAI = (PrefabAI) m_prefabinfo.gameObject.AddComponent(m_AIInfo[vIndex].typo);

					TryCopyAttributes(oldAI, newAI);

					m_prefabinfo.TempInitializePrefab();
					RefreshPropertiesPanel(m_prefabinfo);

				}
				m_omnious = false;
			}
		}


		/// <summary>
		/// Copies all attributes that share the same name and type from one AI object to another
		/// </summary>
		/// <param name="oldAI">Source AI instance</param>
		/// <param name="newAI">Destination AI instance</param>
		private void TryCopyAttributes(PrefabAI oldAI, PrefabAI newAI) {
			var oldAIFields =
				oldAI.GetType()
					.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			var newAIFields = newAI.GetType()
				.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

			var newAIFieldDic = new Dictionary<String, FieldInfo>(newAIFields.Length);
			foreach (var field in newAIFields) {
				newAIFieldDic.Add(field.Name, field);
			}

			foreach (var fieldInfo in oldAIFields) {
				if (fieldInfo.IsDefined(typeof (CustomizablePropertyAttribute), true)) {
					FieldInfo newAIField;
					newAIFieldDic.TryGetValue(fieldInfo.Name, out newAIField);
					try {
						if (newAIField.GetType().Equals(fieldInfo.GetType())) {
							newAIField.SetValue(newAI, fieldInfo.GetValue(oldAI));
						}
					} catch (NullReferenceException) { }
				}
			}
		}


		/// <summary>
		/// Reload the property editor panel so the fields of a new AI class appear
		/// </summary>
		/// <param name="prefabInfo">(PrefabInfo) </param>
		private void RefreshPropertiesPanel(PrefabInfo prefabInfo) {
			var decorationPropertiesPanel = m_propPanel.GetComponent<DecorationPropertiesPanel>();
			decorationPropertiesPanel.GetType()
				.InvokeMember("Refresh", BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, decorationPropertiesPanel, new object[] {prefabInfo});
		}


		/// <summary>
		/// When the selected AI type is changes, checks to see if it is the current PrefabAI type and sets the Checkbox appropriately.
		/// </summary>
		/// <param name="comp">(UIComponent) UIDropDown : UIInteractiveComponent</param>
		/// <param name="selected">(int) selectedIndex</param>
		private void OmniousSelectedIndexChanged(UIComponent comp, int selected) {
			if(!m_omnious) {
				m_omnious = true;
				if(m_prefabinfo.GetAI().GetType().FullName == m_SelectAIDropDown.selectedValue) {
					m_check.isChecked = true;
				} else {
					m_check.isChecked = false;
				}
				m_check.tooltip = String.Concat("Set PrefabAI to ", m_SelectAIDropDown.selectedValue);
				m_omnious = false;
			}
		}


		/// <summary>
		/// When the game loads a prefab reinitialize the PrefabAI Panel, and hide/show it as appropriate.
		/// </summary>
		/// <param name="info">(PrefabInfo) m_editPrefabInfo</param>
		private void OmniousEditPrefabChanged(PrefabInfo info) {
			if(!m_omnious) {
				m_prefabinfo = info;
				m_omnious = true;
				m_check.isChecked = false;
				if(m_prefabinfo != null) {
					var assetAI = info.GetAI();
					if(assetAI != null){
						m_SelectAIDropDown.selectedValue = assetAI.GetType().FullName;
						m_check.isChecked = true;
					} else {
						m_SelectAIDropDown.selectedValue = "PrefabAI";
						m_PrefabAIPanel.Hide();
					}
				} else {
					m_SelectAIDropDown.selectedValue = "PrefabAI";
					m_PrefabAIPanel.Hide();
				}
				m_omnious = false;
			}
		}


		//public class DecorationPropertiesPanel : ToolsModifierControl
		/// <summary>
		/// Validate text field input, then if input is okay set the user data object's field indicated by the textfield component.
		/// </summary>
		/// <param name="comp">(UIComponent) UITextField : UIInteractiveComponent</param>
		/// <param name="text">(string) text</param>
		private void OnConfirmChange(UIComponent comp, string text) {
			if (comp == null || comp.objectUserData == null) {
				return;
			}
			FieldInfo field = comp.objectUserData.GetType().GetField(comp.stringUserData, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (field.FieldType == typeof(int)) {
				int num;
				if (int.TryParse(text, out num)) {
					comp.color = Color.white;
					field.SetValue(comp.objectUserData, num);
				//	this.ProcessIndirectFields();
				} else {
					comp.color = Color.red;	//mew, fancy.
				}
			} else if (field.FieldType == typeof(float)) { //branch left intact for future reuse.
				float num2;
				if (float.TryParse(text, out num2)) {
					comp.color = Color.white;
					field.SetValue(comp.objectUserData, num2);
				//	this.ProcessIndirectFields();
				} else {
					comp.color = Color.red;
				}
			}
		}

	}

}
