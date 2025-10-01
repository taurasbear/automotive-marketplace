import MakeSelect from "@/components/forms/select/MakeSelect";
import ModelSelect from "@/components/forms/select/ModelSelect";
import { UI_CONSTANTS } from "@/constants/uiConstants";
import { cn } from "@/lib/utils";
import LocationCombobox from "../../../components/forms/select/LocationCombobox";
import PriceSelect from "../../../components/forms/select/PriceSelect";
import UsedSelect from "../../../components/forms/select/UsedSelect";
import YearSelect from "../../../components/forms/select/YearSelect";
import { ListingSearchStateValues } from "../types/listingSearchStateValues";

type ListingSearchFiltersProps = {
  searchValues: ListingSearchStateValues;
  updateSearchValue: <K extends keyof ListingSearchStateValues>(
    key: K,
    value: string | string[],
  ) => void;
  className?: string;
};

const ListingSearchFilters = ({
  searchValues,
  updateSearchValue,
  className,
}: ListingSearchFiltersProps) => {
  return (
    <div className={cn(className)}>
      <div className="border-input divide-input grid grid-cols-1 divide-y-1 border-b sm:grid-cols-3 sm:divide-x-1 sm:divide-y-0">
        <div className="col-span-1">
          <MakeSelect
            className="min-h-15 rounded-none rounded-tl-md rounded-tr-md border border-b-0 shadow-none sm:rounded-tr-none sm:border-r-0 sm:border-b-0"
            label="Make"
            isAllMakesEnabled={true}
            value={searchValues.makeId}
            onValueChange={(value) => updateSearchValue("makeId", value)}
          />
        </div>
        <div className="col-span-1">
          <ModelSelect
            className="min-h-15 rounded-none border-0 border-x shadow-none sm:border-x-0 sm:border-t"
            isAllModelsEnabled={true}
            defaultValue="all"
            label="Model"
            selectedMake={searchValues.makeId}
            value={
              searchValues.models[0] ?? UI_CONSTANTS.SELECT.ALL_MODELS.VALUE
            }
            onValueChange={(value) => updateSearchValue("models", [value])}
          />
        </div>
        <div className="col-span-1">
          <LocationCombobox
            className="min-h-15 rounded-none border-0 border-x shadow-none sm:rounded-tr-md sm:border-x-0 sm:border-t sm:border-r"
            value={searchValues.city}
            onValueChange={(value) => updateSearchValue("city", value)}
          />
        </div>
      </div>
      <div className="divide-input grid grid-cols-2 sm:grid-cols-6 sm:divide-x-1">
        <div className="col-span-2 border-b-1 sm:border-b-0">
          <UsedSelect
            className="min-h-15 rounded-none border-0 border-x shadow-none sm:rounded-bl-md sm:border-x-0 sm:border-b-1 sm:border-l-1"
            value={searchValues.isUsed}
            defaultValue="newUsed"
            onValueChange={(value) => updateSearchValue("isUsed", value)}
          />
        </div>
        <div className="border-r-1 border-b-1 sm:border-b-0">
          <YearSelect
            className="min-h-15 rounded-none border-0 border-l shadow-none sm:border-b-1 sm:border-l-0"
            label="Min year"
            value={searchValues.minYear}
            onValueChange={(value) => updateSearchValue("minYear", value)}
          />
        </div>
        <div className="border-b-1 sm:border-b-0">
          <YearSelect
            className="min-h-15 rounded-none border-0 border-r shadow-none sm:border-r-0 sm:border-b-1"
            label="Max year"
            value={searchValues.maxYear}
            onValueChange={(value) => updateSearchValue("maxYear", value)}
          />
        </div>
        <div className="border-r-1">
          <PriceSelect
            className="min-h-15 rounded-none rounded-bl-md border-0 border-b border-l shadow-none sm:rounded-bl-none sm:border-l-0"
            label="Min price"
            value={searchValues.minPrice}
            onValueChange={(value) => updateSearchValue("minPrice", value)}
          />
        </div>
        <div>
          <PriceSelect
            className="min-h-15 rounded-none rounded-br-md border-0 border-r border-b shadow-none"
            label="Max price"
            value={searchValues.maxPrice}
            onValueChange={(value) => updateSearchValue("maxPrice", value)}
          />
        </div>
      </div>
    </div>
  );
};

export default ListingSearchFilters;
