import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  SelectGroup,
  SelectLabel,
} from "@/components/ui/select";

type PriceSelectProps = {
  label: string;
  onValueChange: (value: string) => void;
};

const PriceSelect = ({ label, onValueChange }: PriceSelectProps) => {
  const prices = [150, 300, 500];
  for (let i = 1000; i <= 5000; i += 500) {
    prices.push(i);
  }
  return (
    <div>
      <Select onValueChange={onValueChange}>
        <SelectTrigger className="flex w-full flex-row">
          <SelectValue placeholder="-" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>{label}</SelectLabel>
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
