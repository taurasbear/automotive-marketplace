namespace Automotive.Marketplace.Infrastructure.Repositories
{
    using Automotive.Marketplace.Infrastructure.Data.DbContext;

    public abstract class BaseRepository
    {
        protected readonly AutomotiveContext automotiveContext;

        protected BaseRepository(AutomotiveContext automotiveContext)
        {
            this.automotiveContext = automotiveContext;
        }

        //TODO: Implement CRUD
    }
}
