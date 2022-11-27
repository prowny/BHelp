namespace BHelp.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<BHelp.DataAccessLayer.BHelpContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "BHelp.DataAccessLayer.BHelpContext";
        }

        protected override void Seed(BHelp.DataAccessLayer.BHelpContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
