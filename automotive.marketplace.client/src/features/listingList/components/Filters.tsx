import {
  getSearchParams,
  getSearchValues,
  ListingSearchParams,
  ListingSearchStateValues,
} from "@/features/search";

import BasicFilters from "./BasicFilters";
import ModelFilter from "./ModelFilter";
import RangeFilters from "./RangeFilters";

type FiltersProps = {
  searchParams: ListingSearchParams;
  onSearchParamChange: (searchParams: ListingSearchParams) => Promise<void>;
};

const Filters = ({ searchParams, onSearchParamChange }: FiltersProps) => {
  const searchValues = getSearchValues(searchParams);

  const handleFilterChange = async <K extends keyof ListingSearchStateValues>(
    key: K,
    value: string | string[],
  ) => {
    const updatedSearchValues = { ...searchValues, [key]: value };
    const updatedSearchParams = getSearchParams(updatedSearchValues);

    await onSearchParamChange(updatedSearchParams);
  };

  return (
    <div className="columns-1">
      <div className="bg-secondary dark:bg-card p-4">
        <BasicFilters
          filters={{
            makeId: searchValues.makeId,
            isUsed: searchValues.isUsed,
            city: searchValues.city,
          }}
          onFilterChange={handleFilterChange}
        />
      </div>
      {searchParams.makeId && (
        <div className="bg-background p-4">
          <ModelFilter
            makeId={searchValues.makeId}
            filteredModels={searchValues.models}
            onFilterChange={(value) => handleFilterChange("models", value)}
          />
        </div>
      )}
      <div className="bg-secondary dark:bg-card p-4">
        <RangeFilters />
      </div>
    </div>
  );
};

export default Filters;
