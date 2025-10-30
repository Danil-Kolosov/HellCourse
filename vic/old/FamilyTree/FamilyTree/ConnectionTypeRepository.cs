//namespace FamilyTree
//{
//    public class ConnectionTypeRepository
//    {
//        private List<ConnectionType> connectionTypes = new List<ConnectionType>();

//        public ConnectionTypeRepository()
//        {
//            // Инициализация стандартными типами связей
//            connectionTypes.Add(new ConnectionType(1, "супружество"));
//            connectionTypes.Add(new ConnectionType(2, "родитель-ребенок"));
//            connectionTypes.Add(new ConnectionType(3, "братство-сестринство"));
//        }

//        public void Add(ConnectionType connectionType)
//        {
//            connectionTypes.Add(connectionType);
//        }

//        public ConnectionType GetConnectionType(int connectionTypeId)
//        {
//            return connectionTypes.FirstOrDefault(ct => ct.ConnectionTypeId == connectionTypeId);
//        }

//        public string GetConnectionTypeName(int connectionTypeId)
//        {
//            return ConnectionType.GetConnectionTypeName(connectionTypeId, connectionTypes);
//        }

//        public List<ConnectionType> GetAllConnectionTypes() => new List<ConnectionType>(connectionTypes);
//    }
//}
