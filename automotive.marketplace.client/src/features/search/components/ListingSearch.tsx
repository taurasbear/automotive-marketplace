import { Button } from "@/components/ui/button";
import { UI_CONSTANTS } from "@/constants/uiConstants";
import { cn } from "@/lib/utils";
import { Link } from "@tanstack/react-router";
import { useState } from "react";
import { SearchStateValues } from "../types/searchStateValues";
import { getSearchParams } from "../utils/listingSearchUtils";
import ListingSearchFilters from "./ListingSearchFilters";

type ListingSearchProps = {
  className?: string;
};

const ListingSearch = ({ className }: ListingSearchProps) => {
  const [searchValues, setSearchValues] = useState<SearchStateValues>({
    makeId: UI_CONSTANTS.SELECT.ALL_MAKES.VALUE,
    modelId: UI_CONSTANTS.SELECT.ALL_MODELS.VALUE,
    city: UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE,
    isUsed: "newUsed",
  });

  const updateSearchValue = <K extends keyof SearchStateValues>(
    key: K,
    value: string,
  ) => {
    setSearchValues((prev) => ({ ...prev, [key]: value }));
  };

  return (
    <div
      className={cn(
        "bg-secondary border-border mx-auto mt-64 rounded-sm border-1 p-4",
        className,
      )}
    >
      <label className="block pb-5 text-2xl font-semibold">Look up</label>
      <ListingSearchFilters
        searchValues={searchValues}
        updateSearchValue={updateSearchValue}
        className="w-full"
      />
      <div className="flex justify-end pt-4">
        <Link to="/listings" search={() => getSearchParams(searchValues)}>
          <Button className="px-8 py-5 text-lg">Search</Button>
        </Link>
      </div>
    </div>
  );
};

export default ListingSearch;
