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

type BasicSelectProps = SelectRootProps & {
  label?: string;
  options: string[];
  suffix?: string;
  className?: string;
};

const BasicSelect = ({
  label,
  options,
  suffix,
  className,
  ...props
}: BasicSelectProps) => {
  return (
    <Select {...props}>
      <SelectTrigger
        className={cn("flex w-full flex-row", className)}
        aria-label={label}
      >
        <div className="grid grid-cols-1 justify-items-start">
          <span className="text-muted-foreground text-xs">{label}</span>
          <SelectValue placeholder="-" />
        </div>
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          {options.map((value) => (
            <SelectItem key={value} value={value}>
              {suffix ? `${value} ${suffix}` : value}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
};

export default BasicSelect;
