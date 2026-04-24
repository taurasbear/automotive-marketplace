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
import { UI_CONSTANTS } from "@/constants/uiConstants";
import { cn } from "@/lib/utils";
import { SelectRootProps } from "@/types/ui/selectRootProps";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";

type MakeSelectProps = SelectRootProps & {
  isAllMakesEnabled: boolean;
  label?: string;
  className?: string;
};

const MakeSelect = ({
  isAllMakesEnabled,
  label,
  className,
  ...props
}: MakeSelectProps) => {
  const { t } = useTranslation("common");
  const { data: makesQuery } = useQuery(getAllMakesOptions);

  const makes = makesQuery?.data || [];

  return (
    <Select {...props}>
      <SelectTrigger className={cn("w-full", className)} aria-label={label}>
        <div className="grid grid-cols-1 justify-items-start">
          <span className="text-muted-foreground text-xs">{label}</span>
          <SelectValue placeholder="Toyota" />
        </div>
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          <SelectLabel>{t("select.makes")}</SelectLabel>
          {!isAllMakesEnabled || (
            <SelectItem value={UI_CONSTANTS.SELECT.ALL_MAKES.VALUE}>
              {t("select.allMakes")}
            </SelectItem>
          )}
          {makes.map((make) => (
            <SelectItem key={make.id} value={make.id}>
              {make.name}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
};

export default MakeSelect;
