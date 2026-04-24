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
import { useTranslation } from "react-i18next";

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
  const { t } = useTranslation("common");

  const conditionKeys: Record<string, string> = {
    new: "select.conditionNew",
    used: "select.conditionUsed",
    newUsed: "select.conditionUsedAndNew",
  };

  return (
    <Select
      value={value}
      onValueChange={onValueChange}
      defaultValue={defaultValue}
    >
      <SelectTrigger
        className={cn("w-full", className)}
        aria-label="Used, new or both"
      >
        <div className="grid grid-cols-1 justify-items-start">
          <span className="text-muted-foreground text-xs">
            {t("select.usedNew")}
          </span>
          <SelectValue />
        </div>
      </SelectTrigger>
      <SelectContent>
        {CAR_CONDITION_OPTIONS.map(([conditionKey]) => (
          <SelectItem key={conditionKey} value={conditionKey}>
            {t(conditionKeys[conditionKey])}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
};

export default UsedSelect;
