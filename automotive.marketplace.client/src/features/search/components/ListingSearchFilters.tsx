import BasicSelect from "@/components/forms/select/BasicSelect";
import MakeSelect from "@/components/forms/select/MakeSelect";
import ModelSelect from "@/components/forms/select/ModelSelect";
import { UI_CONSTANTS } from "@/constants/uiConstants";
import { cn } from "@/lib/utils";
import { getPriceRange, getYearRange } from "@/utils/rangeUtils";
import { useTranslation } from "react-i18next";
import LocationCombobox from "../../../components/forms/select/LocationCombobox";
import UsedSelect from "../../../components/forms/select/UsedSelect";
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
  const { t } = useTranslation("listings");
  return (
    <div className={cn("rounded-md", className)}>
      <div className="border-input divide-input grid grid-cols-1 divide-y-1 border-b sm:grid-cols-3 sm:divide-x-1 sm:divide-y-0">
        <div className="col-span-1">
          <MakeSelect
            className="min-h-15 rounded-none rounded-tl-md rounded-tr-md border border-b-0 shadow-none sm:rounded-tr-none sm:border-r-0 sm:border-b-0"
            label={t("filters.make")}
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
            label={t("filters.model")}
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
            value={searchValues.municipalityId}
            onValueChange={(value) =>
              updateSearchValue("municipalityId", value)
            }
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
          <BasicSelect
            options={getYearRange()}
            className="min-h-15 rounded-none border-0 border-l shadow-none sm:border-b-1 sm:border-l-0"
            label={t("filters.minYear")}
            value={searchValues.minYear}
            onValueChange={(value) => updateSearchValue("minYear", value)}
          />
        </div>
        <div className="border-b-1 sm:border-b-0">
          <BasicSelect
            options={getYearRange()}
            className="min-h-15 rounded-none border-0 border-r shadow-none sm:border-r-0 sm:border-b-1"
            label={t("filters.maxYear")}
            value={searchValues.maxYear}
            onValueChange={(value) => updateSearchValue("maxYear", value)}
          />
        </div>
        <div className="border-r-1">
          <BasicSelect
            options={getPriceRange()}
            suffix="€"
            className="min-h-15 rounded-none rounded-bl-md border-0 border-b border-l shadow-none sm:rounded-bl-none sm:border-l-0"
            label={t("filters.minPrice")}
            value={searchValues.minPrice}
            onValueChange={(value) => updateSearchValue("minPrice", value)}
          />
        </div>
        <div>
          <BasicSelect
            options={getPriceRange()}
            suffix="€"
            className="min-h-15 rounded-none rounded-br-md border-0 border-r border-b shadow-none"
            label={t("filters.maxPrice")}
            value={searchValues.maxPrice}
            onValueChange={(value) => updateSearchValue("maxPrice", value)}
          />
        </div>
      </div>
    </div>
  );
};

export default ListingSearchFilters;
