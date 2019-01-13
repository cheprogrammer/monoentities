using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonoEntities
{
    /// <summary>
    /// Factory for building GameObject Component System Manager
    /// </summary>
    public static class EcsServiceFactory
    {
        private static bool _assembliesScanned = false;

        private static readonly List<Type> AvailableTemplates = new List<Type>();

        /// <summary>
        /// Performs initial scan of assemblies for EC Systems
        /// </summary>
        /// <param name="assemblies"></param>
        public static void ScanAssemblies(params Assembly[] assemblies)
        {
            AvailableTemplates.Clear();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (typeof(EntityTemplate).IsAssignableFrom(type) &&
                        type.GetCustomAttribute<EntityTemplateAttribute>() != null)
                    {
                        AvailableTemplates.Add(type);
                    }
                }
            }

            _assembliesScanned = true;
        }

        /// <summary>
        /// Creates fully initialized ECS Manager
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public static EcsService CreateECSManager()
        {
            if (!_assembliesScanned)
                throw new Exception($"Unable to create ECS Service. Call method '{nameof(ScanAssemblies)}' before creating ECS Manager");

            EcsService result = new EcsService(AvailableTemplates);

            return result;
        }
    }
}
