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

const years = Array.from({ length: 2025 - 1950 + 1 }, (_, i) => 1950 + i);

type YearSelectProps = {
  label: string;
  onValueChange: (value: string) => void;
  className?: string;
};

const YearSelect = ({ label, onValueChange, className }: YearSelectProps) => {
  return (
    <div>
      <Select onValueChange={onValueChange}>
        <SelectTrigger className={cn(className, "w-full")}>
          <SelectValue placeholder="-" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>{label}</SelectLabel>
            {years.map((year) => (
              <SelectItem key={year} value={year.toString()}>
                {year}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default YearSelect;
