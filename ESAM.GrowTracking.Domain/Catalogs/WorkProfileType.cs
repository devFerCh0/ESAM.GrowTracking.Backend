using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ESAM.GrowTracking.Domain.Catalogs
{
    public enum WorkProfileType : byte
    {
        [Display(Name = "None")]
        [EnumMember(Value = "None")]
        None = 0,

        [Display(Name = "WithRoles")]
        [EnumMember(Value = "WithRoles")]
        WithRoles = 1,

        [Display(Name = "OnlyWorkProfile")]
        [EnumMember(Value = "OnlyWorkProfile")]
        OnlyWorkProfile = 2
    }
}