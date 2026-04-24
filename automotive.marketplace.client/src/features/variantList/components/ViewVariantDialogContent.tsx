import {
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useTranslation } from "react-i18next";
import { Variant } from "../types/Variant";

type ViewVariantDialogContentProps = {
  variant: Variant;
};

const ViewVariantDialogContent = ({
  variant,
}: ViewVariantDialogContentProps) => {
  const { t } = useTranslation("admin");

  return (
    <>
      <DialogHeader>
        <DialogTitle>{t("variants.variantDetails")}</DialogTitle>
        <DialogDescription>{t("variants.readOnly")}</DialogDescription>
      </DialogHeader>
      <div className="grid gap-4">
        <p>
          {t("variants.fuel")}: {variant.fuelName}
        </p>
        <p>
          {t("variants.transmission")}: {variant.transmissionName}
        </p>
        <p>
          {t("variants.bodyType")}: {variant.bodyTypeName}
        </p>
        <p>
          {t("variants.doors")}: {variant.doorCount}
        </p>
        <p>
          {t("variants.power")}: {variant.powerKw} kW
        </p>
        <p>
          {t("variants.engineSize")}: {variant.engineSizeMl} ml
        </p>
        <p>
          {t("variants.custom")}:{" "}
          {variant.isCustom ? t("variants.yes") : t("variants.no")}
        </p>
      </div>
    </>
  );
};

export default ViewVariantDialogContent;
