import BasicSelect from "@/components/forms/select/BasicSelect";
import {
  getMileageRange,
  getPowerRange,
  getPriceRange,
  getYearRange,
} from "@/utils/rangeUtils";
import { ValueOf } from "type-fest";
import { RangeFilter } from "../types/rangeFilter";

type RangeFiltersProps = {
  filters: RangeFilter;
  onFilterChange: (
    key: keyof RangeFilter,
    value: ValueOf<RangeFilter>,
  ) => Promise<void>;
};

const RangeFilters = ({ filters, onFilterChange }: RangeFiltersProps) => {
  return (
    <div className="columns-1 space-y-4">
      <fieldset>
        <legend className="mb-2 text-sm font-normal">Year</legend>
        <div className="border-input bg-background flex rounded-md border-1">
          <BasicSelect
            options={getYearRange()}
            label="Min"
            className="min-h-12 rounded-r-none border-0 border-r-1 shadow-none"
            value={filters.minYear}
            onValueChange={(value) => onFilterChange("minYear", value)}
          />
          <BasicSelect
            options={getYearRange()}
            label="Max"
            className="min-h-12 rounded-l-none border-0 shadow-none"
            value={filters.maxYear}
            onValueChange={(value) => onFilterChange("maxYear", value)}
          />
        </div>
      </fieldset>
      <fieldset>
        <legend className="mb-2 text-sm font-normal">Price (€)</legend>
        <div className="border-input bg-background flex rounded-md border-1">
          <BasicSelect
            options={getPriceRange()}
            suffix="€"
            label="Min"
            className="min-h-12 rounded-r-none border-0 border-r-1 shadow-none"
            value={filters.minPrice}
            onValueChange={(value) => onFilterChange("minPrice", value)}
          />
          <BasicSelect
            options={getPriceRange()}
            suffix="€"
            label="Max"
            className="min-h-12 rounded-l-none border-0 shadow-none"
            value={filters.maxPrice}
            onValueChange={(value) => onFilterChange("maxPrice", value)}
          />
        </div>
      </fieldset>
      <fieldset>
        <legend className="mb-2 text-sm font-normal">Mileage (km)</legend>
        <div className="border-input bg-background flex rounded-md border-1">
          <BasicSelect
            options={getMileageRange()}
            suffix="km"
            label="Min"
            className="min-h-12 rounded-r-none border-0 border-r-1 shadow-none"
            value={filters.minMileage}
            onValueChange={(value) => onFilterChange("minMileage", value)}
          />
          <BasicSelect
            options={getMileageRange()}
            suffix="km"
            label="Max"
            className="min-h-12 rounded-l-none border-0 shadow-none"
            value={filters.maxMileage}
            onValueChange={(value) => onFilterChange("maxMileage", value)}
          />
        </div>
      </fieldset>
      <fieldset>
        <legend className="mb-2 text-sm font-normal">Power (kW)</legend>
        <div className="border-input bg-background flex rounded-md border-1">
          <BasicSelect
            options={getPowerRange()}
            suffix="kW"
            label="Min"
            className="min-h-12 rounded-r-none border-0 border-r-1 shadow-none"
            value={filters.minPower}
            onValueChange={(value) => onFilterChange("minPower", value)}
          />
          <BasicSelect
            options={getPowerRange()}
            suffix="kW"
            label="Max"
            className="min-h-12 rounded-l-none border-0 shadow-none"
            value={filters.maxPower}
            onValueChange={(value) => onFilterChange("maxPower", value)}
          />
        </div>
      </fieldset>
    </div>
  );
};

export default RangeFilters;
