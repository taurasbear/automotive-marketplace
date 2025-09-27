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

type UsedSelectProps = {
  onValueChange: (value: CarConditionKey) => void;
};

const UsedSelect = ({ onValueChange }: UsedSelectProps) => {
  return (
    <div>
      <Select onValueChange={onValueChange} defaultValue="newused">
        <SelectTrigger className="w-full min-w-3xs border-0 shadow-none">
          <SelectValue />
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
