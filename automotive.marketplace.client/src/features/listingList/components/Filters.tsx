import {
  ListingSearchParams,
  ListingSearchStateValues,
} from "@/features/search";
import {
  getSearchParams,
  getSearchValues,
} from "@/features/search/utils/listingSearchUtils";
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
    value: string,
  ) => {
    const updatedSearchValues = { ...searchValues, [key]: value };
    const updatedSearchParams = getSearchParams(updatedSearchValues);

    await onSearchParamChange(updatedSearchParams);
  };

  return (
    <div className="columns-1">
      <div className="bg-card p-4">
        <BasicFilters
          filters={{
            makeId: searchValues.makeId,
            isUsed: searchValues.isUsed,
            city: searchValues.city,
          }}
          onFilterChange={handleFilterChange}
        />
      </div>
      <div className="bg-background p-4">
        <ModelFilter />
      </div>
      <div className="bg-secondary p-4">
        <RangeFilters />
      </div>
    </div>
  );
};

export default Filters;
