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
import * as SelectPrimitive from "@radix-ui/react-select";
import { useQuery } from "@tanstack/react-query";

type FuelSelectProps = React.ComponentProps<typeof SelectPrimitive.Root> & {
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
