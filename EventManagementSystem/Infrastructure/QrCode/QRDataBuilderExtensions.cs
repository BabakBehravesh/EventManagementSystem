namespace EventManagementSystem.Infrastructure.QrCode
{
    public static class QRDataBuilderExtensions
    {
        public static QRDataBuilder WithEmail(this QRDataBuilder builder, string email)
        {
            return builder.WithCustomField("EMAIL", email);
        }

        public static QRDataBuilder WithName(this QRDataBuilder builder, string name)
        {
            return builder.WithCustomField("NAME", name);
        }

        public static QRDataBuilder WithRoles(this QRDataBuilder builder, ICollection<string> roles)
        {
            return builder.WithCustomField("ROLES", string.Join("," , roles.Select(role => role.ToString())));
        }
    }
}
