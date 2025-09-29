import { getModelsByMakeIdOptions } from "@/api/model/getModelsByMakeIdOptions";
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

type ModelSelectProps = React.ComponentProps<typeof SelectPrimitive.Root> & {
  selectedMake?: string;
  isAllModelsEnabled: boolean;
  label?: string;
  className?: string;
};

const ModelSelect = ({
  selectedMake,
  isAllModelsEnabled,
  label,
  className,
  ...props
}: ModelSelectProps) => {
  const { data: modelsQuery } = useQuery({
    ...getModelsByMakeIdOptions({ makeId: selectedMake! }),
    enabled: !!selectedMake,
  });

  const models = modelsQuery?.data || [];

  return (
    <Select {...props}>
      <SelectTrigger className={cn("w-full", className)}>
        <div className="grid grid-cols-1 justify-items-start">
          <label className="text-muted-foreground text-xs">{label}</label>
          <SelectValue placeholder="Auris" />
        </div>
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          <SelectLabel>Models</SelectLabel>
          {!isAllModelsEnabled || (
            <SelectItem value="all">All models</SelectItem>
          )}
          {models.map((model) => (
            <SelectItem key={model.id} value={model.id}>
              {model.name}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
};

export default ModelSelect;
