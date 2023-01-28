namespace API.Extensions
{
    public static class DateTimeExtentions
    {
        public static int CalcuateAge(this DateTime dob)
        {
            var today = DateTime.UtcNow;
            var age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;
            return age;
        }
    }
}
