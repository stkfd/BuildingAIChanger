using System;
using System.Collections.Generic;
using ColossalFramework;
using UnityEngine;
using System.Reflection;
using System.Linq;

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
                IEnumerable<Type> types;
                try
                {
                    types = a.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    continue;
                }
                
                allTypes.AddRange(
                    types.Where(t => t.IsSubclassOf(typeof(PrefabAI)))
                        .Select(t => new PrefabAIInfo(t))
                );
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
