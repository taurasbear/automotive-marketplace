import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Link } from "@tanstack/react-router";
import ListingSearchFilters from "./ListingSearchFilters";

type ListingSearchProps = {
  className?: string;
};

const ListingSearch = ({ className }: ListingSearchProps) => {
  return (
    <div
      className={cn(
        "bg-secondary border-border mx-auto mt-64 rounded-sm border-1 p-4",
        className,
      )}
    >
      <label className="block pb-5 text-2xl font-semibold">Look up</label>
      <ListingSearchFilters className="w-full" />
      <div className="flex justify-end pt-4">
        <Link to="/listings">
          <Button className="px-8 py-5 text-lg">Search</Button>
        </Link>
      </div>
    </div>
  );
};

export default ListingSearch;
