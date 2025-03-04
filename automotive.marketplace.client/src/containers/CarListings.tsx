import CarListingCard from "../components/CarListingCard";

const CarListings = () => {
    return (
        <div className="flex flex-row justify-center">
            <div className="flex flex-col gap-10 justify-end w-[39rem] bg-mint-500">
                <CarListingCard />
                <CarListingCard />
            </div>
        </div>
    );
}

export default CarListings;