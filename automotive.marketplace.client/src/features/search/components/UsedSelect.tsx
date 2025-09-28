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

type UsedSelectProps = {
  onValueChange: (value: CarConditionKey) => void;
  className?: string;
};

const UsedSelect = ({ onValueChange, className }: UsedSelectProps) => {
  return (
    <div>
      <Select onValueChange={onValueChange} defaultValue="newused">
        <SelectTrigger className={cn(className, "w-full min-w-3xs")}>
          <div className="grid grid-cols-1 justify-items-start">
            <label className="text-muted-foreground text-xs">Used/New</label>
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
    </div>
  );
};

export default UsedSelect;
