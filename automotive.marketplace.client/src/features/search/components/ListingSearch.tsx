import MakeSelect from "@/components/forms/MakeSelect";
import ModelSelect from "@/components/forms/ModelSelect";
import { Button } from "@/components/ui/button";
import { getSearchParamFromValue } from "@/features/listing/utils/listingSearchUtils";
import { cn } from "@/lib/utils";
import { Link } from "@tanstack/react-router";
import { useState } from "react";
import { SearchParams } from "../types/searchParams";
import LocationCombobox from "./LocationCombobox";
import PriceSelect from "./PriceSelect";
import UsedSelect from "./UsedSelect";
import YearSelect from "./YearSelect";

type ListingSearchProps = {
  className?: string;
};

const ListingSearch = ({ className }: ListingSearchProps) => {
  const [searchParams, setSearchParams] = useState<SearchParams>({});

  const updateSearchParam = <K extends keyof SearchParams>(
    key: K,
    value: string,
  ) => {
    const searchParam = getSearchParamFromValue(key, value);
    setSearchParams((prev) => ({ ...prev, [key]: searchParam }));
  };

  return (
    <div className={cn(className)}>
      <div className="bg-background overflow-hidden rounded-2xl border-1">
        <div className="grid grid-cols-1 divide-y-1 border-b-1 sm:grid-cols-3 sm:divide-x-1 sm:divide-y-0">
          <div className="col-span-1">
            <MakeSelect
              className="min-h-15 border-0 shadow-none"
              isAllMakesEnabled={true}
              defaultValue="all"
              onValueChange={(value) => updateSearchParam("makeId", value)}
            />
          </div>
          <div className="col-span-1">
            <ModelSelect
              className="min-h-15 border-0 shadow-none"
              isAllModelsEnabled={true}
              defaultValue="all"
              selectedMake={searchParams.makeId}
              onValueChange={(value) => updateSearchParam("modelId", value)}
            />
          </div>
          <div className="col-span-1">
            <LocationCombobox
              className="min-h-15 border-0 shadow-none"
              selectedLocation={searchParams.city}
              onValueChange={(value) => updateSearchParam("city", value)}
            />
          </div>
        </div>
        <div className="grid grid-cols-2 sm:grid-cols-6 sm:divide-x-1">
          <div className="col-span-2 border-b-1 sm:border-b-0">
            <UsedSelect
              className="min-h-15 border-0 shadow-none"
              onValueChange={(value) => updateSearchParam("isUsed", value)}
            />
          </div>
          <div className="border-r-1 border-b-1 sm:border-b-0">
            <YearSelect
              className="min-h-15 border-0 shadow-none"
              label="From year"
              onValueChange={(value) => updateSearchParam("yearFrom", value)}
            />
          </div>
          <div className="border-b-1 sm:border-b-0">
            <YearSelect
              className="min-h-15 border-0 shadow-none"
              label="To year"
              onValueChange={(value) => updateSearchParam("yearTo", value)}
            />
          </div>
          <div className="border-r-1">
            <PriceSelect
              className="min-h-15 border-0 shadow-none"
              label="From price"
              onValueChange={(value) => updateSearchParam("priceFrom", value)}
            />
          </div>
          <div>
            <PriceSelect
              className="min-h-15 border-0 shadow-none"
              label="To price"
              onValueChange={(value) => updateSearchParam("priceTo", value)}
            />
          </div>
        </div>
      </div>
      <div className="flex justify-end pt-4">
        <Link to="/listings" search={searchParams}>
          <Button className="px-8 py-5 text-lg">Search</Button>
        </Link>
      </div>
    </div>
  );
};

export default ListingSearch;
