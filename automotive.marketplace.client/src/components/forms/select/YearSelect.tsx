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

const years = Array.from(
  { length: new Date().getUTCFullYear() - 1950 + 1 },
  (_, i) => 1950 + i,
);

type YearSelectProps = SelectRootProps & {
  label: string;
  className?: string;
};

const YearSelect = ({ label, className, ...props }: YearSelectProps) => {
  return (
    <Select {...props}>
      <SelectTrigger className={cn("w-full", className)} aria-label={label}>
        <div className="grid grid-cols-1 justify-items-start">
          <span className="text-muted-foreground text-xs">{label}</span>
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
  );
};

export default YearSelect;
