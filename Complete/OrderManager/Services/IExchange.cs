using System.Collections.Generic;
using System.ServiceModel;
using Almirex.Contracts.Messages;

namespace OrderManager.Services
{
    [ServiceContract]
//    [XmlSerializerFormat]

    public interface IExchange
    {
        [OperationContract]
        List<ExecutionReport> GetOrders();
        [OperationContract]
        ExecutionReport GetOrder(string id);
        [OperationContract]
        List<ExecutionReport> NewOrder(ExecutionReport order);

        [OperationContract]
        ExecutionReport CancelOrder(string id);
    }
    
}
