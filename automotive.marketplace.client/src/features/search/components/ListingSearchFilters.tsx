import MakeSelect from "@/components/forms/MakeSelect";
import ModelSelect from "@/components/forms/ModelSelect";
import { UI_CONSTANTS } from "@/constants/uiConstants";
import { cn } from "@/lib/utils";
import { useState } from "react";
import { SearchStateValues } from "../types/searchStateValues";
import LocationCombobox from "./LocationCombobox";
import PriceSelect from "./PriceSelect";
import UsedSelect from "./UsedSelect";
import YearSelect from "./YearSelect";

type ListingSearchFiltersProps = {
  className?: string;
};

const ListingSearchFilters = ({ className }: ListingSearchFiltersProps) => {
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
        "bg-background overflow-hidden rounded-2xl border-1",
        className,
      )}
    >
      <div className="grid grid-cols-1 divide-y-1 border-b-1 sm:grid-cols-3 sm:divide-x-1 sm:divide-y-0">
        <div className="col-span-1">
          <MakeSelect
            className="min-h-15 border-0 shadow-none"
            label="Make"
            isAllMakesEnabled={true}
            value={searchValues.makeId}
            onValueChange={(value) => updateSearchValue("makeId", value)}
          />
        </div>
        <div className="col-span-1">
          <ModelSelect
            className="min-h-15 border-0 shadow-none"
            isAllModelsEnabled={true}
            defaultValue="all"
            label="Model"
            selectedMake={searchValues.makeId}
            value={searchValues.modelId}
            onValueChange={(value) => updateSearchValue("modelId", value)}
          />
        </div>
        <div className="col-span-1">
          <LocationCombobox
            className="min-h-15 border-0 shadow-none"
            value={searchValues.city}
            onValueChange={(value) => updateSearchValue("city", value)}
          />
        </div>
      </div>
      <div className="grid grid-cols-2 sm:grid-cols-6 sm:divide-x-1">
        <div className="col-span-2 border-b-1 sm:border-b-0">
          <UsedSelect
            className="min-h-15 border-0 shadow-none"
            value={searchValues.isUsed}
            defaultValue="newUsed"
            onValueChange={(value) => updateSearchValue("isUsed", value)}
          />
        </div>
        <div className="border-r-1 border-b-1 sm:border-b-0">
          <YearSelect
            className="min-h-15 border-0 shadow-none"
            label="From year"
            value={searchValues.yearFrom}
            onValueChange={(value) => updateSearchValue("yearFrom", value)}
          />
        </div>
        <div className="border-b-1 sm:border-b-0">
          <YearSelect
            className="min-h-15 border-0 shadow-none"
            label="To year"
            value={searchValues.yearTo}
            onValueChange={(value) => updateSearchValue("yearTo", value)}
          />
        </div>
        <div className="border-r-1">
          <PriceSelect
            className="min-h-15 border-0 shadow-none"
            label="From price"
            value={searchValues.priceFrom}
            onValueChange={(value) => updateSearchValue("priceFrom", value)}
          />
        </div>
        <div>
          <PriceSelect
            className="min-h-15 border-0 shadow-none"
            label="To price"
            value={searchValues.priceTo}
            onValueChange={(value) => updateSearchValue("priceTo", value)}
          />
        </div>
      </div>
    </div>
  );
};

export default ListingSearchFilters;
