using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColossalFramework;
using UnityEngine;

namespace PrefabAIChanger
{
    class PrefabAITypes : Singleton<PrefabAITypes>
    {
        private PrefabAITypes() { }
        
        private bool typesLoaded = false;
        private readonly List<PrefabAIInfo> allTypes = new List<PrefabAIInfo>();

        public List<PrefabAIInfo> All()
        {
            if (typesLoaded) return allTypes;

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in a.GetTypes())
                {
                    if (type.IsSubclassOf(typeof (PrefabAI)))
                    {
                        allTypes.Add(new PrefabAIInfo(type));
                    }
                }
            }

            typesLoaded = true;

            return allTypes;
        }

        public PrefabAIInfo GetAIInfo(string name)
        {
            return All().Find((info) => info.type.FullName == name);
        }

        public void Invalidate()
        {
            allTypes.Clear();
            typesLoaded = false;
        }
    }
}
