import {
  SelectValue,
  SelectTrigger,
  SelectContent,
  SelectItem,
  Select,
} from "@/components/ui/select";
import {
  CarConditionKey,
  CAR_CONDITION_OPTIONS,
} from "@/shared/constants/carConditions";

type UsedSelectProps = {
  onValueChange: (value: CarConditionKey) => void;
};

const UsedSelect = ({ onValueChange }: UsedSelectProps) => {
  return (
    <div>
      <Select onValueChange={onValueChange} defaultValue="newused">
        <SelectTrigger className="w-full min-w-3xs">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {CAR_CONDITION_OPTIONS.map(([conditionKey, conditionName]) => (
            <SelectItem value={conditionKey}>{conditionName}</SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
};

export default UsedSelect;
