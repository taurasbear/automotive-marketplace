import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../ui/select";

const PriceSelect = () => {
  const prices = [150, 300, 500];
  for (let i = 1000; i <= 5000; i += 500) {
    prices.push(i);
  }
  return (
    <div>
      <Select>
        <SelectTrigger className="flex w-full flex-row">
          <SelectValue placeholder="-" />
        </SelectTrigger>
        <SelectContent>
          {prices.map((price) => (
            <SelectItem
              key={price}
              value={price.toString()}
            >{`${price} €`}</SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
};

export default PriceSelect;
