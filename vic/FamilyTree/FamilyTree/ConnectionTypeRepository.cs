//namespace FamilyTree
//{
//    public class ConnectionTypeRepository
//    {
//        private List<ConnectionType> connectionTypes = new List<ConnectionType>();

//        public ConnectionTypeRepository()
//        {
//            // Инициализация стандартными типами связей
//            connectionTypes.Add(new ConnectionType(1, "Мужья-жены"));
//            connectionTypes.Add(new ConnectionType(2, "Родители-дети"));
//            connectionTypes.Add(new ConnectionType(3, "Братья-сестры"));
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
