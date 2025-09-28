import { getAllMakesOptions } from "@/api/make/getAllMakesOptions";
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
import * as SelectPrimitive from "@radix-ui/react-select";
import { useQuery } from "@tanstack/react-query";

type MakeSelectProps = React.ComponentProps<typeof SelectPrimitive.Root> & {
  isAllMakesEnabled: boolean;
  label?: string;
  className?: string;
};

const MakeSelect = ({
  isAllMakesEnabled,
  label,
  className,
  ...props
}: MakeSelectProps) => {
  const { data: makesQuery } = useQuery(getAllMakesOptions);

  const makes = makesQuery?.data || [];

  return (
    <Select {...props}>
      <SelectTrigger className={cn("w-full", className)}>
        <div className="grid grid-cols-1 justify-items-start">
          <label className="text-muted-foreground text-xs">{label}</label>
          <SelectValue placeholder="Toyota" />
        </div>
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          <SelectLabel>Makes</SelectLabel>
          {!isAllMakesEnabled || <SelectItem value="all">All makes</SelectItem>}
          {makes.map((make) => (
            <SelectItem key={make.id} value={make.id}>
              {make.name}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
};

export default MakeSelect;
