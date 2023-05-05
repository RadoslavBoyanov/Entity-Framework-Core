using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftJail
{
    public static class GlobalConstants
    {
        //Prisoner
        public const int FullNameMaxLengthPrisioner = 20;
        public const int FullNameMinLengthPrisioner = 3;
        public const int MinAgePrisioner = 18;
        public const int MaxAgePrisioner = 65;
        public const string PrisonerBailMinValue = "0.0";
        public const string PrisonerBailMaxValue = "79228162514264337593543950335";
        public const string NickNameExpression = "^(The\\s)([A-Z]{1}[a-z]*)$";

        //Officer
        public const int OfficerFullNameMaxLength = 30;
        public const int OfficerFullNameMinLength = 3;
        public const string SalaryOfficerMin = "0.0";
        public const string SalaryOfficerMax = "79228162514264337593543950335";

        //Cell
        public const int CellNumberMax = 1000;
        public const int CellNumberMin = 1;

        //Mail
        public const string AddressExpression = "^([A-Za-z0-9\\s]+?)(\\sstr\\.)$";

        //Department
        public const int DepartmentMaxLength = 25;
        public const int DepartmentMinLength = 3;
    }
}
