import {
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useDateLocale } from "@/lib/i18n/dateLocale";
import { useSuspenseQuery } from "@tanstack/react-query";
import { format } from "date-fns";
import { useTranslation } from "react-i18next";
import { getModelByIdOptions } from "../api/getModelByIdOptions";

type ViewModelDialogContentProps = {
  id: string;
  className?: string;
};

const ViewModelDialogContent = ({ id }: ViewModelDialogContentProps) => {
  const { t } = useTranslation("admin");
  const locale = useDateLocale();
  const { data: modelQuery } = useSuspenseQuery(getModelByIdOptions({ id }));

  const model = modelQuery.data;

  return (
    <>
      <DialogHeader>
        <DialogTitle>{t("models.modelDetails")}</DialogTitle>
        <DialogDescription>{t("models.inDepth")}</DialogDescription>
      </DialogHeader>
      <div className="grid gap-4">
        <h3>{model.name}</h3>
        <p>
          {t("models.createdBy_label")}: {model.createdBy}
        </p>
        <p>
          {t("models.createdAt_label")}:{" "}
          {format(new Date(model.createdAt), "Pp", { locale })}
        </p>
        {model.modifiedAt && (
          <p>
            {t("models.lastModifiedBy")}: {model.modifiedBy}{" "}
            {t("models.on") && `${t("models.on")} `}
            {format(new Date(model.modifiedAt), "Pp", { locale })}
          </p>
        )}
      </div>
    </>
  );
};

export default ViewModelDialogContent;
