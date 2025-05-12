import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../ui/select";

const years = Array.from({ length: 2025 - 1950 + 1 }, (_, i) => 1950 + i);

const YearSelect = () => {
  return (
    <div>
      <Select>
        <SelectTrigger>
          <SelectValue placeholder="-" />
        </SelectTrigger>
        <SelectContent>
          {years.map((year) => (
            <SelectItem key={year} value={year.toString()}>
              {year}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
};

export default YearSelect;
