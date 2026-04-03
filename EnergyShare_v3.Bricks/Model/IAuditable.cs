using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Bricks.Model
/*Version simplifiée de l’audit pour répondre au besoin métier principal (traçabilité),
 * sans introduire de complexité inutile (DomainEvents ou les behaviors complets),
 * qui ne sont pas indispensables pour le MVP.*/
{
    public interface IAuditable
    {
        AuditInfo Audit { get; }
    }
}
