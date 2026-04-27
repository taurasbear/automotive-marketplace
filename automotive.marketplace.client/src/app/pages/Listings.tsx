import { Route } from "@/app/routes/listings";
import { Filters, ListingList } from "@/features/listingList";
import { Suspense } from "react";

const ListingListSkeleton = () => (
  <div className="flex w-204 flex-col gap-10">
    {Array.from({ length: 3 }).map((_, i) => (
      <div key={i} className="bg-card border-border h-48 w-full animate-pulse rounded border-1" />
    ))}
  </div>
);

const Listings = () => {
  const searchParams = Route.useSearch();
  const navigate = Route.useNavigate();

  return (
    <div className="mt-12 mb-24 flex w-full flex-col items-start justify-start space-x-12 md:flex-row">
      <div className="sticky top-4 mt-12 flex-1 self-start max-h-[calc(100vh-5rem)] overflow-y-auto">
        <Filters
          searchParams={searchParams}
          onSearchParamChange={(searchParams) =>
            navigate({ search: searchParams })
          }
        />
      </div>
      <Suspense fallback={<ListingListSkeleton />}>
        <ListingList listingSearchQuery={searchParams} />
      </Suspense>
    </div>
  );
};

export default Listings;
