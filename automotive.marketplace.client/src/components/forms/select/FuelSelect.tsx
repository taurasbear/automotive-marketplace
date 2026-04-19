import { getAllFuelsOptions } from "@/api/enum/getAllFuelsOptions";
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
  const { data: fuelsQuery } = useQuery(getAllFuelsOptions);

  const fuels = fuelsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Select {...props}>
        <SelectTrigger className="w-full">
          <SelectValue placeholder="Petrol" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Fuel types</SelectLabel>
            {fuels.map((fuel) => (
              <SelectItem key={fuel.id} value={fuel.id}>
                {fuel.name}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default FuelSelect;
