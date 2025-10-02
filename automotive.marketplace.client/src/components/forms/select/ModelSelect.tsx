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
import { UI_CONSTANTS } from "@/constants/uiConstants";
import { cn } from "@/lib/utils";
import { SelectRootProps } from "@/types/ui/selectRootProps";
import { useQuery } from "@tanstack/react-query";

type ModelSelectProps = SelectRootProps & {
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
    enabled:
      selectedMake !== UI_CONSTANTS.SELECT.ALL_MAKES.VALUE && !!selectedMake,
  });

  const models = modelsQuery?.data || [];

  return (
    <Select {...props}>
      <SelectTrigger className={cn("w-full", className)} aria-label={label}>
        <div className="grid grid-cols-1 justify-items-start">
          <span className="text-muted-foreground text-xs">{label}</span>
          <SelectValue placeholder="Auris" />
        </div>
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          <SelectLabel>Models</SelectLabel>
          {!isAllModelsEnabled || (
            <SelectItem value={UI_CONSTANTS.SELECT.ALL_MODELS.VALUE}>
              {UI_CONSTANTS.SELECT.ALL_MODELS.LABEL}
            </SelectItem>
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
