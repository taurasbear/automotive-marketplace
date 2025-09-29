import { Route } from "@/app/routes/listings";
import MakeSelect from "@/components/forms/MakeSelect";
import { CarConditionKey } from "@/constants/carConditions";
import { getAllListingsOptions, ListingCard } from "@/features/listingList";
import UsedSelect from "@/features/search/components/UsedSelect";
import { useSuspenseQuery } from "@tanstack/react-query";

const Listings = () => {
  const searchParams = Route.useSearch();

  const { data: listingsQuery } = useSuspenseQuery(
    getAllListingsOptions(searchParams),
  );

  const listings = listingsQuery.data;

  return (
    <div className="flex w-full items-start justify-start space-x-24">
      <div className="border-border bg-secondary mt-64 flex-1 border-1 p-4">
        <span>Im right here</span>
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
      <div className="bg-background text-on-background flex w-188 flex-col gap-10">
        {listings.map((l) => (
          <ListingCard key={l.id} listing={l} />
        ))}
      </div>
    </div>
  );
};

export default Listings;
