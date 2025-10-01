import LocationCombobox from "@/components/forms/select/LocationCombobox";
import MakeSelect from "@/components/forms/select/MakeSelect";
import UsedSelect from "@/components/forms/select/UsedSelect";
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
    <div className="border-border bg-background grid grid-cols-1 space-y-0 divide-y-1 rounded-md border-1 p-0">
      <div>
        <MakeSelect
          isAllMakesEnabled={true}
          label="Make"
          className="shadow-non min-h-15 rounded-b-none border-0"
          value={filters.makeId}
          onValueChange={(value) => onFilterChange("makeId", value)}
        />
      </div>
      <div>
        <UsedSelect
          className="min-h-15 rounded-none border-0 shadow-none"
          value={filters.isUsed}
          onValueChange={(value) => onFilterChange("isUsed", value)}
        />
      </div>
      <div>
        <LocationCombobox
          className="min-h-15 rounded-t-none border-0 shadow-none"
          value={filters.city}
          onValueChange={(value) => onFilterChange("city", value)}
        />
      </div>
    </div>
  );
};

export default BasicFilters;
