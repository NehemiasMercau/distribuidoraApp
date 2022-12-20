using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.enumeradores
{
    public enum TipoCobroE
    {
        EFECTIVO = 1,
        TARJETA_DEBITO = 2,
        TARJETA_CREDITO = 3,
        CUENTA_CORRIENTE = 4,
        CHEQUE = 5,
        TRANSFERENCIA_BANCARIA = 6,
        OTROS = 7

    }
}
