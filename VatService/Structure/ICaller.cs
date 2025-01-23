using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VatService.Model;
//metodi rest
namespace VatService.Structure
{
    public interface ICaller
    {
         void Verify(ref CheckVatResponse response);
    }
}
