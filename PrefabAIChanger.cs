/** PrefabAIChanger
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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace PrefabAIChanger
{
    /// <summary>
    /// AI Changer main mod class; provides Mod Info and handles loading
    /// </summary>
    public class PrefabAIChanger : IUserMod
    {
        public string Name
        {
            get { return "Asset Prefab AI Changer"; }
        }

        public string Description
        {
            get
            {
                return
                    "Allows you to change the Prefab AI in the Asset Editor. incl building, vehicle, citizen, net etc.";
            }
        }
    }

    /// <summary>
    /// Prefab AI Changer main mod class; 
    /// </summary>
    public class PrefabAIChangerExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            try
            {
                if (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset)
                {
                    toolController = ToolsModifierControl.toolController;
                    toolController.eventEditPrefabChanged += InitializeUI;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                UIView.ForwardException(new ModException("Prefab AI Changer caused an error", ex));
            }
        }

        private ToolController toolController;

        private PrefabInfo prefabInfo;

        private MeowUI meowUI;

        private Semaphore sem = new Semaphore(1, 1);

        /// <summary>
        /// Initialize the UI elements and set up UI events
        /// </summary>
        /// <param name="info"></param>
        private void InitializeUI(PrefabInfo info)
        {
            toolController.eventEditPrefabChanged -= InitializeUI;
            prefabInfo = info;
            meowUI = MeowUI.InsertInstance(info);
            meowUI.eventApplyAIClick += ApplyNewAI;
            toolController.eventEditPrefabChanged += OnEditPrefabChanged;
        }

        /// <summary>
        /// When the checkbox is set 
        /// Attempts to set the Prefab AI to the indicated type (if appropriate)
        /// </summary>
        private void ApplyNewAI(UIComponent ui, UIMouseEventParameter e)
        {
            sem.WaitOne();

            var confirmPanel = UIView.library.ShowModal<ConfirmPanel>("ConfirmPanel", (UIComponent component, int result) =>
            {
                if (result != -1)
                {
                    if (prefabInfo.GetAI().GetType().FullName != meowUI.selectAIDropDown.selectedValue)
                    {
                        // remove old ai
                        var oldAI = prefabInfo.gameObject.GetComponent<PrefabAI>();
                        UnityEngine.Object.DestroyImmediate(oldAI);

                        // add new ai
                        var newAIInfo = meowUI.SelectedAIInfo;
                        if (newAIInfo != null)
                        {
                            var newAI = (PrefabAI) prefabInfo.gameObject.AddComponent(newAIInfo.type);
                            TryCopyAttributes(oldAI, newAI, result == 0);
                            prefabInfo.InitializePrefab();
                            meowUI.PrefabInfo = prefabInfo;
                        }
                        else
                        {
                            Debug.LogError("New AI Info could not be found.");
                        }
                    }
                }
                sem.Release();
            });
            confirmPanel.SetMessage("Copy all attributes", "Copy only visible attributes or all attributes? Only copy all attributes if you know what you are doing.");
            confirmPanel.Find<UIButton>("Yes").text = "All";
            confirmPanel.Find<UIButton>("No").text = "Only visible";
        }

        /// <summary>
        /// Copies all attributes that share the same name and type from one AI object to another
        /// </summary>
        /// <param name="src">Source AI instance</param>
        /// <param name="dst">Destination AI instance</param>
        /// <param name="safe"></param>
        private void TryCopyAttributes(PrefabAI src, PrefabAI dst, bool safe = true)
        {
            var oldAIFields = src.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            var newAIFields = dst.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            var newAIFieldDic = new Dictionary<string, FieldInfo>(newAIFields.Length);
            foreach (var field in newAIFields)
            {
                newAIFieldDic.Add(field.Name, field);
            }

            foreach (var fieldInfo in oldAIFields)
            {
                // do not copy attributes marked NonSerialized
                bool copyField = !fieldInfo.IsDefined(typeof(NonSerializedAttribute), true);
                
                if (safe && !fieldInfo.IsDefined(typeof(CustomizablePropertyAttribute), true)) copyField = false;

                if (copyField)
                {
                    FieldInfo newAIField;
                    newAIFieldDic.TryGetValue(fieldInfo.Name, out newAIField);
                    try
                    {
                        if (newAIField != null && newAIField.GetType().Equals(fieldInfo.GetType()))
                        {
                            newAIField.SetValue(dst, fieldInfo.GetValue(src));
                        }
                    }
                    catch (NullReferenceException)
                    {
                    }
                }
            }
        }
        
        /// <summary>
        /// When the game loads a prefab reinitialize the PrefabAI Panel, and hide/show it as appropriate.
        /// </summary>
        /// <param name="info">(PrefabInfo) m_editPrefabInfo</param>
        private void OnEditPrefabChanged(PrefabInfo info)
        {
            prefabInfo = info;

            var ai = prefabInfo.gameObject.GetComponent<PrefabAI>();
            if (ai == null)
            {
                meowUI.Hide();
            }
            else
            {
                meowUI.PrefabInfo = info;
                meowUI.ResetDropDown();
                meowUI.Show();
            }
        }
    }
}