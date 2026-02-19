public static class OTPGenerator
{
    public static string GenerateOTP()
    {
        Random rnd = new();
        return rnd.Next(100000, 999999).ToString();
    }
}