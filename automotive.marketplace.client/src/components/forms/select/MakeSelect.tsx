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
import { UI_CONSTANTS } from "@/constants/uiConstants";
import { cn } from "@/lib/utils";
import { SelectRootProps } from "@/types/ui/selectRootProps";
import { useQuery } from "@tanstack/react-query";

type MakeSelectProps = SelectRootProps & {
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
          {!isAllMakesEnabled || (
            <SelectItem value={UI_CONSTANTS.SELECT.ALL_MAKES.VALUE}>
              {UI_CONSTANTS.SELECT.ALL_MAKES.LABEL}
            </SelectItem>
          )}
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
