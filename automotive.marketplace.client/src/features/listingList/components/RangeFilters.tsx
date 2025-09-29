import PriceSelect from "@/features/search/components/PriceSelect";
import YearSelect from "@/features/search/components/YearSelect";

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
