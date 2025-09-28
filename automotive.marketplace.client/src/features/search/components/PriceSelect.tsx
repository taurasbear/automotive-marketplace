import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";

type PriceSelectProps = {
  label: string;
  onValueChange: (value: string) => void;
  className?: string;
};

const PriceSelect = ({ label, onValueChange, className }: PriceSelectProps) => {
  const prices = [150, 300, 500];
  for (let i = 1000; i <= 5000; i += 500) {
    prices.push(i);
  }
  return (
    <div>
      <Select onValueChange={onValueChange}>
        <SelectTrigger className={cn(className, "flex w-full flex-row")}>
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
    </div>
  );
};

export default PriceSelect;
