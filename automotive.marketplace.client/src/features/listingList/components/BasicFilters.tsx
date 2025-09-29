import MakeSelect from "@/components/forms/MakeSelect";
import LocationCombobox from "@/features/search/components/LocationCombobox";
import UsedSelect from "@/features/search/components/UsedSelect";
import type { ValueOf } from "type-fest";
import { BasicFilter } from "../types/basicFilter";

type BasicFiltersProps = {
  filters: BasicFilter;
  onFilterChange: (
    key: keyof BasicFilter,
    value: ValueOf<BasicFilter>,
  ) => Promise<void>;
};

const BasicFilters = ({ filters, onFilterChange }: BasicFiltersProps) => {
  return (
    <div className="border-border bg-background grid grid-cols-1 space-y-0 divide-y-1 rounded-2xl border-1 p-0">
      <div>
        <MakeSelect
          isAllMakesEnabled={true}
          label="Make"
          className="min-h-15 border-0 bg-transparent shadow-none"
          value={filters.makeId}
          onValueChange={(value) => onFilterChange("makeId", value)}
        />
      </div>
      <div>
        <UsedSelect
          className="min-h-15 border-0 shadow-none"
          value={filters.isUsed}
          onValueChange={(value) => onFilterChange("isUsed", value)}
        />
      </div>
      <div>
        <LocationCombobox
          className="min-h-15 border-0 shadow-none"
          value={filters.city}
          onValueChange={(value) => onFilterChange("city", value)}
        />
      </div>
    </div>
  );
};

export default BasicFilters;
