namespace Automotive.Marketplace.Infrastructure.Repositories;

using Automotive.Marketplace.Infrastructure.Data.DbContext;

public abstract class BaseRepository(AutomotiveContext automotiveContext)
{
    protected readonly AutomotiveContext automotiveContext = automotiveContext;

    //TODO: Implement CRUD
}
