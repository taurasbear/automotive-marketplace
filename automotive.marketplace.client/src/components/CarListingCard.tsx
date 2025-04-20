import EnginePill from "./EnginePill";
import { Button } from "./ui/button";

const CarListingCard = () => {
  return (
    <div className="bg-surface border-secondary-border grid w-full grid-cols-2 gap-8 border-1">
      <div className="flex flex-shrink-0 py-4">
        <img
          className="aspect-[4/3] object-cover"
          alt="Toyota Prius 2025"
          src="https://imgs.search.brave.com/_avFlFDyXU8SS34ve__STsLcC6LfrFsy76XnfAbI4Vo/rs:fit:860:0:0:0/g:ce/aHR0cHM6Ly9tZWRp/YS5nZXR0eWltYWdl/cy5jb20vaWQvNDU5/NDQ1ODUxL3Bob3Rv/L3RveW90YS1wcml1/cy5qcGc_cz02MTJ4/NjEyJnc9MCZrPTIw/JmM9OGRDdF9lSGxP/YzhMcUxEQllYME42/N0FpZFNNd2lRT0ZT/LVhzMUxYcnBjQT0"
        />
      </div>
      <div className="flex min-w-0 flex-grow flex-col justify-between pt-4 pr-4 pb-2">
        <div className="truncate">
          <p className="truncate font-sans text-xs">Used</p>
          <p className="font-sans text-xl">2013 BMW X6 M Base</p>
          <p className="font-sans text-xs">137 332 km</p>
          <p className="font-sans text-4xl font-bold">23 000 €</p>
        </div>
        <div className="grid grid-cols-2 justify-items-stretched gap-x-0 gap-y-4">
          <div className="flex justify-self-start">
            <EnginePill />
          </div>
          <div className="flex justify-self-end">
            <EnginePill />
          </div>
          <div className="flex justify-self-start">
            <EnginePill />
          </div>
          <div className="flex justify-self-end">
            <EnginePill />
          </div>
        </div>
        <div className="flex justify-end">
          <Button>Check out</Button>
        </div>
      </div>
    </div>
  );
};

export default CarListingCard;
