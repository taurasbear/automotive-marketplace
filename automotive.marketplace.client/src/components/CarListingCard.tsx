const CarListingCard = () => {
    return (
        <div className="flex flex-row gap-2 w-full bg-surface">
            <div className="w-40 h-32 flex-shrink-0">
                <img
                    alt="Toyota Prius 2025"
                    src="https://imgs.search.brave.com/_avFlFDyXU8SS34ve__STsLcC6LfrFsy76XnfAbI4Vo/rs:fit:860:0:0:0/g:ce/aHR0cHM6Ly9tZWRp/YS5nZXR0eWltYWdl/cy5jb20vaWQvNDU5/NDQ1ODUxL3Bob3Rv/L3RveW90YS1wcml1/cy5qcGc_cz02MTJ4/NjEyJnc9MCZrPTIw/JmM9OGRDdF9lSGxP/YzhMcUxEQllYME42/N0FpZFNNd2lRT0ZT/LVhzMUxYcnBjQT0"
                    className="object-cover h-full w-full"
                />
            </div>
            <div className="flex flex-col flex-grow justify-between min-w-0">
                <div className="flex flex-row justify-between w-full">
                    <div className="truncate">
                        <p className="truncate font-sans font-semibold">Toyota prius</p>
                        <p className="font-sans">2025</p>
                    </div>
                    <p className="flex-shrink-0 font-sans font-semibold">9000 eur</p>
                </div>
                <div className="flex flex-row gap-2 flex-wrap">
                    <p>Benzinas/Elektra</p>
                    <p>Automatinė</p>
                    <p>73 kW</p>
                    <p>83 000 km</p>
                </div>
            </div>
        </div>
    );
}

export default CarListingCard;