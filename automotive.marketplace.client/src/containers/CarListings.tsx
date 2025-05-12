import CarListingCard from "../components/CarListingCard";
import { useGetListings } from "../shared/utils/queries/ListingQueries";

const CarListings = () => {
  const { data: listings, error, isLoading } = useGetListings();

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

  console.log("Listings: ", listings);

  return (
    <div className="flex flex-row justify-center">
      <div className="bg-background text-on-background flex w-188 flex-col gap-10">
        {listings?.map((l) => <CarListingCard key={l.price} listing={l} />)}
      </div>
    </div>
  );
};

export default CarListings;
