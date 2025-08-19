import ListingCard from "@/components/Listings/ListingCard";
import { getAllListingsOptions } from "@/shared/utils/queries/listing/getAllListingsOptions";
import { useQuery } from "@tanstack/react-query";

const Listings = () => {
  const {
    data: listingsQuery,
    error,
    isLoading,
  } = useQuery(getAllListingsOptions);

  const listings = listingsQuery?.data;

  if (error) {
    return (
      <div>
        <h1>{error.message}</h1>
      </div>
    );
  }

  if (isLoading) {
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
