import { useSuspenseQuery } from "@tanstack/react-query";
import { getAllListingsOptions } from "../api/getAllListingsOptions";
import { GetAllListingsQuery } from "../types/GetAllListingsQuery";
import ListingCard from "./ListingCard";

type ListingListProps = {
  listingSearchQuery: GetAllListingsQuery;
};

const ListingList = ({ listingSearchQuery }: ListingListProps) => {
  const { data: listingsQuery } = useSuspenseQuery(
    getAllListingsOptions(listingSearchQuery),
  );

  const listings = listingsQuery.data;

  return (
    <div className="bg-background text-on-background flex w-204 flex-col gap-10">
      {listings.map((l) => (
        <ListingCard key={l.id} listing={l} />
      ))}
    </div>
  );
};

export default ListingList;
