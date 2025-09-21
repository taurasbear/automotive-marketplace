namespace Automotive.Marketplace.Domain.Constants;

public static class ValidationConstants
{
    public static class Password
    {
        public const int MIN = 6;

        public const int MAX = 100;
    }

    public static class Username
    {
        public const int MIN = 3;

        public const int MAX = 100;
    }

    public static class Text
    {
        public const int SHORT = 55;

        public const int MEDIUM = 255;

        public const int LONG = 1000;
    }

    public static class Name
    {
        public const int SHORT = 5;

        public const int MEDIUM = 30;

        public const int LONG = 125;
    }
}