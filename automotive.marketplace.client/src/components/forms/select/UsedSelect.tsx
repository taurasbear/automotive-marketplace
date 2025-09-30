import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  CAR_CONDITION_OPTIONS,
  CarConditionKey,
} from "@/constants/carConditions";
import { cn } from "@/lib/utils";
import { SelectRootProps } from "@/types/ui/selectRootProps";

type UsedSelectProps = Omit<SelectRootProps, "value" | "onValueChange"> & {
  value: CarConditionKey;
  defaultValue?: CarConditionKey;
  onValueChange: (value: CarConditionKey) => void;
  className?: string;
};

const UsedSelect = ({
  value,
  defaultValue,
  onValueChange,
  className,
}: UsedSelectProps) => {
  return (
    <Select
      value={value}
      onValueChange={onValueChange}
      defaultValue={defaultValue}
    >
      <SelectTrigger className={cn("w-full min-w-3xs", className)}>
        <div className="grid grid-cols-1 justify-items-start">
          <span className="text-muted-foreground text-xs">Used/New</span>
          <SelectValue />
        </div>
      </SelectTrigger>
      <SelectContent>
        {CAR_CONDITION_OPTIONS.map(([conditionKey, conditionName]) => (
          <SelectItem key={conditionKey} value={conditionKey}>
            {conditionName}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
};

export default UsedSelect;
