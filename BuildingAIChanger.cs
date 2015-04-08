/* BuildingAIChanger
 * Copyright (c) 2015 Stefan Kaufhold, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */

using ColossalFramework.UI;
using ICities;

namespace BuildingAIChanger
{
    public class BuildingAIChanger : LoadingExtensionBase, IUserMod
    {
        private SelectAIPanel m_selectAI;

        public string Name
        {
            get { return "Building AI Changer"; }
        }

        public string Description
        {
            get { return "Allows you to change the Building AI in the Asset Editor"; }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset)
            {
                SelectAIPanel.Insert();
            }
        }
    }
}
