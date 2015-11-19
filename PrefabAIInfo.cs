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
    public class PrefabAIInfo
    {
        public PrefabAIInfo(Type aiType)
        {
            type = aiType;

            if (!type.IsSubclassOf(typeof (PrefabAI))) throw new PrefabAITypeException();
            
            if (type.IsSubclassOf(typeof(BuildingAI)))
            {
                aiGroup = PrefabAIGroup.BuildingAI;
            }
            else if (type.IsSubclassOf(typeof(CitizenAI)))
            {
                aiGroup = PrefabAIGroup.CitizenAI;
            }
            else if (type.IsSubclassOf(typeof(NetAI)))
            {
                aiGroup = PrefabAIGroup.NetAI;
            }
            else if (type.IsSubclassOf(typeof (VehicleAI)))
            {
                aiGroup = PrefabAIGroup.VehicleAI;
            }
            else
            {
                aiGroup = PrefabAIGroup.Other;
            }

            /*var derived = type;
            do
            {
                derived = derived.BaseType;
                if (derived != null)
                {
                    // temp.fullname.Insert(0,derived.FullName);
                    temp.fullname = String.Concat(derived.FullName, ".", temp.fullname);
                }
            } while (derived != null);*/
        }

        public readonly Type type; //type of Prefab AI
        public readonly string fullname; //not an actual valid fullname, but an approximation used for sorting
        public readonly PrefabAIGroup aiGroup;
    }

    /// <summary>
    /// Thrown when a Type is unexpectedly not a subclass of PrefabAI
    /// </summary>
    public class PrefabAITypeException : Exception { }

    public enum PrefabAIGroup
    {
        Other,
        BuildingAI,
        CitizenAI,
        NetAI,
        VehicleAI
    }
}