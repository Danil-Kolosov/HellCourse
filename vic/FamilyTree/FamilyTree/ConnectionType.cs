using System.Collections.Generic;
using System.Linq;

namespace FamilyTree
{
    public static class ConnectionType
    {
        private static readonly Dictionary<int, string> ConnectionTypes = new Dictionary<int, string>
    {
        {1, "Мужья-жены"},
        {2, "Родители-дети"},
        {3, "Братья-сестры"}
    };

        public static string GetConnectionTypeName(int conTypeId)
        {
            return ConnectionTypes.ContainsKey(conTypeId) ? ConnectionTypes[conTypeId] : "Неизвестный тип связи";
        }

        public static bool IsValidConnectionType(int conTypeId)
        {
            return ConnectionTypes.ContainsKey(conTypeId);
        }
    }
}
