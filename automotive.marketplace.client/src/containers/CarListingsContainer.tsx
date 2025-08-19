import CarListings from "@/components/CarListings/CarListings";
import { getAllListingsOptions } from "@/shared/utils/queries/listing/getAllListingsOptions";
import { useQuery } from "@tanstack/react-query";

const CarListingsContainer = () => {
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

  return <CarListings listings={listings!} />;
};

export default CarListingsContainer;
