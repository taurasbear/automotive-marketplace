import { Route } from "@/app/routes/listings";
import MakeSelect from "@/components/forms/MakeSelect";
import { CarConditionKey } from "@/constants/carConditions";
import ListingList from "@/features/listingList/components/ListingList";
import UsedSelect from "@/features/search/components/UsedSelect";

const Listings = () => {
  const searchParams = Route.useSearch();

  return (
    <div className="flex w-full items-start justify-start space-x-24">
      <div className="border-border bg-secondary mt-64 flex-1 border-1 p-4">
        <div className="border-border space-y-4 rounded-2xl border-1 p-2">
          <MakeSelect
            isAllMakesEnabled={false}
            label="Make"
            className="min-h-15"
          />
          <UsedSelect
            onValueChange={function (value: CarConditionKey): void {
              throw new Error("Function not implemented.");
            }}
          />
        </div>
      </div>
      <ListingList listingSearchQuery={searchParams} />
    </div>
  );
};

export default Listings;
