import { getBodyTypesOptions } from "@/api/enum/getBodyTypesOptions";
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

type BodytypeSelectProps = React.ComponentProps<typeof SelectPrimitive.Root> & {
  className?: string;
};

const BodyTypeSelect = ({ className, ...props }: BodytypeSelectProps) => {
  const { data: bodyTypesQuery } = useQuery(getBodyTypesOptions);

  const bodyTypes = bodyTypesQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Select {...props}>
        <SelectTrigger className="w-full">
          <SelectValue placeholder="Sedan" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Body types</SelectLabel>
            {bodyTypes.map((body) => (
              <SelectItem key={body.bodyType} value={body.bodyType}>
                {body.bodyType}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default BodyTypeSelect;
