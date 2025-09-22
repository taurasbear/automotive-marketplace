import { Route } from "@/app/routes/listings";
import { getAllListingsOptions, ListingCard } from "@/features/listing";
import { useSuspenseQuery } from "@tanstack/react-query";

const Listings = () => {
  const searchParams = Route.useSearch();

  const { data: listingsQuery } = useSuspenseQuery(
    getAllListingsOptions(searchParams),
  );

  const listings = listingsQuery.data;

  return (
    <div className="flex flex-row justify-center">
      <div className="bg-background text-on-background flex w-188 flex-col gap-10">
        {listings.map((l) => (
          <ListingCard key={l.id} listing={l} />
        ))}
      </div>
    </div>
  );
};

export default Listings;
