import CarListings from "@/components/CarListings/CarListings";
import { useGetListings } from "@/shared/utils/queries/ListingQueries";

const CarListingsContainer = () => {
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

  return <CarListings listings={listings!.listingsDetailsWithCar} />;
};

export default CarListingsContainer;
