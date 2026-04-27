import { useSuspenseQuery } from "@tanstack/react-query";
import { Pagination } from "@/components/ui/Pagination";
import { getAllListingsOptions } from "../api/getAllListingsOptions";
import { GetAllListingsQuery } from "../types/GetAllListingsQuery";
import ListingCard from "./ListingCard";

type ListingListProps = {
  listingSearchQuery: GetAllListingsQuery;
  page: number;
  onPageChange: (page: number) => void;
};

const PAGE_SIZE = 20;

const ListingList = ({ listingSearchQuery, page, onPageChange }: ListingListProps) => {
  const { data: listingsQuery } = useSuspenseQuery(
    getAllListingsOptions({ ...listingSearchQuery, page, pageSize: PAGE_SIZE }),
  );

  const listings = listingsQuery.data.items;
  const totalPages = listingsQuery.data.totalPages;

  return (
    <div className="bg-background text-on-background flex w-204 flex-col gap-10">
      {listings.map((l) => (
        <ListingCard key={l.id} listing={l} />
      ))}
      <Pagination page={page} totalPages={totalPages} onPageChange={onPageChange} />
    </div>
  );
};

export default ListingList;

