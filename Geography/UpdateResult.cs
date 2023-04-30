using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class GeographyRepository
{
    public class UpdateResult
    {
        public readonly bool Result;
        public readonly string Message;

        protected UpdateResult(bool result, string message)
        {
            Result = result;
            Message = message;
        }

        public static UpdateResult Success(string message = "عملیات با موفقیت انجام شد.") => new UpdateResult(true, message);
        public static UpdateResult Failed(string message = "عملیات با خطا مواجه شد.") => new UpdateResult(false, message);
        public static UpdateResult ByResult(bool result, string message = "") => new UpdateResult(result, message);
    }

    public class UpdateElementResult: UpdateResult
    {
        public readonly bool PointsChanged;
        public readonly bool FieldValuesChanged;
        public readonly bool StatusChanged;

        protected UpdateElementResult(bool result, bool pointsChanged, bool fieldValuesChanged, bool statusChanged, string message) : base(result, message)
        {
            PointsChanged = pointsChanged;
            FieldValuesChanged = fieldValuesChanged;
            StatusChanged = statusChanged;
        }

        public static UpdateElementResult Failed(string message = "عملیات با خطا مواجه شد.") => new UpdateElementResult(false, false, false, false, message);
        public static UpdateElementResult Success(bool pointsChanged, bool fieldValuesChanged, bool statusChanged,string message = "عملیات با موفقیت انجام شد.") 
            => new UpdateElementResult(true, pointsChanged, fieldValuesChanged, statusChanged, message);

    }
}