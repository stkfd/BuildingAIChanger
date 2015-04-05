using System;
using UnityEngine;
using ICities;

namespace BuildingAIChanger
{
    public class BuildingAIChangerUserMod : IUserMod
    {
        public string Name
        {
            get { return "Building AI Changer"; }
        }

        public string Description
        {
            get { return "Allows  you to change the Building AI in the Asset Editor"; }
        }
    }
}
