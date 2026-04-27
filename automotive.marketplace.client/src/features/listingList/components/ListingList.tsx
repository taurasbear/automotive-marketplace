import { useSuspenseQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { Pagination } from "@/components/ui/Pagination";
import { getAllListingsOptions } from "../api/getAllListingsOptions";
import { GetAllListingsQuery } from "../types/GetAllListingsQuery";
import ListingCard from "./ListingCard";

type ListingListProps = {
  listingSearchQuery: GetAllListingsQuery;
};

const PAGE_SIZE = 20;

const ListingList = ({ listingSearchQuery }: ListingListProps) => {
  const [page, setPage] = useState(1);

  useEffect(() => {
    setPage(1);
  }, [listingSearchQuery]);

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
      <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
    </div>
  );
};

export default ListingList;

