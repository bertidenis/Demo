using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//metodi wsdl
namespace VatService.Structure
{
    public interface IOperations
    {
        void GetBulk(ref DataSet dataSet);
        void WriteOnDB();
        void Connect();
        void Dispose();
    }
}
