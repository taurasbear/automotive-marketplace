import { Route } from "@/app/routes/listings";
import { getAllListingsOptions } from "@/features/listing/api/getAllListingsOptions";
import ListingCard from "@/features/listing/components/ListingCard";
import { useQuery } from "@tanstack/react-query";

const Listings = () => {
  const searchParams = Route.useSearch();

  const {
    data: listingsQuery,
    error,
    isLoading: isPending,
  } = useQuery(getAllListingsOptions(searchParams));

  const listings = listingsQuery?.data;

  if (error) {
    return (
      <div>
        <h1>{error.message}</h1>
      </div>
    );
  }

  if (isPending) {
    return (
      <div>
        <h1>Loading...</h1>
      </div>
    );
  }

  return (
    <div className="flex flex-row justify-center">
      <div className="bg-background text-on-background flex w-188 flex-col gap-10">
        {listings?.map((l) => <ListingCard key={l.price} listing={l} />)}
      </div>
    </div>
  );
};

export default Listings;
