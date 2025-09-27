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
  className?: string;
  selectedMake?: string;
  isAllModelsEnabled: boolean;
};

const ModelSelect = ({
  className,
  selectedMake,
  isAllModelsEnabled,
  ...props
}: ModelSelectProps) => {
  const { data: modelsQuery } = useQuery({
    ...getModelsByMakeIdOptions({ makeId: selectedMake! }),
    enabled: !!selectedMake,
  });

  const models = modelsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Select {...props}>
        <SelectTrigger className="w-full border-0 shadow-none">
          <SelectValue placeholder="Auris" />
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
    </div>
  );
};

export default ModelSelect;
