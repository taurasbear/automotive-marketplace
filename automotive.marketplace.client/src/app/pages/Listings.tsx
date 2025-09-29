import { Route } from "@/app/routes/listings";
import Filters from "@/features/listingList/components/Filters";
import ListingList from "@/features/listingList/components/ListingList";

const Listings = () => {
  const searchParams = Route.useSearch();
  const navigate = Route.useNavigate();

  return (
    <div className="mt-12 flex w-full items-start justify-start space-x-12">
      <div className="mt-48 flex-1">
        <Filters
          searchParams={searchParams}
          onSearchParamChange={(searchParams) =>
            navigate({ search: searchParams })
          }
        />
      </div>
      <ListingList listingSearchQuery={searchParams} />
    </div>
  );
};

export default Listings;
