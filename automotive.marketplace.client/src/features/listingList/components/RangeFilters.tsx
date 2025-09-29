import PriceSelect from "@/components/forms/select/PriceSelect";
import YearSelect from "@/components/forms/select/YearSelect";

const RangeFilters = () => {
  return (
    <div>
      <YearSelect
        label={""}
        onValueChange={function (value: string): void {
          throw new Error("Function not implemented.");
        }}
      />
      <YearSelect
        label={""}
        onValueChange={function (value: string): void {
          throw new Error("Function not implemented.");
        }}
      />
      <PriceSelect
        label={""}
        onValueChange={function (value: string): void {
          throw new Error("Function not implemented.");
        }}
      />
      <PriceSelect
        label={""}
        onValueChange={function (value: string): void {
          throw new Error("Function not implemented.");
        }}
      />
    </div>
  );
};

export default RangeFilters;
