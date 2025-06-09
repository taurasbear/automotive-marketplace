import LocationCombobox from "./LocationCombobox";
import MakeSelect from "./MakeSelect";
import ModelSelect from "./ModelSelect";
import PriceSelect from "./PriceSelect";
import UsedSelect from "./UsedSelect";
import YearSelect from "./YearSelect";

const ListingSearch = () => {
  return (
    <div className="grid grid-cols-6 grid-rows-2">
      <div className="col-span-2">
        <MakeSelect />
      </div>
      <div className="col-span-2">
        <ModelSelect />
      </div>
      <div className="col-span-2">
        <LocationCombobox />
      </div>
      <div className="col-span-2">
        <UsedSelect />
      </div>
      <div>
        <YearSelect />
      </div>
      <div>
        <YearSelect />
      </div>
      <div>
        <PriceSelect />
      </div>
      <div>
        <PriceSelect />
      </div>
    </div>
  );
};

export default ListingSearch;
