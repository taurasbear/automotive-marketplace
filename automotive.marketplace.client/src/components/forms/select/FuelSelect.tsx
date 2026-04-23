import { getAllFuelsOptions } from "@/api/enum/getAllFuelsOptions";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { getTranslatedName } from "@/lib/i18n/getTranslatedName";
import { cn } from "@/lib/utils";
import { SelectRootProps } from "@/types/ui/selectRootProps";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";

type FuelSelectProps = SelectRootProps & {
  className?: string;
};

const FuelSelect = ({ className, ...props }: FuelSelectProps) => {
  const { t, i18n } = useTranslation("common");
  const { data: fuelsQuery } = useQuery(getAllFuelsOptions);

  const fuels = fuelsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Select {...props}>
        <SelectTrigger className="w-full">
          <SelectValue placeholder={t("select.fuelTypes")} />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>{t("select.fuelTypes")}</SelectLabel>
            {fuels.map((fuel) => (
              <SelectItem key={fuel.id} value={fuel.id}>
                {getTranslatedName(fuel.translations, i18n.language)}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default FuelSelect;
