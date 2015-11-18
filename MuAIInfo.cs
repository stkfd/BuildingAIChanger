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

namespace BuildingAIChanger
{
    public class MuAIInfo
    {
        public Type type; //type of Prefab AI
        public string fullname; //not an actual valid fullname, but an approximation used for sorting
        //could have relied on a recursive branch discorvery, but meh.
        public bool buildingAI = false;
        public bool citizenAI = false;
        public bool netAI = false;
        public bool vehicleAI = false;
    }
}