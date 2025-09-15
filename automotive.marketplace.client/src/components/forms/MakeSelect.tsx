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
  className?: string;
  isAllMakesEnabled: boolean;
};

const MakeSelect = ({
  className,
  isAllMakesEnabled,
  ...props
}: MakeSelectProps) => {
  const { data: makesQuery } = useQuery(getAllMakesOptions);

  const makes = makesQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Select {...props}>
        <SelectTrigger className="w-full">
          <SelectValue placeholder="Make" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Makes</SelectLabel>
            {!isAllMakesEnabled || (
              <SelectItem value="all">All makes</SelectItem>
            )}
            {makes.map((make) => (
              <SelectItem key={make.id} value={make.id}>
                {make.name}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default MakeSelect;
