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

  const currentPage = searchParams.page ?? 1;

  return (
    <div className="mt-12 mb-24 flex w-full flex-col items-start justify-start space-x-12 md:flex-row">
      <div className="sticky top-4 mt-12 max-h-[calc(100vh-5rem)] flex-1 self-start overflow-y-auto">
        <Filters
          searchParams={searchParams}
          onSearchParamChange={(updatedParams) =>
            navigate({ search: { ...updatedParams, page: 1 } })
          }
        />
      </div>
      <Suspense fallback={<ListingListSkeleton />}>
        <ListingList
          listingSearchQuery={searchParams}
          page={currentPage}
          onPageChange={(p) => navigate({ search: { ...searchParams, page: p } })}
        />
      </Suspense>
    </div>
  );
};

export default Listings;
