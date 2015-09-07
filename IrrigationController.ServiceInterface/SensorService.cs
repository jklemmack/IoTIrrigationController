using IrrigationController.Model;
using IrrigationController.Model.Types;
using ServiceStack;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationController.ServiceInterface
{
    public class SensorService : Service
    {
        public object Get(Sensor request)
        {
            return Db.SingleById<Sensor>(request.Id);
        }

        public object Put(Sensor request)
        {
            Db.Save<Sensor>(request);
            return Get(request);
        }

        public object Post(Sensor request)
        {
            Db.Save<Sensor>(request);
            return Get(request);
        }

        public object Delete(Sensor request)
        {
            Db.Delete<Sensor>(request);
            return true;
        }
    }
}
