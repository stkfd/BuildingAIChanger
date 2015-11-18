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
using System.Linq;
using System.Reflection;
using System.Threading;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace BuildingAIChanger
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
                    m_toolController = ToolsModifierControl.toolController;
                    m_toolController.eventEditPrefabChanged += InitializeUI;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                UIView.ForwardException(new ModException("Prefab AI Changer caused an error", ex));
            }
        }

        private ToolController m_toolController;

        private PrefabInfo m_prefabInfo;

        private MeowUI m_meowUI;

        private Semaphore sem = new Semaphore(1, 1);

        private void InitializeUI(PrefabInfo info)
        {
            m_toolController.eventEditPrefabChanged -= InitializeUI;
            m_prefabInfo = info;
            m_meowUI = MeowUI.BuildInstance(info);
            m_meowUI.eventCheckChanged += OmniousCheckChanged;
            m_meowUI.eventSelectedAIChanged += OmniousSelectedValueChanged;
            m_toolController.eventEditPrefabChanged += OmniousEditPrefabChanged;
        }

        /// <summary>
        /// When the checkbox is set attempts to set the Prefab AI to the indicated type (if appropriate)
        /// </summary>
        /// <param name="comp">UICheckBox : UIComponent</param>
        /// <param name="value">isChecked value</param>
        private void OmniousCheckChanged(MeowUI ui, bool value)
        {
            sem.WaitOne();
            if (value && m_prefabInfo.GetAI().GetType().FullName != ui.m_selectAIDropDown.selectedValue)
            {
                var aiInfo = ui.m_aiInfo.First(x => x.type.FullName == ui.m_selectAIDropDown.selectedValue);

                // remove old ai
                var oldAI = m_prefabInfo.gameObject.GetComponent<PrefabAI>();
                UnityEngine.Object.DestroyImmediate(oldAI);

                // add new ai
                var newAI = (PrefabAI) m_prefabInfo.gameObject.AddComponent(aiInfo.type);

                TryCopyAttributes(oldAI, newAI);

                m_prefabInfo.TempInitializePrefab();
                m_meowUI.RefreshPropertiesPanel(m_prefabInfo);
            }
            sem.Release();
        }


        /// <summary>
        /// Copies all attributes that share the same name and type from one AI object to another
        /// </summary>
        /// <param name="oldAI">Source AI instance</param>
        /// <param name="newAI">Destination AI instance</param>
        private void TryCopyAttributes(PrefabAI oldAI, PrefabAI newAI)
        {
            var oldAIFields =
                oldAI.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                               BindingFlags.FlattenHierarchy);
            var newAIFields = newAI.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                           BindingFlags.FlattenHierarchy);

            var newAIFieldDic = new Dictionary<String, FieldInfo>(newAIFields.Length);
            foreach (var field in newAIFields)
            {
                newAIFieldDic.Add(field.Name, field);
            }

            foreach (var fieldInfo in oldAIFields)
            {
                if (fieldInfo.IsDefined(typeof (CustomizablePropertyAttribute), true))
                {
                    FieldInfo newAIField;
                    newAIFieldDic.TryGetValue(fieldInfo.Name, out newAIField);
                    try
                    {
                        if (newAIField.GetType().Equals(fieldInfo.GetType()))
                        {
                            newAIField.SetValue(newAI, fieldInfo.GetValue(oldAI));
                        }
                    }
                    catch (NullReferenceException)
                    {
                    }
                }
            }
        }
        
        /// <summary>
        /// When the selected AI type is changes, checks to see if it is the current PrefabAI type and sets the Checkbox appropriately.
        /// </summary>
        /// <param name="comp">(MeowUI) MeowUI</param>
        /// <param name="selected">(string) selectedValue</param>
        private void OmniousSelectedValueChanged(MeowUI ui, string selected)
        {
            sem.WaitOne();

            ui.m_check.isChecked = m_prefabInfo.GetAI().GetType().FullName == selected;
            ui.m_check.tooltip = string.Concat("Set PrefabAI to ", selected);

            sem.Release();
        }


        /// <summary>
        /// When the game loads a prefab reinitialize the PrefabAI Panel, and hide/show it as appropriate.
        /// </summary>
        /// <param name="info">(PrefabInfo) m_editPrefabInfo</param>
        private void OmniousEditPrefabChanged(PrefabInfo info)
        {
            sem.WaitOne();

            m_prefabInfo = info;
            m_meowUI.Reset(info);

            sem.Release();
        }
    }
}