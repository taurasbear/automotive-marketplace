import EnginePill from "./EnginePill";
import ListingCardBadge from "./ListingCardBadge";
import { Button } from "./ui/button";
import { PiEngine } from "react-icons/pi";
import { MdOutlineLocalGasStation } from "react-icons/md";
import { TbManualGearbox } from "react-icons/tb";
import { IoLocationOutline } from "react-icons/io5";
import { GetListingDetailsWithCarResponse } from "@/shared/types/dto/Listing/GetListingDetailsWithCarResponse";

interface CarListingCardProps {
  listing: GetListingDetailsWithCarResponse;
}

const CarListingCard: React.FC<CarListingCardProps> = ({ listing }) => {
  return (
    <div className="bg-surface border-secondary-border grid w-full grid-cols-2 gap-8 border-1">
      <div className="flex flex-shrink-0 py-5">
        <img
          className="aspect-[4/3] object-cover"
          alt="Toyota Prius 2025"
          src="https://imgs.search.brave.com/_avFlFDyXU8SS34ve__STsLcC6LfrFsy76XnfAbI4Vo/rs:fit:860:0:0:0/g:ce/aHR0cHM6Ly9tZWRp/YS5nZXR0eWltYWdl/cy5jb20vaWQvNDU5/NDQ1ODUxL3Bob3Rv/L3RveW90YS1wcml1/cy5qcGc_cz02MTJ4/NjEyJnc9MCZrPTIw/JmM9OGRDdF9lSGxP/YzhMcUxEQllYME42/N0FpZFNNd2lRT0ZT/LVhzMUxYcnBjQT0"
        />
      </div>
      <div className="flex min-w-0 flex-grow flex-col justify-between pt-4 pr-4 pb-2">
        <div className="truncate">
          <p className="truncate font-sans text-xs">
            {listing.used ? "Used" : "New"}
          </p>
          <p className="font-sans text-xl">{`${listing.year} ${listing.make} ${listing.model}`}</p>
          <p className="font-sans text-xs">{listing.mileage} km</p>
          <p className="font-sans text-3xl font-bold">{listing.price} €</p>
        </div>
        <div className="justify-items-stretched grid grid-cols-2 gap-x-0 gap-y-4">
          <div className="flex justify-self-start">
            <ListingCardBadge
              Icon={<PiEngine className="h-8 w-8" />}
              title={"Engine"}
              stat={`${listing.engineSize / 1000} l ${listing.power} kW`}
            />
          </div>
          <div className="flex justify-self-end">
            <ListingCardBadge
              Icon={<MdOutlineLocalGasStation className="h-8 w-8" />}
              title={"Fuel Type"}
              stat={listing.fuelType}
            />
          </div>
          <div className="flex justify-self-start">
            <ListingCardBadge
              Icon={<TbManualGearbox className="h-8 w-8" />}
              title={"Gear Box"}
              stat={listing.transmission}
            />
          </div>
          <div className="flex justify-self-end">
            <ListingCardBadge
              Icon={<IoLocationOutline className="h-8 w-8" />}
              title={"Location"}
              stat={listing.city}
            />
          </div>
        </div>
        <div className="flex justify-end">
          <Button className="h-full max-h-12 rounded-3xl text-xl font-bold">
            Check out
          </Button>
        </div>
      </div>
    </div>
  );
};

export default CarListingCard;
