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
            value={
              searchValues.models[0] ?? UI_CONSTANTS.SELECT.ALL_MODELS.VALUE
            }
            onValueChange={(value) => updateSearchValue("models", [value])}
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
