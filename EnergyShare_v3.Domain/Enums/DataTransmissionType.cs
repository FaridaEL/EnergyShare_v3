using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnergyShare_v3.Domain.Enums
{
    public enum DataTransmissionType
    {
        [Display(Name = "LienSharePoint")]
        LienSharePoint = 1,
        [Display(Name = "SFTP")]
        SFTP = 2
    }
}
