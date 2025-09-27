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
      <div className="grid grid-cols-6 grid-rows-2">
        <div className="col-span-2">
          <MakeSelect
            isAllMakesEnabled={true}
            defaultValue="all"
            onValueChange={(value) => updateSearchParam("makeId", value)}
          />
        </div>
        <div className="col-span-2">
          <ModelSelect
            isAllModelsEnabled={true}
            defaultValue="all"
            selectedMake={searchParams.makeId}
            onValueChange={(value) => updateSearchParam("modelId", value)}
          />
        </div>
        <div className="col-span-2">
          <LocationCombobox
            selectedLocation={searchParams.city}
            onValueChange={(value) => updateSearchParam("city", value)}
          />
        </div>
        <div className="col-span-2">
          <UsedSelect
            onValueChange={(value) => updateSearchParam("isUsed", value)}
          />
        </div>
        <div>
          <YearSelect
            label="From year"
            onValueChange={(value) => updateSearchParam("yearFrom", value)}
          />
        </div>
        <div>
          <YearSelect
            label="To year"
            onValueChange={(value) => updateSearchParam("yearTo", value)}
          />
        </div>
        <div>
          <PriceSelect
            label="From price"
            onValueChange={(value) => updateSearchParam("priceFrom", value)}
          />
        </div>
        <div>
          <PriceSelect
            label="To price"
            onValueChange={(value) => updateSearchParam("priceTo", value)}
          />
        </div>
      </div>
      <div className="flex justify-end pt-2">
        <Link to="/listings" search={searchParams}>
          <Button>Search</Button>
        </Link>
      </div>
    </div>
  );
};

export default ListingSearch;
