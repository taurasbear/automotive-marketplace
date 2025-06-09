import { ListingDetailsWithCar } from "@/shared/types/dto/Listing/GetListingDetailsWithCarResponse";
import CarListingCard from "@/components/CarListings/CarListingCard";

export interface CarListingsProps {
  listings: ListingDetailsWithCar[];
}

const CarListings: React.FC<CarListingsProps> = ({ listings }) => {
  return (
    <div className="flex flex-row justify-center">
      <div className="bg-background text-on-background flex w-188 flex-col gap-10">
        {listings?.map((l) => <CarListingCard key={l.price} listing={l} />)}
      </div>
    </div>
  );
};

export default CarListings;
