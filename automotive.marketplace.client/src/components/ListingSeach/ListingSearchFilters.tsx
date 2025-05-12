import LocationCombobox from "./LocationCombobox";
import MakeSelect from "./MakeSelect";
import ModelSelect from "./ModelSelect";
import PriceSelect from "./PriceSelect";
import UsedSelect from "./UsedSelect";
import YearSelect from "./YearSelect";

const ListingSearchFilters = () => {
  return (
    <div className="flex flex-col">
      <div className="flex flex-row">
        <ModelSelect />
        <MakeSelect />
        <LocationCombobox />
      </div>
      <div className="flex flex-row">
        <UsedSelect />
        <YearSelect />
        <YearSelect />
        <PriceSelect />
        <PriceSelect />
      </div>
    </div>
  );
};

export default ListingSearchFilters;
