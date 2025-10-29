using System.Collections.Generic;
using System.Linq;

namespace FamilyTree
{
    public static class ConnectionType
    {
        public static readonly Dictionary<int, string> ConnectionTypes = new Dictionary<int, string>
    {
        {1, "супружество"},
        {2, "родитель-ребенок"},
        {3, "братство-сестринство"}
    };

        public static string GetConnectionTypeName(int conTypeId)
        {
            return ConnectionTypes.ContainsKey(conTypeId) ? ConnectionTypes[conTypeId] : "Неизвестный тип связи";
        }

        public static bool IsValidConnectionType(int conTypeId)
        {
            return ConnectionTypes.ContainsKey(conTypeId);
        }

        public static List<KeyValuePair<int, string>> GetAllConnectionTypes()
        {
            return ConnectionTypes.ToList();
        }
    }
}
