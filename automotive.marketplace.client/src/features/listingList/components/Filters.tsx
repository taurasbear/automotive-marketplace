import {
  ListingSearchParams,
  mapFilterValuesToSearchParams,
  mapSearchParamsToFilterValues,
} from "@/features/search";
import { UI_CONSTANTS } from "@/constants/uiConstants";

import { ListingFilterStateValues } from "../types/listingFilterStateValues";
import BasicFilters from "./BasicFilters";
import ModelFilter from "./ModelFilter";
import RangeFilters from "./RangeFilters";

type FiltersProps = {
  searchParams: ListingSearchParams;
  onSearchParamChange: (searchParams: ListingSearchParams) => Promise<void>;
};

const Filters = ({ searchParams, onSearchParamChange }: FiltersProps) => {
  const filterValues = mapSearchParamsToFilterValues(searchParams);

  const handleFilterChange = async <K extends keyof ListingFilterStateValues>(
    key: K,
    value: string | string[],
  ) => {
    let updatedFilterValues = { ...filterValues, [key]: value };
    if (key === "makeId" && value === UI_CONSTANTS.SELECT.ALL_MAKES.VALUE) {
      updatedFilterValues = { ...updatedFilterValues, models: [] };
    }
    const updatedSearchParams =
      mapFilterValuesToSearchParams(updatedFilterValues);

    await onSearchParamChange(updatedSearchParams);
  };

  return (
    <div className="columns-1">
      <div className="bg-secondary dark:bg-card p-4">
        <BasicFilters
          filters={{
            makeId: filterValues.makeId,
            isUsed: filterValues.isUsed,
            municipalityId: filterValues.municipalityId,
          }}
          onFilterChange={handleFilterChange}
        />
      </div>
      {searchParams.makeId && (
        <div className="bg-background p-4">
          <ModelFilter
            makeId={filterValues.makeId}
            filteredModels={filterValues.models}
            onFilterChange={(value) => handleFilterChange("models", value)}
          />
        </div>
      )}
      <div className="bg-secondary dark:bg-card p-4">
        <RangeFilters
          filters={{
            minYear: filterValues.minYear,
            maxYear: filterValues.maxYear,
            minPrice: filterValues.minPrice,
            maxPrice: filterValues.maxPrice,
            minMileage: filterValues.minMileage,
            maxMileage: filterValues.maxMileage,
            minPower: filterValues.minPower,
            maxPower: filterValues.maxPower,
          }}
          onFilterChange={handleFilterChange}
        />
      </div>
    </div>
  );
};

export default Filters;
