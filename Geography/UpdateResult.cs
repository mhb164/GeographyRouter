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

        public UpdateResult(bool result, string message)
        {
            Result = result;
            Message = message;
        }

        public static UpdateResult Success(string message = "عملیات با موفقیت انجام شد.") => new UpdateResult(true, message);
        public static UpdateResult Failed(string message = "عملیات با خطا مواجه شد.") => new UpdateResult(false, message);
        public static UpdateResult ByResult(bool result, string message = "") => new UpdateResult(result, message);
    }
}