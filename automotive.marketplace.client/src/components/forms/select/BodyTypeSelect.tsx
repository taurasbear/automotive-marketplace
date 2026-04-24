import { getAllBodyTypesOptions } from "@/api/enum/getAllBodyTypesOptions";
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

type BodytypeSelectProps = SelectRootProps & {
  className?: string;
};

const BodyTypeSelect = ({ className, ...props }: BodytypeSelectProps) => {
  const { t, i18n } = useTranslation("common");
  const { data: bodyTypesQuery } = useQuery(getAllBodyTypesOptions);

  const bodyTypes = bodyTypesQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Select {...props}>
        <SelectTrigger className="w-full">
          <SelectValue placeholder={t("select.bodyTypes")} />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>{t("select.bodyTypes")}</SelectLabel>
            {bodyTypes.map((body) => (
              <SelectItem key={body.id} value={body.id}>
                {getTranslatedName(body.translations, i18n.language)}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default BodyTypeSelect;
