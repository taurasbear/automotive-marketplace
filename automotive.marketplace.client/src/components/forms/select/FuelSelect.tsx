import { getFuelTypesOptions } from "@/api/enum/getFuelTypesOptions";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { SelectRootProps } from "@/types/ui/selectRootProps";
import { useQuery } from "@tanstack/react-query";

type FuelSelectProps = SelectRootProps & {
  className?: string;
};

const FuelSelect = ({ className, ...props }: FuelSelectProps) => {
  const { data: fuelTypesQuery } = useQuery(getFuelTypesOptions);

  const fuelTypes = fuelTypesQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Select {...props}>
        <SelectTrigger className="w-full">
          <SelectValue placeholder="Petrol" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Fuel types</SelectLabel>
            {fuelTypes.map((fuel) => (
              <SelectItem key={fuel.fuelType} value={fuel.fuelType}>
                {fuel.fuelType}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default FuelSelect;
