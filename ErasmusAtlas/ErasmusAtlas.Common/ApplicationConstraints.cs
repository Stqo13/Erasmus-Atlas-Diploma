namespace ErasmusAtlas.Common;

public static class ApplicationConstraints
{
    public static class CityConstraints
    {
        public const int CityNameMaxLength = 120;

        public const int CountryIsoLength = 2;
    }

    public static class UserConstraints
    {
        public const int UserFirstNameMaxLength = 80;
        public const int UserLastNameMaxLength = 80;
    }

    public static class InstitutionConstraints
    {
        public const int InstitutionNameMaxLength = 180;

        public const int InstitutionWebsiteUrlMaxLength = 400;
    }

    public static class PostConstraints
    {
        public const int PostTitleMaxLength = 160;

        public const int PostBodyMaxLength = 8000;

        public const int PostStatusMaxLength = 32;
    }

    public static class ProjectConstraints
    {
        public const int ProjectTitleMaxLength = 200;

        public const int ProjectDescriptionMaxLength = 12000;
    }

    public static class ProjectApplicationConstraints
    {
        public const int ProjectApplicationStatusMaxLength = 24;

        public const int ProjectApplicationMotivationMaxLength = 3000;
    }

    public static class TagConstraints
    {
        public const int TagNameMaxLength = 50;
    }

    public static class TopicConstraints
    {
        public const int TopicNameMaxLength = 50;
    }
}
