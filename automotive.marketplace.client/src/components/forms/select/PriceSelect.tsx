import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { SelectRootProps } from "@/types/ui/selectRootProps";

type PriceSelectProps = SelectRootProps & {
  label?: string;
  className?: string;
};

const PriceSelect = ({ label, className, ...props }: PriceSelectProps) => {
  const prices = [150, 300, 500];
  for (let i = 1000; i < 5000; i += 500) {
    prices.push(i);
  }
  for (let i = 5000; i < 10000; i += 1000) {
    prices.push(i);
  }
  for (let i = 10000; i <= 100000; i += 2500) {
    prices.push(i);
  }
  return (
    <Select {...props}>
      <SelectTrigger className={cn("flex w-full flex-row", className)}>
        <div className="grid grid-cols-1 justify-items-start">
          <label className="text-muted-foreground text-xs">{label}</label>
          <SelectValue placeholder="-" />
        </div>
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          {prices.map((price) => (
            <SelectItem
              key={price}
              value={price.toString()}
            >{`${price} €`}</SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
};

export default PriceSelect;
