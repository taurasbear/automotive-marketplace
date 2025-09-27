import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
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
          <div className="grid grid-cols-1 justify-items-start">
            <label className="text-muted-foreground text-xs">{label}</label>
            <SelectValue placeholder="-" />
          </div>
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
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
